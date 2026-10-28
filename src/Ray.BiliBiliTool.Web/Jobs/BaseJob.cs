using System.Reflection;
using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Config.Options;
using Ray.BiliBiliTool.Web.Services;
using Ray.Serilog.Sinks.Batched;
using Serilog.Context;

namespace Ray.BiliBiliTool.Web.Jobs;

public abstract class BaseJob<TJob>(
    ILogger<TJob> logger,
    ITaskFailureBatchMonitor? failureMonitor = null,
    IOptionsMonitor<SecurityOptions>? securityOptions = null
) : IJob
    where TJob : BaseJob<TJob>
{
    protected ILogger<TJob> Logger { get; } = logger;

    public async ValueTask Execute(
        IJobExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        // Quartz creates a one-off simple trigger for the panel's run-now action.
        using var notificationScope = new TaskFailureNotificationScope(
            suppress: context.Trigger is not ICronTrigger
        );
        using var failureBatch = TaskFailureNotificationScope.IsSuppressed
            ? null
            : failureMonitor?.BeginBatch();
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
                logger.LogInformation($"{typeof(TJob).Name} started.");
                await RandomSleepAsync(context, cancellationToken);
                await DoExecuteAsync(context);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                var taskKey = TaskFailureNotificationCatalog.TaskKeyForJob(typeof(TJob).Name);
                if (
                    !TaskFailureNotificationScope.IsSuppressed
                    && failureMonitor is not null
                    && taskKey is not null
                )
                {
                    try
                    {
                        await failureMonitor.RecordFailureAsync(null, taskKey, cancellationToken);
                    }
                    catch (Exception)
                    {
                        logger.LogWarning("任务失败汇总记录暂时无法保存");
                    }
                }
            }
            finally
            {
                logger.LogInformation("---");
                logger.LogInformation(
                    "{version} 开源 by {url}",
                    Config.AppVersion.DisplayOf(typeof(Program).Assembly),
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
    /// Random delay before a scheduled task runs, so requests do not hit Bilibili at the
    /// exact same time every day. Only applies to cron triggers; manual runs from the
    /// panel and the login/test jobs are not delayed.
    /// </summary>
    private async Task RandomSleepAsync(
        IJobExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.Trigger is not ICronTrigger)
            return;

        var jobName = typeof(TJob).Name;
        if (jobName == nameof(LoginJob) || jobName == nameof(TestBiliJob))
            return;

        var maxSec = securityOptions?.CurrentValue.RandomSleepMaxSec ?? 0;
        if (maxSec <= 0)
            return;

        var seconds = Random.Shared.Next(1, maxSec + 1);
        logger.LogInformation(
            "Random delay of {seconds} seconds before execution (cap {maxSec} seconds)",
            seconds,
            maxSec
        );
        await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
    }

    protected abstract Task DoExecuteAsync(IJobExecutionContext context);
}
