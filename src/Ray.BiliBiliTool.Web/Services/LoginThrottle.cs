using Microsoft.Extensions.Caching.Memory;

namespace Ray.BiliBiliTool.Web.Services;

/// <summary>
/// 登录失败次数限流：同一来源在窗口期内失败达到上限后暂时拒绝登录，防止暴力破解。
/// </summary>
public sealed class LoginThrottle(IMemoryCache cache)
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    public bool IsLocked(string key) => GetFailures(key) >= MaxFailures;

    public void RegisterFailure(string key) => cache.Set(key, GetFailures(key) + 1, Window);

    public void Reset(string key) => cache.Remove(key);

    private int GetFailures(string key) => cache.TryGetValue(key, out int failures) ? failures : 0;
}
