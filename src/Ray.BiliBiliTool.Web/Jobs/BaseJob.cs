using System.Reflection;
using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Config.Options;
using Ray.Serilog.Sinks.Batched;
using Serilog.Context;

namespace Ray.BiliBiliTool.Web.Jobs;

public abstract class BaseJob<TJob>(
    ILogger<TJob> logger,
    IOptionsMonitor<SecurityOptions> securityOptions
) : IJob
    where TJob : BaseJob<TJob>
{
    public async Task Execute(IJobExecutionContext context)
    {
        var fireInstanceId = context.FireInstanceId;

        using (LogContext.PushProperty("FireInstanceId", fireInstanceId))
        using (
            LogContext.PushProperty(
                Ray.Serilog.Sinks.Batched.Constants.GroupPropertyKey,
                fireInstanceId
            )
        )
        {
            try
            {
                await RandomSleepAsync(context);
                await DoExecuteAsync(context);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            finally
            {
                logger.LogInformation("---");
                logger.LogInformation(
                    "v{version} 开源 by {url}",
                    typeof(Program).Assembly.GetName().Version?.ToString(),
                    Config.Constants.SourceCodeUrl + Environment.NewLine
                );
            }
        }

        try
        {
            await BatchSinkManager.FlushAsync(fireInstanceId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Fail to push logs");
        }
    }

    /// <summary>
    /// 定时任务执行前随机延时，避免每天准点在固定时间请求。
    /// 仅对定时触发（Cron）生效，页面上的手动运行不延时；登录和测试任务也不延时。
    /// </summary>
    private async Task RandomSleepAsync(IJobExecutionContext context)
    {
        if (context.Trigger is not ICronTrigger)
            return;

        var jobName = context.JobDetail.JobType.Name;
        if (jobName == nameof(LoginJob) || jobName == nameof(TestBiliJob))
            return;

        var maxSec = securityOptions.CurrentValue.RandomSleepMaxSec;
        if (maxSec <= 0)
            return;

        var seconds = Random.Shared.Next(1, maxSec + 1);
        logger.LogInformation(
            "随机延时 {seconds} 秒后开始执行（上限 {maxSec} 秒）",
            seconds,
            maxSec
        );
        await Task.Delay(TimeSpan.FromSeconds(seconds), context.CancellationToken);
    }

    protected abstract Task DoExecuteAsync(IJobExecutionContext context);
}
