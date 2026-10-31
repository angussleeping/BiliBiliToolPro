namespace Ray.BiliBiliTool.Agent.BiliBiliAgent.Dtos.ApiApi.VipBigPoint;

/// <summary>
/// Request body for receiving / completing a big-point task.
/// </summary>
/// <remarks>
/// <para>
/// receive/v2 and complete/v2 are App-side endpoints: the server rejects any request that does not
/// carry the client version (<c>build</c>) with <c>6007000 "please update the client"</c>, so the
/// App parameters below are mandatory rather than decorative. See
/// <c>bruno/api.bilibili.com/pgc/activity/score/task/receive/v2/ogvwatchnew.yml</c> for a captured
/// sample request.
/// </para>
/// <para>
/// Property names must match the wire names exactly (<c>taskCode</c>). Refit serializes a
/// UrlEncoded body using the property name as the key and applies no naming policy, so a
/// PascalCase name would be sent verbatim and ignored by the server.
/// </para>
/// <para>
/// The fields are declared here instead of inherited from BaseAppRequest on purpose: the view
/// endpoints share <c>BaseAppRequest</c>, and keeping a separate client version lets this endpoint
/// track Bilibili's version gate without affecting them.
/// </para>
/// </remarks>
public class ReceiveOrCompleteTaskRequest
{
    /// <summary>
    /// Client build number. Raise it when Bilibili moves the version gate forward.
    /// </summary>
    private const string ClientBuild = "9120300";

    private const string ClientStatistics =
        "{\"appId\":1,\"platform\":3,\"version\":\"9.12.0\",\"abtest\":\"\"}";

    public ReceiveOrCompleteTaskRequest(string taskCode)
    {
        this.taskCode = taskCode;
    }

    /// <summary>
    /// Task code, e.g. ogvwatchnew, filmtab, bonus.
    /// </summary>
    public string taskCode { get; set; }

    /// <summary>
    /// Client build number. Leaving it out is what produces 6007000.
    /// </summary>
    public string build { get; } = ClientBuild;

    public string mobi_app { get; } = "android";

    public string platform { get; } = "android";

    public int disable_rcmd { get; } = 0;

    public string statistics { get; } = ClientStatistics;
}
