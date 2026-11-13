using System.Net;
using Ray.BiliBiliTool.Host.IntegrationTests.Support;

namespace Ray.BiliBiliTool.Host.IntegrationTests;

/// <summary>
/// Guards the login form pipeline end to end. The antiforgery filter is resolved per request,
/// so a missing MVC ViewFeatures registration only shows up as a 500 on POST /auth/login —
/// the DI assertion in <see cref="WebStartupTests"/> catches the same regression earlier, this
/// one proves the endpoint actually answers with a client error instead of crashing.
/// </summary>
[Collection("Host boot")]
public class LoginEndpointTests
{
    [Fact]
    public async Task Login_PostWithoutAntiforgeryToken_ReturnsBadRequestInsteadOfServerError()
    {
        using var factory = new WebHostFactory();
        using var client = factory.CreateClient();
        using var form = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["username"] = "admin",
                ["password"] = "not-a-real-password",
            }
        );

        var response = await client.PostAsync("/auth/login", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
