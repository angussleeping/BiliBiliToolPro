using Microsoft.Extensions.Options;
using Quartz;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.Web.Jobs;

public class MangaJob(
    ILogger<MangaJob> logger,
    IMangaTaskAppService appService,
    IOptionsMonitor<SecurityOptions> securityOptions
) : BaseJob<MangaJob>(logger, securityOptions)
{
    private readonly ILogger<MangaJob> _logger = logger;
    public static readonly JobKey Key = new(nameof(MangaJob), Constants.BiliJobGroup);

    protected override async Task DoExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation($"{nameof(MangaJob)} started.");
        await appService.DoTaskAsync(context.CancellationToken);
    }
}
