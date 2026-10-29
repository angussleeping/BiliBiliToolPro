using System.Net;
using BlazingQuartz;
using BlazingQuartz.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;
using MudBlazor.Services;
using Ray.BiliBiliTool.Config.SQLite;
using Ray.BiliBiliTool.Infrastructure;
using Ray.BiliBiliTool.Infrastructure.EF;
using Ray.BiliBiliTool.Infrastructure.EF.Extensions;
using Ray.BiliBiliTool.Web.Components;
using Ray.BiliBiliTool.Web.Extensions;
using Ray.BiliBiliTool.Web.Services.Pages.BiliAccount;
using Serilog;
using Serilog.Debugging;

SelfLog.Enable(Console.Error);
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var sqliteConnStr = builder.Configuration.GetConnectionString("Sqlite");
    // Parse the DataSource via the connection-string builder instead of naive
    // Split('=')/Split(';') so values containing delimiters survive
    var sqliteDbPath = string.IsNullOrEmpty(sqliteConnStr)
        ? null
        : new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(sqliteConnStr).DataSource;
    if (!string.IsNullOrEmpty(sqliteConnStr))
    {
        builder.Configuration.AddSqlite(
            connectionString: sqliteConnStr,
            tableName: Ray.BiliBiliTool.Config.Constants.SqliteTableName,
            keyColumnName: "Key",
            valueColumnName: "Value"
        );
        BiliAccountPageWorkflow.CompactStoredAccounts(builder.Configuration);
    }

    // 启动期安全开关统一在 AddSqlite 之后读取，保证面板里存的配置与环境变量一视同仁
    var requireHttps = builder.Configuration.GetValue<bool>("Security:RequireHttps");
    var enableSwagger =
        builder.Environment.IsDevelopment()
        || builder.Configuration.GetValue<bool>("Security:EnableSwagger");

    builder
        .Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();
    builder.Services.AddControllers();

    // Persist DataProtection keys explicitly: the container points
    // Security__DataProtectionKeysDirectory at a mounted directory so the login-cookie
    // signing key survives container rebuilds (otherwise every recreate forces re-login)
    var dataProtectionKeysDir = builder.Configuration["Security:DataProtectionKeysDirectory"];
    if (!string.IsNullOrWhiteSpace(dataProtectionKeysDir))
    {
        builder
            .Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysDir));
    }

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "BiliBiliToolPro API",
                Version = "v1",
                Description = "BiliBiliToolPro的API接口文档",
                Contact = new OpenApiContact
                {
                    Name = "BiliBiliToolPro",
                    Url = new Uri("https://github.com/angussleeping/BiliBiliToolPro"),
                },
            }
        );
    });

    builder.Services.AddMudServices();

    builder.Services.AddEF();

    builder.Services.AddSerilog(
        (services, lc) =>
            lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.SQLite(
                    sqliteDbPath: sqliteDbPath,
                    tableName: "bili_logs",
                    storeTimestampInUtc: true,
                    batchSize: 7
                )
    );

    // Add BlazingQuartz
    builder.Services.Configure<BlazingQuartzUIOptions>(
        builder.Configuration.GetSection("BlazingQuartz")
    );
    builder.Services.AddBlazingQuartz();
    builder.Services.AddBiliScheduler(builder.Configuration);

    builder
        .Services.AddWebServices()
        .AddAuthServices(builder.Configuration)
        .AddCoreModuleServices(builder.Configuration);

    var app = builder.Build();

    Global.ServiceProviderRoot = app.Services;
    await app.InitializeBiliToolAsync();

    // Enable when deployed behind a reverse proxy (Nginx/Caddy/Traefik) so HTTPS and
    // the real client IP are recognized correctly
    if (builder.Configuration.GetValue<bool>("Security:UseForwardedHeaders"))
    {
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        };
        var knownProxies = builder
            .Configuration.GetSection("Security:KnownProxies")
            .Get<string[]>();
        if (knownProxies is { Length: > 0 })
        {
            // 只信任列出的反代 IP：否则任意客户端都能伪造 X-Forwarded-For，
            // 绕过基于来源 IP 的登录限流
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            foreach (var proxy in knownProxies)
            {
                if (IPAddress.TryParse(proxy, out var ip))
                {
                    forwardedHeadersOptions.KnownProxies.Add(ip);
                }
                else
                {
                    app.Logger.LogWarning(
                        "Security:KnownProxies entry '{proxy}' is not a valid IP, skipped",
                        proxy
                    );
                }
            }
        }
        else
        {
            // 未配置受信代理时保持「信任所有来源」的旧行为（容器内反代 IP 不固定），
            // 但要在日志里明确提示伪造风险
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.Logger.LogWarning(
                "Security:UseForwardedHeaders is on without Security:KnownProxies; X-Forwarded-For from any client is trusted and login throttling can be bypassed by IP spoofing"
            );
        }
        app.UseForwardedHeaders(forwardedHeadersOptions);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    // Pure-HTTP deployments (e.g. container port mapping to 22330) skip redirection to
    // avoid a warning on every request about the undeterminable HTTPS port
    if (requireHttps)
    {
        app.UseHttpsRedirection();
    }
    else
    {
        app.Logger.LogInformation("Security:RequireHttps is off, skipping HTTPS redirection");
    }

    app.UseStaticFiles();
    app.MapStaticAssets();
    app.UseAntiforgery();

    app.UseSerilogRequestLogging();

    app.MapControllers();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(Ray.BiliBiliTool.Web.Client._Imports).Assembly);

    // API docs are off in production by default; enable explicitly with
    // Security:EnableSwagger=true when needed
    if (enableSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BiliBiliToolPro API V1");
            c.RoutePrefix = "swagger";
        });
    }
    else
    {
        app.Logger.LogInformation("Security:EnableSwagger is off, skipping Swagger UI");
    }

    app.Run();
}
catch (Exception ex)
{
    // 记完日志后必须重新抛出，让进程以非 0 退出码结束。
    // 否则顶层语句正常返回、退出码为 0，Docker / 青龙 / SCF 等编排层会把
    // 「启动崩溃」当成「正常退出」：不重启、不告警，只是端口从未监听。
    // 退出码由运行时按未处理异常决定（Linux 134，Windows 0xE0434352），
    // 详见 docs/adr/0001-web-startup-failure-must-exit-nonzero.md。
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
