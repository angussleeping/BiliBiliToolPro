using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.Web.Jobs;

public class VipBigPointJob(
    ILogger<VipBigPointJob> logger,
    IVipBigPointAppService appService,
    IOptionsMonitor<SecurityOptions> securityOptions
) : BaseJob<VipBigPointJob>(logger, securityOptions)
{
    private readonly ILogger<VipBigPointJob> _logger = logger;
    public static readonly JobKey Key = new(nameof(VipBigPointJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation($"{nameof(VipBigPointJob)} started.");
        await appService.DoTaskAsync(context.CancellationToken);
    }
}
