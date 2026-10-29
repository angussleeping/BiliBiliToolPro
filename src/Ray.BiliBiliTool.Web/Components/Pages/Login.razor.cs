using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Ray.BiliBiliTool.Web.Services.Pages.Login;

namespace Ray.BiliBiliTool.Web.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ILoginPageStateFactory LoginPageStateFactory { get; set; } = null!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    private IAntiforgery Antiforgery { get; set; } = null!;

    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = null!;

    private string _username = "";
    private string _password = "";

    private bool _passwordVisibility;
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

    private void TogglePasswordVisibility()
    {
        if (_passwordVisibility)
        {
            _passwordVisibility = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
        }
        else
        {
            _passwordVisibility = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInput = InputType.Text;
        }
    }

    private string? returnUrl;
    private bool _loginError = false;
    private bool _loginLocked = false;

    // 登录表单的 antiforgery token：登录页是原生 form POST 到 /auth/login，
    // 需要显式带 token 防登录 CSRF（把受害者登录进攻击者账号）
    private string? _antiforgeryToken;
    private string _antiforgeryFieldName = "__RequestVerificationToken";

    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authenticationState.User.Identity?.IsAuthenticated == true)
        {
            NavigationManager.NavigateTo("/", replace: true);
            return;
        }

        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var state = LoginPageStateFactory.Create(uri);
        returnUrl = state.ReturnUrl;
        _loginError = state.HasLoginError;
        _loginLocked = state.IsLocked;

        // HttpContext 仅在服务端预渲染阶段可用；交互阶段（SignalR 线路）为空，
        // 但表单提交用的是预渲染输出的 HTML，token 已在那时生成并写入 cookie
        if (HttpContextAccessor.HttpContext is { } httpContext)
        {
            var tokens = Antiforgery.GetAndStoreTokens(httpContext);
            _antiforgeryToken = tokens.RequestToken;
            _antiforgeryFieldName = tokens.FormFieldName;
        }
    }
}
