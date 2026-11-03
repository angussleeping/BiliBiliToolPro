using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Ray.BiliBiliTool.Agent;
using Ray.BiliBiliTool.Agent.BiliBiliAgent.Dtos;
using Ray.BiliBiliTool.Agent.BiliBiliAgent.Dtos.LiveApi;
using Ray.BiliBiliTool.Agent.BiliBiliAgent.Dtos.LiveTraceApi;
using Ray.BiliBiliTool.Agent.BiliBiliAgent.Interfaces;
using Ray.BiliBiliTool.Config.Options;
using Ray.BiliBiliTool.Domain.Exceptions;

namespace Ray.BiliBiliTool.DomainService;

public class LiveFansMedalTaskRunner(
    ILiveApi liveApi,
    ILiveTraceApi traceApi,
    ILogger logger,
    LiveFansMedalTaskOptions options,
    string userAgent,
    Func<TimeSpan, CancellationToken, Task>? delay = null
)
{
    private readonly Func<TimeSpan, CancellationToken, Task> _delay = delay ?? Task.Delay;
    private int _likesSent;

    /// <summary>已告警过的降级点，避免每轮循环重复刷同一条日志</summary>
    private readonly HashSet<string> _warned = [];

    /// <summary>心跳被 B 站拒绝后最多重新进房几次（换一组 secret_key 再试）</summary>
    private const int MaxHeartBeatRepairs = 3;

    /// <summary>任务文案解析降级时的告警：同一粉丝牌同一原因每次运行只记一条</summary>
    private LiveFansMedalPlan Plan(
        ActivatedMedalResponse data,
        string action,
        string medal,
        bool warn = true
    ) =>
        LiveFansMedalTaskPlanner.Plan(
            data,
            action,
            warn ? FallbackPropagation(medal, action) : null
        );

    private Action<string> FallbackPropagation(string medal, string action) =>
        message =>
        {
            if (_warned.Add($"{medal}|{action}|{message}"))
                logger.LogWarning(
                    "粉丝牌 {medal} 的 {action} 任务定义解析降级：{message}",
                    medal,
                    action,
                    message
                );
        };

    public async Task RunAsync(BiliCookie cookie, string action, CancellationToken token = default)
    {
        var enabled = action switch
        {
            "like" => options.EnableLike,
            "sendDanmu" => options.EnableDanmaku,
            "watchLive" => options.EnableWatch,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
        var customBudget = action switch
        {
            "like" => Math.Clamp(options.LikeNumber, 0, 5000),
            "sendDanmu" => Math.Clamp(options.SendDanmakuNumber, 0, 100),
            _ => Math.Clamp(options.HeartBeatNumber, 0, 1440) * 60,
        };
        if (!enabled || (!options.FollowDailyTaskLimit && customBudget == 0))
            return;

        var medals = await GetMedalsAsync(cookie, token);
        var failures = 0;
        var reasons = new ConcurrentQueue<string>();
        // Limit concurrent watch sessions while retaining parallel watching.
        using var watchSlots = new SemaphoreSlim(8);
        async Task ExecuteAsync(FansMedalPanelItem medal)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var data = await GetTasksAsync(cookie, medal, token);
                var plan = Plan(data, action, medal.Medal.Medal_name);
                var target = Math.Min(
                    plan.Remaining,
                    options.FollowDailyTaskLimit
                        ? action == "like"
                            ? 5000
                            : action == "sendDanmu"
                                ? 100
                                : 86400
                        : customBudget
                );
                if (action == "like")
                    target = Math.Min(target, 5000 - _likesSent);
                if (target <= 0)
                    return;

                if (action == "watchLive")
                {
                    await watchSlots.WaitAsync(token);
                    try
                    {
                        await WatchAsync(cookie, medal, target, token);
                    }
                    finally
                    {
                        watchSlots.Release();
                    }
                }
                else
                {
                    await InteractAsync(cookie, medal, data, action, target, token);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failures);
                var reason = ex is BiliBusinessException or InvalidOperationException
                    ? ex.Message
                    : ex.GetType().Name;
                reasons.Enqueue($"{medal.Medal.Medal_name}: {reason}");
                logger.LogError(
                    "粉丝牌 {medal} 的 {action} 任务失败：{reason}",
                    medal.Medal.Medal_name,
                    action,
                    reason
                );
            }
        }

        if (action == "watchLive")
            await Task.WhenAll(medals.Select(ExecuteAsync));
        else
            foreach (var medal in medals)
                await ExecuteAsync(medal);

        // 单个粉丝牌失败不再让整条任务失败：B 站对不同房间的容忍度不一样（有的房间心跳
        // 一直拿不到时长、有的灯牌任务文案在灰度），一个房间的问题不该拖垮其他房间的成果。
        // 只有「全军覆没」才判整条任务失败，部分失败按警告留痕。
        if (failures == 0)
            return;

        if (failures < medals.Count)
        {
            logger.LogWarning(
                "{failed}/{total} 个粉丝牌的 {action} 任务失败，其余已完成，本次按部分成功处理：{reasons}",
                failures,
                medals.Count,
                action,
                string.Join("；", reasons.Take(3))
            );
            return;
        }

        throw new BiliBusinessException(
            $"全部 {failures} 个粉丝牌的 {action} 任务执行失败：{string.Join("；", reasons.Take(3))}"
        );
    }

    private async Task<List<FansMedalPanelItem>> GetMedalsAsync(
        BiliCookie cookie,
        CancellationToken token
    )
    {
        var result = new Dictionary<long, FansMedalPanelItem>();
        var excluded = options.GetExcludedAnchorIds();
        var included = options.GetIncludedAnchorIds();
        for (var page = 1; page <= 1000; page++)
        {
            var data = Require(
                await liveApi.GetFansMedalPanel(page, cookie.ToString(), token),
                "获取粉丝牌列表"
            );
            foreach (var item in data.Special_list.Concat(data.List))
            {
                if (
                    item.Medal.Target_id > 0
                    && item.Room_info.Room_id > 0
                    && !excluded.Contains(item.Medal.Target_id)
                    && (!options.OnlySelectedAnchors || included.Contains(item.Medal.Target_id))
                )
                    result.TryAdd(item.Medal.Target_id, item);
            }
            if (!data.Page_info.Has_more && page >= data.Page_info.Total_page)
                return result.Values.ToList();
            await _delay(TimeSpan.FromMilliseconds(500), token);
        }
        throw new BiliBusinessException("粉丝牌列表分页未完成");
    }

    private async Task<ActivatedMedalResponse> GetTasksAsync(
        BiliCookie cookie,
        FansMedalPanelItem medal,
        CancellationToken token
    ) =>
        Require(
            await liveApi.GetActivatedMedalInfo(
                medal.Medal.Target_id,
                cookie.BiliJct,
                cookie.ToString(),
                token
            ),
            "获取粉丝牌任务进度"
        );

    private async Task<int> InteractAsync(
        BiliCookie cookie,
        FansMedalPanelItem medal,
        ActivatedMedalResponse data,
        string action,
        int target,
        CancellationToken token
    )
    {
        var sent = 0;
        var failures = 0;
        var previous = Plan(data, action, medal.Medal.Medal_name);
        while (sent < target && previous.Remaining > 0)
        {
            var roundTarget = Math.Min(
                target - sent,
                Math.Min(previous.Remaining, previous.RoundSize)
            );
            var roundSent = 0;
            while (roundSent < roundTarget)
            {
                token.ThrowIfCancellationRequested();
                var room = Require(
                    await liveApi.GetLiveRoomInfo(medal.Room_info.Room_id),
                    "获取直播状态"
                );
                if (action == "like" && room.Live_Status != 1)
                {
                    logger.LogInformation(
                        "粉丝牌 {medal} 的直播间未开播，跳过点赞（B 站只在开播期间计点赞亲密度）",
                        medal.Medal.Medal_name
                    );
                    return sent;
                }
                if (
                    action == "sendDanmu"
                    && options.DanmakuOnlyWhenOffline
                    && room.Live_Status == 1
                )
                    return sent;
                // A live room can be lit with likes without additional chat messages.
                if (
                    action == "sendDanmu"
                    && !data.Is_lighted
                    && room.Live_Status == 1
                    && options.EnableLike
                )
                    return sent;
                var amount = action == "like" ? Math.Min(10, roundTarget - roundSent) : 1;
                if (action == "like")
                {
                    amount = Math.Min(amount, 5000 - _likesSent);
                    if (amount <= 0)
                        return sent;
                }
                BiliApiResponse response;
                if (action == "like")
                    response = await liveApi.LikeLiveRoom(
                        new LikeLiveRoomRequest(
                            medal.Room_info.Room_id,
                            cookie.BiliJct,
                            amount,
                            medal.Medal.Target_id,
                            cookie.UserId
                        ).RawTextBuild(),
                        cookie.ToString()
                    );
                else
                    response = await liveApi.SendLiveDanmuku(
                        new SendLiveDanmukuRequest(
                            cookie.BiliJct,
                            medal.Room_info.Room_id,
                            options.DanmakuContent
                        )
                        {
                            Rnd = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        },
                        cookie.ToString()
                    );
                if (response.Code != 0)
                {
                    failures++;
                    var threshold =
                        action == "like"
                            ? 3
                            : Math.Clamp(options.SendDanmakugiveUpThreshold, 1, 10);
                    if (failures >= threshold)
                        throw new BiliBusinessException($"粉丝牌互动失败，错误码 {response.Code}");
                }
                else
                {
                    sent += amount;
                    roundSent += amount;
                    failures = 0;
                    if (action == "like")
                        _likesSent += amount;
                }
                await _delay(TimeSpan.FromSeconds(action == "like" ? 3 : 7), token);
            }

            var wasLighted = data.Is_lighted;
            data = await GetTasksAsync(cookie, medal, token);
            var latest = Plan(data, action, medal.Medal.Medal_name);
            LogProgress(medal, data, action);
            // A lighting task changes into a different daily task after lighting.
            if (!wasLighted && data.Is_lighted)
            {
                if (!options.FollowDailyTaskLimit)
                    return sent;
                target = Math.Min(action == "like" ? 5000 : 100, sent + latest.Remaining);
                previous = latest;
                continue;
            }
            if (roundSent >= previous.RoundSize && latest.Remaining >= previous.Remaining)
            {
                await _delay(TimeSpan.FromSeconds(3), token);
                data = await GetTasksAsync(cookie, medal, token);
                latest = Plan(data, action, medal.Medal.Medal_name);
                if (latest.Remaining >= previous.Remaining)
                    throw new BiliBusinessException("互动已发送，B 站任务进度尚未更新");
            }
            previous = latest;
        }
        return sent;
    }

    /// <summary>
    /// 挂机看直播。
    ///
    /// <para>
    /// 心跳（<c>/xlive/data-interface/v1/x25Kn/X</c>）会校验 <c>ts</c>/<c>ets</c> 的时间差，
    /// 对不上时 B 站直接回 <c>1012002 time check failed</c>。所以心跳必须紧跟在 sleep 之后发出，
    /// 中间不能插任何 HTTP 请求 —— 旧实现是先 sleep、再查一次直播间状态、最后才心跳，
    /// 多出来的那一次往返正好把时间差顶出容差窗口，于是「时好时坏」。
    /// </para>
    /// <para>
    /// 另外，一旦心跳被拒，继续拿旧的 <c>secret_key</c> 重试只会越错越多（时间差越拉越大），
    /// 正确做法是重新进房换一组心跳参数。
    /// </para>
    /// </summary>
    private async Task WatchAsync(
        BiliCookie cookie,
        FansMedalPanelItem medal,
        int target,
        CancellationToken token
    )
    {
        var roomId = medal.Room_info.Room_id;
        var uuid = Guid.NewGuid().ToString();
        // device 里的 uuid 必须整场稳定，重进房也不能换
        var device = $"[\"{cookie.LiveBuvid}\",\"{uuid}\"]";

        var state = await EnterRoomAsync(cookie, medal, device, token);
        if (state is null)
        {
            logger.LogInformation(
                "粉丝牌 {medal} 的直播间当前未开播，跳过挂机（B 站只在开播期间累计观看时长）",
                medal.Medal.Medal_name
            );
            return;
        }

        var watched = 0;
        var sequence = 1;
        var failures = 0;
        var repairs = 0;
        var lastCheck = 0;
        var lastCode = 0;
        var lastRemaining = int.MaxValue;
        var stalls = 0;
        var giveUp = Math.Clamp(options.HeartBeatSendGiveUpThreshold, 1, 10);

        while (watched < target)
        {
            token.ThrowIfCancellationRequested();

            if (!IsUsableHeartBeat(state))
            {
                // 心跳参数不可用：重进房换一组，换不到就放弃这个房间
                if (repairs >= MaxHeartBeatRepairs)
                    throw new BiliBusinessException("直播观看任务返回了无效的心跳参数");
                repairs++;
                var repaired = await EnterRoomAsync(cookie, medal, device, token);
                if (repaired is null)
                {
                    logger.LogInformation(
                        "粉丝牌 {medal} 的直播间已下播，停止挂机（已观看 {watched}/{target} 秒）",
                        medal.Medal.Medal_name,
                        watched,
                        target
                    );
                    return;
                }
                state = repaired;
                sequence = 1;
                continue;
            }

            var interval = state.Heartbeat_interval;

            // 房间是否还在播：放在 sleep 之前查，绝不能挡在心跳和 sleep 之间
            var room = Require(await liveApi.GetLiveRoomInfo(roomId), "获取直播状态");
            if (room.Live_Status != 1 || room.Parent_area_id <= 0)
            {
                logger.LogInformation(
                    "粉丝牌 {medal} 的直播间已下播或分区信息不可用，停止挂机（已观看 {watched}/{target} 秒）",
                    medal.Medal.Medal_name,
                    watched,
                    target
                );
                return;
            }

            await _delay(TimeSpan.FromSeconds(interval), token);

            var response = await traceApi.HeartBeat(
                new HeartBeatRequest(
                    roomId,
                    room.Parent_area_id,
                    room.Area_id,
                    sequence,
                    cookie.LiveBuvid,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    state.Timestamp,
                    userAgent,
                    state.Secret_rule,
                    state.Secret_key!,
                    cookie.BiliJct,
                    uuid,
                    device,
                    interval,
                    medal.Medal.Target_id
                ),
                cookie.ToString()
            );

            if (response.Code != 0 || response.Data is null)
            {
                lastCode = response.Code;
                failures++;
                if (failures >= giveUp)
                    throw new BiliBusinessException(
                        $"直播观看心跳失败，错误码 {lastCode}（连续 {failures} 次）"
                    );

                if (repairs < MaxHeartBeatRepairs)
                {
                    repairs++;
                    logger.LogWarning(
                        "粉丝牌 {medal} 的心跳被 B 站拒绝（错误码 {code}），第 {n} 次重新进房刷新心跳参数",
                        medal.Medal.Medal_name,
                        lastCode,
                        repairs
                    );
                    var repaired = await EnterRoomAsync(cookie, medal, device, token);
                    if (repaired is null)
                        return;
                    state = repaired;
                    sequence = 1;
                }
                continue;
            }

            state = response.Data;
            failures = 0;
            repairs = 0;
            sequence++;
            watched += interval;

            if (watched - lastCheck < 60 && watched < target)
                continue;

            var data = await GetTasksAsync(cookie, medal, token);
            var remaining = Plan(data, "watchLive", medal.Medal.Medal_name, warn: false).Remaining;
            LogProgress(medal, data, "watchLive");
            lastCheck = watched;
            if (remaining == 0)
                return;

            if (remaining >= lastRemaining)
            {
                // 心跳返回成功、时长却没被计入：多半已经被风控静默丢弃，留一条线索便于以后排查
                if (++stalls == 3)
                    logger.LogWarning(
                        "粉丝牌 {medal} 的心跳连续 3 次未被计入观看时长（当前 {progress}），继续挂机可能白跑",
                        medal.Medal.Medal_name,
                        data.Task_info.FirstOrDefault(item =>
                            item.Jump_type == "watchLive"
                        )?.Sub_title
                    );
            }
            else
            {
                stalls = 0;
            }
            lastRemaining = remaining;

            if (options.FollowDailyTaskLimit && watched >= target)
            {
                await _delay(TimeSpan.FromSeconds(3), token);
                data = await GetTasksAsync(cookie, medal, token);
                if (Plan(data, "watchLive", medal.Medal.Medal_name, warn: false).Remaining > 0)
                    throw new BiliBusinessException("观看时长已达到目标，B 站任务尚未完成");
            }
        }
    }

    /// <summary>进房拿一组心跳参数；房间不在播（或是轮播）时返回 null</summary>
    private async Task<HeartBeatResponse?> EnterRoomAsync(
        BiliCookie cookie,
        FansMedalPanelItem medal,
        string device,
        CancellationToken token
    )
    {
        var roomId = medal.Room_info.Room_id;
        var room = Require(await liveApi.GetLiveRoomInfo(roomId), "获取直播状态");
        if (room.Live_Status != 1 || room.Parent_area_id <= 0)
            return null;

        return Require(
            await traceApi.EnterRoom(
                new EnterRoomRequest(
                    roomId,
                    room.Parent_area_id,
                    room.Area_id,
                    0,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    userAgent,
                    cookie.BiliJct,
                    medal.Medal.Target_id,
                    device
                ),
                cookie.ToString()
            ),
            "进入直播间"
        );
    }

    private static bool IsUsableHeartBeat(HeartBeatResponse state) =>
        state.Heartbeat_interval > 0
        && state.Heartbeat_interval <= 300
        && !string.IsNullOrEmpty(state.Secret_key);

    private void LogProgress(FansMedalPanelItem medal, ActivatedMedalResponse data, string action)
    {
        var task = data.Task_info.FirstOrDefault(item => item.Jump_type == action);
        logger.LogInformation(
            "粉丝牌 {medal} 的任务进度：{progress}，完成状态：{done}",
            medal.Medal.Medal_name,
            task?.Sub_title ?? "",
            task?.Is_done ?? false
        );
    }

    private static T Require<T>(BiliApiResponse<T> response, string operation)
        where T : class
    {
        if (response.Code != 0 || response.Data is null)
            throw new BiliBusinessException($"{operation}失败，错误码 {response.Code}");
        return response.Data;
    }
}
