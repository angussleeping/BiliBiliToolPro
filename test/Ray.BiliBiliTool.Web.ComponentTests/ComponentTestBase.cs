using Bunit;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ray.BiliBiliTool.Web.ComponentTests;

/// <summary>
/// Base class for component tests.
/// Bridges bUnit 2.x's IAsyncDisposable to xUnit 2.x's IAsyncLifetime so that
/// services implementing IAsyncDisposable (e.g. MudBlazor's KeyInterceptorService)
/// are disposed asynchronously instead of failing the synchronous container dispose.
/// </summary>
public abstract class ComponentTestBase : BunitContext, IAsyncLifetime
{
    protected ComponentTestBase()
    {
        // Login 页注入了 IAntiforgery / IHttpContextAccessor；bUnit 中 HttpContext 为 null，
        // antiforgery 永远不会被实际调用，注册真实服务只为满足 DI
        Services.AddAntiforgery();
        Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsyncCore();
}
