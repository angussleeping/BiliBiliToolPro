using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.Web.Jobs;

public class MangaPrivilegeJob(
    ILogger<MangaPrivilegeJob> logger,
    IMangaPrivilegeTaskAppService appService,
    IOptionsMonitor<SecurityOptions> securityOptions
) : BaseJob<MangaPrivilegeJob>(logger, securityOptions)
{
    private readonly ILogger<MangaPrivilegeJob> _logger = logger;
    public static readonly JobKey Key = new(nameof(MangaPrivilegeJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation($"{nameof(MangaPrivilegeJob)} started.");
        await appService.DoTaskAsync();
    }
}
