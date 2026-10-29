using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;
using Ray.BiliBiliTool.Web.Services;

namespace Ray.BiliBiliTool.Web.Jobs;

public class DailyJob(
    ILogger<DailyJob> logger,
    IDailyTaskAppService appService,
    ITaskFailureBatchMonitor? failureMonitor = null,
    IOptionsMonitor<SecurityOptions>? securityOptions = null
) : BaseJob<DailyJob>(logger, failureMonitor, securityOptions)
{
    public static readonly JobKey Key = new(nameof(DailyJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context) =>
        await appService.DoTaskAsync();
}
