using BlazingQuartz;
using BlazingQuartz.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using MudBlazor.Services;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Ray.BiliBiliTool.Agent.Extensions;
using Ray.BiliBiliTool.Application.Extensions;
using Ray.BiliBiliTool.Config.Extensions;
using Ray.BiliBiliTool.Config.SQLite;
using Ray.BiliBiliTool.DomainService.Extensions;
using Ray.BiliBiliTool.Infrastructure;
using Ray.BiliBiliTool.Infrastructure.EF;
using Ray.BiliBiliTool.Infrastructure.EF.Extensions;
using Ray.BiliBiliTool.Web.Components;
using Ray.BiliBiliTool.Web.Extensions;
using Serilog;
using Serilog.Debugging;

SelfLog.Enable(Console.Error);
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile("config/cookies.json", optional: true, reloadOnChange: true);
    var requireHttps = builder.Configuration.GetValue<bool>("Security:RequireHttps");
    var enableSwagger =
        builder.Environment.IsDevelopment()
        || builder.Configuration.GetValue<bool>("Security:EnableSwagger");
    var sqliteConnStr = builder.Configuration.GetConnectionString("Sqlite");
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
    }

    builder
        .Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();
    builder.Services.AddControllers();

    // 显式持久化 DataProtection 密钥：容器内由 Security__DataProtectionKeysDirectory 指向挂载目录，
    // 避免容器重建后登录 Cookie 的签名密钥变化，导致必须重新登录
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

    builder.Services.AddQuartz(q =>
    {
        q.UsePersistentStore(storeOptions =>
        {
            storeOptions.UseMicrosoftSQLite(sqlLiteOptions =>
            {
                sqlLiteOptions.UseDriverDelegate<SQLiteDelegate>();
                sqlLiteOptions.ConnectionString =
                    sqliteConnStr ?? throw new InvalidOperationException();
                sqlLiteOptions.TablePrefix = "QRTZ_";
            });
            storeOptions.UseSystemTextJsonSerializer();
        });

        q.AddBiliJobs(builder.Configuration);
    });
    builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    builder
        .Services.AddWebServices()
        .AddAuthServices(builder.Configuration)
        .AddAppServices()
        .AddDomainServices()
        .AddBiliBiliConfigs(builder.Configuration)
        .AddBiliBiliClientApi(builder.Configuration);

    var app = builder.Build();

    Global.ServiceProviderRoot = app.Services;

    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.InitializeAsync();

    // 反向代理（Nginx/Caddy/Traefik 等）后面部署时开启，才能正确识别 HTTPS 与真实客户端 IP
    if (builder.Configuration.GetValue<bool>("Security:UseForwardedHeaders"))
    {
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        };
        forwardedHeadersOptions.KnownNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
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

    // 纯 HTTP 部署（如容器映射 22330 端口）不启用重定向，避免每次请求都因无法确定 HTTPS 端口而告警
    if (requireHttps)
    {
        app.UseHttpsRedirection();
    }
    else
    {
        app.Logger.LogInformation("Security:RequireHttps 未开启，跳过 HTTPS 重定向");
    }

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.UseSerilogRequestLogging();

    app.MapControllers();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(Ray.BiliBiliTool.Web.Client._Imports).Assembly);

    // 生产环境默认关闭 API 文档，需要时通过 Security:EnableSwagger=true 显式开启
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
        app.Logger.LogInformation("Security:EnableSwagger 未开启，跳过 Swagger 文档");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}
