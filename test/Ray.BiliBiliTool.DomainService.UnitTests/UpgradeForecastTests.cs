using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ray.BiliBiliTool.Agent.BiliBiliAgent.Dtos.NavApi;
using Ray.BiliBiliTool.Config.Options;
using Ray.BiliBiliTool.DomainService;

namespace Ray.BiliBiliTool.DomainService.UnitTests;

public class UpgradeForecastTests
{
    [Theory]
    [InlineData(0, 100, 100, true, true, 7)]
    [InlineData(5, 0, 100, true, true, 7)]
    [InlineData(5, 0, 2, false, false, 8)]
    [InlineData(1, 0, 0, false, false, 7)]
    [InlineData(5, 7.9, 7, false, false, 7)]
    [InlineData(0, 0, 0, false, false, 20)]
    public void UpgradeForecast_RespectsProtectionAndEnabledTasks(
        int coins,
        double balance,
        int protectedCoins,
        bool watch,
        bool share,
        int expected
    )
    {
        var service = new AccountDomainService(
            NullLogger<AccountDomainService>.Instance,
            null!,
            null!,
            new ForecastOptions<UnfollowBatchedTaskOptions>(new()),
            new ForecastOptions<DailyTaskOptions>(
                new()
                {
                    NumberOfCoins = coins,
                    NumberOfProtectedCoins = protectedCoins,
                    IsWatchVideo = watch,
                    IsShareVideo = share,
                }
            )
        );
        Assert.Equal(
            expected,
            service.CalculateUpgradeTime(
                new UserInfo
                {
                    Money = (decimal)balance,
                    Level_info = new()
                    {
                        Current_level = 5,
                        Current_exp = 0,
                        Next_exp = 100,
                    },
                    Wbi_img = new() { img_url = "", sub_url = "" },
                }
            )
        );
    }

    [Fact]
    public void UpgradeForecast_MaximumLevel_HasNoNextLevel()
    {
        var service = new AccountDomainService(
            NullLogger<AccountDomainService>.Instance,
            null!,
            null!,
            new ForecastOptions<UnfollowBatchedTaskOptions>(new()),
            new ForecastOptions<DailyTaskOptions>(new())
        );
        Assert.Equal(
            0,
            service.CalculateUpgradeTime(
                new UserInfo
                {
                    Level_info = new() { Current_level = 6, Next_exp = "--" },
                    Wbi_img = new() { img_url = "", sub_url = "" },
                }
            )
        );
    }

    /// <summary>
    /// The day count is read from the live options, not from a snapshot taken when the
    /// service was built: the same account must be reported consistently once the config
    /// is saved, instead of one job printing 580 and the next 723.
    /// </summary>
    [Fact]
    public void UpgradeForecast_FollowsConfigSavedAtRuntime()
    {
        var options = new MutableOptions<DailyTaskOptions>(DailyTask(protectedCoins: 0));
        var service = new AccountDomainService(
            NullLogger<AccountDomainService>.Instance,
            null!,
            null!,
            new ForecastOptions<UnfollowBatchedTaskOptions>(new()),
            options
        );
        UserInfo account = Account(exp: 8520, money: 1372);

        Assert.Equal(580, service.CalculateUpgradeTime(account));

        options.Value = DailyTask(protectedCoins: 1150);

        Assert.Equal(723, service.CalculateUpgradeTime(account));
    }

    /// <summary>
    /// Protected coins are the one input that can move the answer by hundreds of days:
    /// once the spendable balance drops below the horizon, coin exp stops accumulating
    /// and the forecast falls back to the 25/day tail. These fixtures come from a real
    /// 2026-11 log where the number jumped 580 -> 723 overnight.
    /// </summary>
    [Theory]
    [InlineData(0, 580)]
    [InlineData(1150, 723)]
    [InlineData(1780, 975)]
    public void UpgradeForecast_ProtectedCoinsStretchTheEstimate(int protectedCoins, int expected)
    {
        UpgradeForecast forecast = UpgradeForecast.Calculate(
            Account(exp: 8520, money: 1372),
            DailyTask(protectedCoins: protectedCoins)
        );

        Assert.Equal(expected, forecast.Days);
        Assert.Equal(20280, forecast.NeedExp);
        Assert.Equal(2, forecast.CoinLimit);
        Assert.Equal(1372 - protectedCoins, forecast.AvailableCoins);
    }

    [Fact]
    public void UpgradeForecast_BreakdownExplainsCoinBudget()
    {
        UpgradeForecast forecast = UpgradeForecast.Calculate(
            Account(exp: 8520, money: 1372),
            DailyTask(protectedCoins: 1150)
        );

        Assert.True(forecast.IsStretchedByProtectedCoins);
        Assert.Equal(222, forecast.CoinCoverDays);
        Assert.Contains("可投币 222 枚", forecast.Describe());
        Assert.Contains("硬币只够投 222 天", forecast.Describe());
    }

    [Fact]
    public void UpgradeForecast_ProtectionAboveBalanceSaysHowLongCoinsStayLocked()
    {
        UpgradeForecast forecast = UpgradeForecast.Calculate(
            Account(exp: 8520, money: 1372),
            DailyTask(protectedCoins: 1780)
        );

        Assert.Equal(408, forecast.LockedDays);
        Assert.Contains("需再攒 408 天", forecast.Describe());
    }

    /// <summary>
    /// Nothing is donated once the stop level is reached, so its exp must not be counted.
    /// </summary>
    [Fact]
    public void UpgradeForecast_StoppedCoinDonationEarnsNoCoinExp()
    {
        var options = DailyTask(protectedCoins: 0);
        options.CoinDonationStopLevel = 5;

        UpgradeForecast forecast = UpgradeForecast.Calculate(
            Account(exp: 8520, money: 1372),
            options
        );

        Assert.Equal(0, forecast.CoinLimit);
        Assert.Equal(15, forecast.DailyBaseExp);
        Assert.Equal(1352, forecast.Days);
        Assert.Contains("已停投硬币", forecast.Describe());
    }

    private static DailyTaskOptions DailyTask(int protectedCoins) =>
        new()
        {
            NumberOfCoins = 2,
            NumberOfProtectedCoins = protectedCoins,
            IsWatchVideo = true,
            IsShareVideo = true,
        };

    private static UserInfo Account(long exp, decimal money) =>
        new()
        {
            Money = money,
            Level_info = new()
            {
                Current_level = 5,
                Current_exp = exp,
                Next_exp = 28800,
            },
            Wbi_img = new() { img_url = "", sub_url = "" },
        };

    private sealed class MutableOptions<T>(T value) : IOptionsMonitor<T>
    {
        public T Value { get; set; } = value;

        public T CurrentValue => Value;

        public T Get(string? name) => Value;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class ForecastOptions<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;

        public T Get(string? name) => value;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
