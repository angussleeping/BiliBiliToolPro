using Ray.BiliBiliTool.Agent.BiliBiliAgent.Dtos.NavApi;
using Ray.BiliBiliTool.Config.Options;

namespace Ray.BiliBiliTool.DomainService;

/// <summary>
/// Level-up forecast: how much exp is missing and how much exp each day can still earn.
/// The inputs are returned together with the result so a reported day count can be
/// reproduced from the log alone.
/// </summary>
public sealed record UpgradeForecast
{
    /// <summary>Days needed to reach the next level.</summary>
    public required int Days { get; init; }

    /// <summary>Exp still missing for the next level.</summary>
    public required long NeedExp { get; init; }

    /// <summary>Exp earned every day regardless of coins: login 5 + watch 5 + share 5.</summary>
    public required int DailyBaseExp { get; init; }

    /// <summary>Coins donatable per day, 0 when coin donation is off or stopped at this level.</summary>
    public required int CoinLimit { get; init; }

    /// <summary>Coin balance reported by Bilibili.</summary>
    public required decimal Money { get; init; }

    /// <summary>Coins the user asked to keep untouched.</summary>
    public required int ProtectedCoins { get; init; }

    /// <summary>
    /// Coins that may actually be spent: balance minus the protected floor.
    /// Negative means the balance is still below the floor, so nothing can be spent yet.
    /// </summary>
    public required decimal AvailableCoins { get; init; }

    /// <summary>Days to wait before the protected floor is met and coin exp starts to count.</summary>
    public required int LockedDays { get; init; }

    /// <summary>Days the spendable coins last at <see cref="CoinLimit"/> per day.</summary>
    public required int CoinCoverDays { get; init; }

    /// <summary>Exp per day on day one, when coins are still available.</summary>
    public int FirstDayExp => DailyBaseExp + CoinLimit * 10;

    /// <summary>Exp per day once the spendable coins are gone; login still adds one coin daily.</summary>
    public int TailExp => DailyBaseExp + 10;

    /// <summary>True when the days were stretched by the protected-coin floor rather than by exp.</summary>
    public bool IsStretchedByProtectedCoins =>
        CoinLimit > 0 && Days > CoinCoverDays && CoinCoverDays < int.MaxValue;

    public static UpgradeForecast None { get; } =
        new()
        {
            Days = 0,
            NeedExp = 0,
            DailyBaseExp = 0,
            CoinLimit = 0,
            Money = 0,
            ProtectedCoins = 0,
            AvailableCoins = 0,
            LockedDays = 0,
            CoinCoverDays = int.MaxValue,
        };

    public static UpgradeForecast Calculate(UserInfo useInfo, DailyTaskOptions options)
    {
        var level = useInfo.Level_info;
        if (level is null || level.Current_level >= 6)
            return None;

        decimal needExp = (decimal)level.GetNext_expLong() - level.Current_exp;
        if (needExp <= 0)
            return None;

        int dailyBaseExp = 5 + (options.IsWatchVideo ? 5 : 0) + (options.IsShareVideo ? 5 : 0);

        // No coin is donated once the configured stop level is reached, so its exp must
        // not be counted either.
        int coinLimit = options.ShouldSkipCoinDonation(level.Current_level)
            ? 0
            : Math.Clamp(options.NumberOfCoins, 0, 5);

        int protectedCoins = Math.Max(0, options.NumberOfProtectedCoins);
        decimal money = decimal.Floor(useInfo.Money ?? 0);
        decimal availableCoins = money - protectedCoins;

        // Login grants one coin per day, which can be donated the same day.
        decimal ExpAfter(int days) =>
            (decimal)days * dailyBaseExp
            + 10 * Math.Min((decimal)days * coinLimit, Math.Max(0, availableCoins + days));

        int low = 1;
        int high = int.MaxValue;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            if (ExpAfter(middle) >= needExp)
                high = middle;
            else
                low = middle + 1;
        }

        return new UpgradeForecast
        {
            Days = low,
            NeedExp = (long)needExp,
            DailyBaseExp = dailyBaseExp,
            CoinLimit = coinLimit,
            Money = money,
            ProtectedCoins = protectedCoins,
            AvailableCoins = availableCoins,
            LockedDays = availableCoins >= 0 ? 0 : (int)decimal.Ceiling(-availableCoins),
            CoinCoverDays = CoinCoverDaysOf(availableCoins, coinLimit),
        };
    }

    /// <summary>
    /// Describes where the day count comes from, so the log line answers "why this number".
    /// </summary>
    public string Describe()
    {
        if (Days <= 0)
            return "经验已达标，等待 B 站结算";

        string head =
            $"剩余 {NeedExp} 经验｜首日 {FirstDayExp} 分/天（基础 {DailyBaseExp} + 投币 {CoinLimit}×10）";
        string coins =
            CoinLimit <= 0
                ? "｜已停投硬币"
                : $"｜可投币 {AvailableCoins} 枚（余额 {Money} - 保留 {ProtectedCoins}）";

        if (LockedDays > 0)
            return $"{head}{coins}｜保留线未达，需再攒 {LockedDays} 天硬币才开始计投币经验";

        if (IsStretchedByProtectedCoins)
            return $"{head}{coins}｜硬币只够投 {CoinCoverDays} 天，之后回落到 {TailExp} 分/天";

        return $"{head}{coins}";
    }

    private static int CoinCoverDaysOf(decimal availableCoins, int coinLimit)
    {
        if (coinLimit <= 0 || availableCoins <= 0)
            return availableCoins <= 0 && coinLimit > 0 ? 0 : int.MaxValue;

        // Coins last while days * coinLimit <= available + days, i.e. (coinLimit - 1) * days.
        if (coinLimit == 1)
            return int.MaxValue;

        decimal days = availableCoins / (coinLimit - 1);
        return days >= int.MaxValue ? int.MaxValue : (int)decimal.Floor(days);
    }
}
