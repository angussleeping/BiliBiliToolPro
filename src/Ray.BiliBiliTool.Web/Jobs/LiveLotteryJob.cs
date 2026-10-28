using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.Web.Jobs;

public class LiveLotteryJob(
    ILogger<LiveLotteryJob> logger,
    ILiveLotteryTaskAppService appService,
    IOptionsMonitor<SecurityOptions> securityOptions
) : BaseJob<LiveLotteryJob>(logger, securityOptions)
{
    private readonly ILogger<LiveLotteryJob> _logger = logger;
    public static readonly JobKey Key = new(nameof(LiveLotteryJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation($"{nameof(LiveLotteryJob)} started.");
        await appService.DoTaskAsync();
    }
}
