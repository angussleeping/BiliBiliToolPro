using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.Web.Jobs;

public class TestBiliJob(
    ILogger<TestBiliJob> logger,
    ITestAppService appService,
    IOptionsMonitor<SecurityOptions> securityOptions
) : BaseJob<TestBiliJob>(logger, securityOptions)
{
    private readonly ILogger<TestBiliJob> _logger = logger;
    public static readonly JobKey Key = new(nameof(TestBiliJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation($"{nameof(TestBiliJob)} started.");
        await appService.DoTaskAsync();
    }
}
