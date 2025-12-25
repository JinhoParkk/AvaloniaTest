using JinoOrder.Presentation.Common;
using JinoOrder.Presentation.Shell;
using JinoOrder.Infrastructure.Storage;
using JinoOrder.Application.Auth;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Domain.Common;
using JinoOrder.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Presentation.Auth;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;
    private readonly PreferencesService _preferencesService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe = true;

    public string AppVersion { get; }

    public LoginViewModel(
        IAuthenticationService authService,
        PreferencesService preferencesService,
        NavigationService navigationService,
        ILogger<LoginViewModel> logger)
    {
        _authService = authService;
        _preferencesService = preferencesService;
        _navigationService = navigationService;
        Logger = logger;

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        AppVersion = $"v{version?.Major}.{version?.Minor}.{version?.Build}";

        Logger.LogDebug("LoginViewModel 초기화됨");
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ClearError();

        // 검증
        if (string.IsNullOrWhiteSpace(Username))
        {
            SetError(ValidationMessages.UsernameRequired);
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            SetError(ValidationMessages.PasswordRequired);
            return;
        }

        Logger.LogInformation("로그인 시도: Username={Username}", Username);

        var result = await ExecuteAsync(async ct =>
        {
            var success = await _authService.LoginAsync(Username, Password);

            if (success)
            {
                Logger.LogInformation("로그인 성공: Username={Username}", Username);

                if (RememberMe && _authService.CurrentUser != null)
                {
                    _preferencesService.SaveAutoLogin(_authService.CurrentUser);
                    Logger.LogDebug("자동 로그인 정보 저장됨");
                }

                _navigationService.NavigateTo<JinoOrderMainViewModel>();
            }
            else
            {
                Logger.LogWarning("로그인 실패 - 잘못된 인증 정보: Username={Username}", Username);
                SetError(ValidationMessages.InvalidCredentials);
            }
        }, "로그인");

        if (result.IsFailure && result.ErrorType != ErrorType.Validation)
        {
            Logger.LogError("로그인 중 오류 발생: Username={Username}, Error={Error}", Username, result.Error);
            SetError(ValidationMessages.LoginError);
        }
    }

    [RelayCommand]
    private void FindPassword()
    {
        Logger.LogDebug("비밀번호 찾기 요청됨");
        // 나중에 구현 예정
    }
}
