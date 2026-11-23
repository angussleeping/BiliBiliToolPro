using System.Net.Http.Json;

namespace Ray.BiliBiliTool.Infrastructure.Helpers;

/// <summary>
/// 获取本机公网 IP 地址（异步版本）。
/// 注意：此类仅供诊断/日志用途，已接入 DI 的调用方应注入 IHttpClientFactory。
/// </summary>
public static class IpHelper
{
    private static readonly HttpClient SharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5),
    };

    /// <summary>
    /// 异步获取公网 IP；失败时返回 null，不抛异常。
    /// </summary>
    public static async Task<string?> GetIpAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ip = await SharedHttpClient.GetStringAsync("https://api.ipify.org/", cancellationToken);
            return string.IsNullOrWhiteSpace(ip) ? null : ip.Trim();
        }
        catch (HttpRequestException)
        {
            // 网络不通或 DNS 解析失败，不记录：调用方无需因诊断辅助失败而中断主流程
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }
}
