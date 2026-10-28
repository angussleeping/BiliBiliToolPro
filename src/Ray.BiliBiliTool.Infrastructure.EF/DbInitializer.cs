using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ray.BiliBiliTool.Domain;
using Ray.BiliBiliTool.Infrastructure.Helpers;

namespace Ray.BiliBiliTool.Infrastructure.EF;

public class DbInitializer(
    BiliDbContext context,
    IConfiguration configuration,
    ILogger<DbInitializer> logger
)
{
    private const string DefaultUserName = "admin";
    private const string DefaultPassword = "BiliTool@2233";

    public async Task InitializeAsync()
    {
        await context.Database.MigrateAsync();

        await InitUserAsync();
    }

    private async Task InitUserAsync()
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var password = configuration["Security:DefaultAdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            password = DefaultPassword;
            logger.LogWarning(
                "Security:DefaultAdminPassword is not configured; using the default admin password. Change it immediately after signing in"
            );
        }

        var (hash, salt) = PasswordHelper.HashPassword(password);
        var adminUser = new User
        {
            Username = DefaultUserName,
            PasswordHash = hash,
            Salt = salt,
            Roles = ["Administrator"],
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
