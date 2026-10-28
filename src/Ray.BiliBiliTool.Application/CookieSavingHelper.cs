using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ray.BiliBiliTool.Agent;
using Ray.BiliBiliTool.DomainService.Interfaces;
using Ray.BiliBiliTool.Infrastructure.Enums;

namespace Ray.BiliBiliTool.Application;

/// <summary>
/// Cookie 持久化的统一入口：青龙写入环境变量，其余平台写入 cookies.json
/// </summary>
internal static class CookieSavingHelper
{
    public static async Task SaveAsync(
        IConfiguration configuration,
        ILogger logger,
        ILoginDomainService loginDomainService,
        BiliCookie ckInfo,
        CancellationToken cancellationToken = default
    )
    {
        var platformType = configuration.GetSection("PlatformType").Get<PlatformType>();
        logger.LogInformation("当前运行平台：{platform}", platformType);

        if (platformType == PlatformType.QingLong)
        {
            await loginDomainService.SaveCookieToQinLongAsync(ckInfo, cancellationToken);
            return;
        }

        await loginDomainService.SaveCookieToJsonFileAsync(ckInfo, cancellationToken);
    }
}
