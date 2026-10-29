using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Ray.BiliBiliTool.Agent.Extensions;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.Application.Extensions;
using Ray.BiliBiliTool.Config.Extensions;
using Ray.BiliBiliTool.DomainService.Extensions;
using Ray.BiliBiliTool.Infrastructure.Extensions;
using Ray.BiliBiliTool.Web.Auth;
using Ray.BiliBiliTool.Web.Services;
using Ray.BiliBiliTool.Web.Services.Pages.Admin;
using Ray.BiliBiliTool.Web.Services.Pages.BiliAccount;
using Ray.BiliBiliTool.Web.Services.Pages.Configs;
using Ray.BiliBiliTool.Web.Services.Pages.Login;
using Ray.BiliBiliTool.Web.Services.Pages.Schedules;

namespace Ray.BiliBiliTool.Web.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILoginPageStateFactory, LoginPageStateFactory>();
        services.AddScoped<IAdminPageWorkflow, AdminPageWorkflow>();
        services.AddScoped<ISchedulerPageWorkflow, SchedulerPageWorkflow>();
        services.AddScoped<ILogsDialogWorkflow, LogsDialogWorkflow>();
        services.AddScoped<IHistoryDialogWorkflow, HistoryDialogWorkflow>();
        services.AddScoped<IBiliAccountPageWorkflow, BiliAccountPageWorkflow>();
        services.AddScoped<
            ICookieNotificationSettingsWorkflow,
            CookieNotificationSettingsWorkflow
        >();
        services.AddScoped<INotificationSettingsWorkflow, NotificationSettingsWorkflow>();
        services.AddMemoryCache();
        services.AddSingleton<LoginThrottle>();
        services.AddScoped<ILiveMedalDashboardService, LiveMedalDashboardService>();

        // 应用版本：宿主程序集元数据，进程内不变，单例即可
        services.AddSingleton<IAppInfoProvider, AppInfoProvider>();

        // 「今日任务」相关
        services.AddSingleton<ITaskRecordWriter, TaskRecordWriter>();
        services.AddSingleton<ITaskFailureBatchMonitor, TaskFailureBatchMonitor>();
        services.AddHostedService<TaskFailureNotificationWorker>();
        services.AddScoped<
            ITaskFailureNotificationSettingsWorkflow,
            TaskFailureNotificationSettingsWorkflow
        >();
        services.AddSingleton<IBiliAccountProbe, BiliAccountProbe>();
        services.AddScoped<TaskRecoveryExecutor>();
        services.AddScoped<ITodayTaskService, TodayTaskService>();

        return services;
    }

    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
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
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                // 显式安全标志（HttpOnly/Lax 其实已是框架默认，写出来防止以后被默认值变动波及）；
                // Secure=Always 只在强制 HTTPS 时开启，纯 HTTP 部署下 Secure cookie 浏览器根本不会回传
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                if (configuration.GetValue<bool>("Security:RequireHttps"))
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            });
        services.AddHttpContextAccessor();
        services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

        return services;
    }

    public static IServiceCollection AddCoreModuleServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddBiliBiliConfigs(configuration)
            .AddBiliBiliClientApi(configuration)
            .AddDomainServices()
            .AddCookieMonitoring()
            .AddAppServices();
    }
}
