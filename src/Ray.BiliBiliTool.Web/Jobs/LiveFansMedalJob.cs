using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.Web.Jobs;

public class LiveFansMedalJob(
    ILogger<LiveFansMedalJob> logger,
    ILiveFansMedalAppService appService,
    IOptionsMonitor<SecurityOptions> securityOptions
) : BaseJob<LiveFansMedalJob>(logger, securityOptions)
{
    private readonly ILogger<LiveFansMedalJob> _logger = logger;
    public static readonly JobKey Key = new(nameof(LiveFansMedalJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation($"{nameof(LiveFansMedalJob)} started.");
        await appService.DoTaskAsync(context.CancellationToken);
    }
}
