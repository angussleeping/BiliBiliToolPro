using Microsoft.Extensions.Caching.Memory;

namespace Ray.BiliBiliTool.Web.Services;

/// <summary>
/// Login failure rate limiting: after the failure limit is reached within the window,
/// login attempts from the same source are temporarily rejected to prevent brute force.
/// </summary>
public sealed class LoginThrottle(IMemoryCache cache)
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    // IMemoryCache 的读+写不是原子操作，并发失败会互相覆盖计数，加锁保证计数准确
    private readonly object _sync = new();

    public bool IsLocked(string key) => GetFailures(key) >= MaxFailures;

    public void RegisterFailure(string key)
    {
        lock (_sync)
        {
            cache.Set(key, GetFailures(key) + 1, Window);
        }
    }

    public void Reset(string key) => cache.Remove(key);

    private int GetFailures(string key) => cache.TryGetValue(key, out int failures) ? failures : 0;
}
