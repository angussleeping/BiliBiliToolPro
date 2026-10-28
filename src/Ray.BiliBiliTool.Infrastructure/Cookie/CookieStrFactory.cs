using Microsoft.Extensions.Configuration;
using Ray.BiliBiliTool.Infrastructure.Extensions;

namespace Ray.BiliBiliTool.Infrastructure.Cookie;

public class CookieStrFactory<TCookieInfo>(IConfiguration configuration)
    where TCookieInfo : CookieInfo
{
    private Dictionary<int, Dictionary<string, string>> CookieDictionary => GetCookieDictionary();

    public int Count => CookieDictionary.Count;

    public TCookieInfo GetCookie(int index)
    {
        var dic = GetCookieDictionary()[index];
        return (TCookieInfo)Activator.CreateInstance(typeof(TCookieInfo), dic)!
            ?? throw new InvalidOperationException();
    }

    public static TCookieInfo CreateNew(string cookie)
    {
        Dictionary<string, string> dic = CkStrToDictionary(cookie);
        return (TCookieInfo)Activator.CreateInstance(typeof(TCookieInfo), dic)!
            ?? throw new InvalidOperationException();
    }

    #region private

    private Dictionary<int, Dictionary<string, string>> GetCookieDictionary()
    {
        var list = configuration.GetSection("BiliBiliCookies").Get<List<string>>() ?? [];
        return CookeStrListToCookieDic(list);
    }

    private Dictionary<int, Dictionary<string, string>> CookeStrListToCookieDic(List<string> ckList)
    {
        var dic = new Dictionary<int, Dictionary<string, string>>();

        for (int i = 0; i < ckList.Count; i++)
        {
            dic.Add(i, CkStrToDictionary(ckList[i]));
        }

        return dic;
    }

    private static Dictionary<string, string> CkStrToDictionary(string ckStr)
    {
        var dic = new Dictionary<string, string>();
        var ckItemList = ckStr.Split(";", StringSplitOptions.TrimEntries).Distinct();
        foreach (var item in ckItemList)
        {
            // 跳过无键名的片段（如末尾多余的 ;），避免 IndexOf 返回 -1 时切片越界
            var index = item.IndexOf("=", StringComparison.Ordinal);
            if (index <= 0)
                continue;

            var key = item[..index].Trim();
            var value = item[(index + 1)..].Trim();
            dic.AddIfNotExist(new KeyValuePair<string, string>(key, value), p => p.Key == key);
        }
        return dic;
    }

    #endregion
}
