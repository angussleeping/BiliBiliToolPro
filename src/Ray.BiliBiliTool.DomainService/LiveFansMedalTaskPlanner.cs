using System.Globalization;
using System.Text.RegularExpressions;
using Ray.BiliBiliTool.Agent.BiliBiliAgent.Dtos.LiveApi;

namespace Ray.BiliBiliTool.DomainService;

public record LiveFansMedalPlan(int Remaining, int RoundSize);

/// <summary>
/// 把 B 站返回的粉丝牌任务定义翻译成「还需要做多少」。
///
/// <para>
/// B 站的 <c>title</c> / <c>sub_title</c> 是给人看的文案，不是稳定的接口字段，随时可能改版。
/// 4.2.x 的实现遇到解析不出来就直接抛异常，结果弹幕任务的文案一旦不带数字，整条链路在发第一条
/// 弹幕之前就挂了——实测 24 天里 165 次异常、一条弹幕都没发出去过。
/// </para>
/// <para>
/// 现在的策略是：能解析就按 B 站的要求做；解析不出来就降级成「按一轮算」并通过
/// <c>onFallback</c> 回调告警，宁可少做一点，也不要让一句文案改版毁掉整天的任务。
/// </para>
/// </summary>
public static class LiveFansMedalTaskPlanner
{
    /// <summary>单轮需求量上限，超出认为文案异常（例如把时间戳当成了数量）</summary>
    private const decimal MaxSupportedQuantity = 1440;

    /// <summary>每日进度上限，超出认为文案异常</summary>
    private const decimal MaxSupportedLimit = 1000;

    private static readonly Regex Number = new(
        @"\d+(?:\.\d+)?",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );
    private static readonly Regex Progress = new(
        @"(\d+(?:\.\d+)?)\s*/\s*(\d+(?:\.\d+)?)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    /// <summary>
    /// 计算剩余工作量。
    /// </summary>
    /// <param name="data">B 站返回的粉丝牌任务信息</param>
    /// <param name="action">like / sendDanmu / watchLive</param>
    /// <param name="onFallback">解析降级时的告警回调（由调用方接日志），可为 null</param>
    public static LiveFansMedalPlan Plan(
        ActivatedMedalResponse data,
        string action,
        Action<string>? onFallback = null
    )
    {
        var task = data.Task_info.FirstOrDefault(item => item.Jump_type == action);
        if (task is null || task.Is_done || (data.Is_lighted && data.Reach_free_intimacy_limit))
            return new(0, 0);

        var quantity = ParseQuantity(task.Title);
        if (quantity is null)
        {
            quantity = DefaultQuantity(action);
            onFallback?.Invoke(
                $"任务文案「{Shorten(task.Title)}」里没有可用的数量，本轮按 {quantity} 计算"
            );
        }

        var (current, limit) = ParseProgress(task.Sub_title);
        if (limit <= 0)
        {
            // 没有可信的进度：按「做完一轮即可」处理，与未点亮灯牌时的语义一致
            onFallback?.Invoke($"进度文案「{Shorten(task.Sub_title)}」无法解析，按 0/1 计算");
            current = 0;
            limit = 1;
        }

        // 观看时长以秒计，其他互动以次数计
        var multiplier = action == "watchLive" ? 60 : 1;
        var round = (int)decimal.Ceiling(quantity.Value * multiplier);
        var remaining = (int)decimal.Ceiling(Math.Max(0, limit - current) * round);
        return new(remaining, round);
    }

    /// <summary>文案解析不出来时一轮按多少算：点赞按接口单次上限，弹幕/观看按 1</summary>
    private static decimal DefaultQuantity(string action) =>
        action switch
        {
            "like" => 10,
            _ => 1,
        };

    private static decimal? ParseQuantity(string title)
    {
        var match = Number.Match(title);
        if (
            !match.Success
            || !decimal.TryParse(
                match.Value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var size
            )
            || size <= 0
            || size > MaxSupportedQuantity
        )
            return null;
        return size;
    }

    private static (decimal Current, decimal Limit) ParseProgress(string subTitle)
    {
        var match = Progress.Match(subTitle);
        if (!match.Success)
            return (0, 0);

        if (
            !decimal.TryParse(
                match.Groups[1].Value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var current
            )
            || !decimal.TryParse(
                match.Groups[2].Value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var limit
            )
        )
            return (0, 0);

        return limit > MaxSupportedLimit ? (0, 0) : (current, limit);
    }

    private static string Shorten(string value) =>
        string.IsNullOrWhiteSpace(value) ? "(空)"
        : value.Length <= 30 ? value
        : value[..30] + "…";
}
