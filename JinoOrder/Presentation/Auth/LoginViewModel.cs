using JinoOrder.Presentation.Common;
using JinoOrder.Presentation.Main;
using JinoOrder.Infrastructure.Storage;
using JinoOrder.Application.Auth;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Domain.Common;
using JinoOrder.Application.Common;
using JinoOrder.Infrastructure.Services;

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

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public string AppVersion { get; }

    public LoginViewModel(
        IAuthenticationService authService,
        PreferencesService preferencesService,
        NavigationService navigationService)
    {
        _authService = authService;
        _preferencesService = preferencesService;
        _navigationService = navigationService;

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        AppVersion = $"v{version?.Major}.{version?.Minor}.{version?.Build}";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "아이디를 입력해주세요.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "비밀번호를 입력해주세요.";
            return;
        }

        IsLoading = true;

        try
        {
            var success = await _authService.LoginAsync(Username, Password);

            if (success)
            {
                if (RememberMe && _authService.CurrentUser != null)
                {
                    _preferencesService.SaveAutoLogin(_authService.CurrentUser);
                }

                // 타입 기반 네비게이션 (DI에서 resolve)
                _navigationService.NavigateTo<MainViewModel>();
            }
            else
            {
                ErrorMessage = "아이디 또는 비밀번호가 올바르지 않습니다.";
            }
        }
        catch
        {
            ErrorMessage = "로그인 중 오류가 발생했습니다. 다시 시도해주세요.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void FindPassword()
    {
        // 나중에 구현 예정
    }
}
