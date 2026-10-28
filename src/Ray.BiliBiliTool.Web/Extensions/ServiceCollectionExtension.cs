using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ray.BiliBiliTool.Web.Auth;
using Ray.BiliBiliTool.Web.Services;

namespace Ray.BiliBiliTool.Web.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<LoginThrottle>();

        return services;
    }

    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var requireHttps = configuration.GetValue<bool>("Security:RequireHttps");

        services.AddAuthenticationCore();
        services.AddAuthorizationCore();
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "BiliToolWebAuth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                // 开启 Security:RequireHttps 后，Cookie 只通过 HTTPS 传输，避免明文泄露
                options.Cookie.SecurePolicy = requireHttps
                    ? CookieSecurePolicy.Always
                    : CookieSecurePolicy.SameAsRequest;
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });
        services.AddHttpContextAccessor();
        services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

        return services;
    }
}
