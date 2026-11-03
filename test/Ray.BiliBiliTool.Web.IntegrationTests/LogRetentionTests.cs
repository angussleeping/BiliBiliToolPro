using BlazingQuartz.Core.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Ray.BiliBiliTool.Domain;
using Ray.BiliBiliTool.Infrastructure.EF;
using Ray.BiliBiliTool.Web.Services;
using Xunit;

namespace Ray.BiliBiliTool.Web.IntegrationTests;

/// <summary>
/// 日志保留策略的回归测试。
///
/// 4.2.x 的清理任务用原生 SQL 删 <c>bili_execution_logs</c>，列名照搬上游的 snake_case，
/// 而我们的表是 EF 生成的 PascalCase，于是每天凌晨都报 <c>no such column: date_added_utc</c>，
/// 连续 24 天一次都没删成功。这几个测试就是为了让这种「静默失败」不会再回来。
/// </summary>
public class LogRetentionTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(
        AppContext.BaseDirectory,
        $"bilitool-logretention-{Guid.NewGuid():N}.db"
    );
    private readonly ServiceProvider _provider;
    private readonly IDbContextFactory<BiliDbContext> _factory;

    public LogRetentionTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Sqlite"] = $"Data Source={_dbPath};Cache=Shared",
                }
            )
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddDbContextFactory<BiliDbContext>();
        _provider = services.BuildServiceProvider();
        _factory = _provider.GetRequiredService<IDbContextFactory<BiliDbContext>>();

        using var db = _factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _provider.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public async Task DeleteLogsByDays_RemovesOnlyExpiredExecutionLogs()
    {
        await SeedExecutionLogsAsync();

        await using var db = await _factory.CreateDbContextAsync();
        var store = new ExecutionLogStore(NullLogger<ExecutionLogStore>.Instance, db);

        var deleted = await store.DeleteLogsByDays(21);

        Assert.Equal(2, deleted);
        var remaining = await db.ExecutionLogs.Select(log => log.RunInstanceId).ToListAsync();
        Assert.Equal("recent", Assert.Single(remaining));
    }

    [Fact]
    public async Task CleanupExpiredLogsAsync_RemovesOnlyExpiredRunLogs()
    {
        await using (var seed = await _factory.CreateDbContextAsync())
        {
            seed.BiliLogs.AddRange(
                Log("recent", DateTime.UtcNow.AddHours(-1)),
                Log("old", DateTime.UtcNow.AddDays(-10)),
                Log("older", DateTime.UtcNow.AddDays(-30))
            );
            await seed.SaveChangesAsync();
        }

        var service = new TodayTaskService(
            null!,
            new ConfigurationBuilder().Build(),
            _factory,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            NullLogger<TodayTaskService>.Instance
        );

        await service.CleanupExpiredLogsAsync(7);

        await using var db = await _factory.CreateDbContextAsync();
        var remaining = await db.BiliLogs.Select(log => log.Level).ToListAsync();
        Assert.Equal("Information", Assert.Single(remaining));
    }

    private async Task SeedExecutionLogsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.ExecutionLogs.AddRange(
            Entry("recent", DateTimeOffset.UtcNow.AddDays(-1)),
            Entry("old", DateTimeOffset.UtcNow.AddDays(-30)),
            Entry("older", DateTimeOffset.UtcNow.AddDays(-90))
        );
        await db.SaveChangesAsync();
    }

    private static ExecutionLog Entry(string runInstanceId, DateTimeOffset dateAdded) =>
        new()
        {
            RunInstanceId = runInstanceId,
            LogType = LogType.ScheduleJob,
            JobName = "TestJob",
            JobGroup = "BiliJob",
            DateAddedUtc = dateAdded,
        };

    private static BiliLogs Log(string message, DateTime timestamp) =>
        new()
        {
            Timestamp = timestamp,
            Level = "Information",
            RenderedMessage = message,
        };
}
