using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.Web.Jobs;

public class UnfollowBatchedJob(
    ILogger<UnfollowBatchedJob> logger,
    IUnfollowBatchedTaskAppService appService,
    IOptionsMonitor<SecurityOptions> securityOptions
) : BaseJob<UnfollowBatchedJob>(logger, securityOptions)
{
    private readonly ILogger<UnfollowBatchedJob> _logger = logger;
    public static readonly JobKey Key = new(nameof(UnfollowBatchedJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation($"{nameof(UnfollowBatchedJob)} started.");
        await appService.DoTaskAsync();
    }
}
