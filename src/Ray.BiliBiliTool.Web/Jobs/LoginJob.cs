using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.Web.Jobs;

public class LoginJob(
    ILogger<LoginJob> logger,
    ILoginTaskAppService appService,
    IOptionsMonitor<SecurityOptions> securityOptions
) : BaseJob<LoginJob>(logger, securityOptions)
{
    private readonly ILogger<LoginJob> _logger = logger;
    public static readonly JobKey Key = new(nameof(LoginJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation($"{nameof(LoginJob)} started.");
        await appService.DoTaskAsync(context.CancellationToken);
    }
}
