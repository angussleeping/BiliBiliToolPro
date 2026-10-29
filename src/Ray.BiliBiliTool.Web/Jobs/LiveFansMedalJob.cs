using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;
using Ray.BiliBiliTool.Web.Services;

namespace Ray.BiliBiliTool.Web.Jobs;

public class LiveFansMedalJob(
    ILogger<LiveFansMedalJob> logger,
    ILiveFansMedalAppService appService,
    ITaskFailureBatchMonitor? failureMonitor = null,
    IOptionsMonitor<SecurityOptions>? securityOptions = null
) : BaseJob<LiveFansMedalJob>(logger, failureMonitor, securityOptions)
{
    public static readonly JobKey Key = new(nameof(LiveFansMedalJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context) =>
        await appService.DoTaskAsync();
}
