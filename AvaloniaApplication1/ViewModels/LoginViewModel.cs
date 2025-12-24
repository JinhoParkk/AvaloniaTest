using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthenticationService _authService;
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

    public string AppVersion { get; }

    public LoginViewModel(
        AuthenticationService authService,
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
    private void Login()
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

        if (_authService.Login(Username, Password))
        {
            if (RememberMe && _authService.CurrentUser != null)
            {
                _preferencesService.SaveAutoLogin(_authService.CurrentUser);
            }

            _navigationService.NavigateTo(new MainViewModel());
        }
        else
        {
            ErrorMessage = "아이디 또는 비밀번호가 올바르지 않습니다.";
        }
    }

    [RelayCommand]
    private void FindPassword()
    {
        // 나중에 구현 예정
    }
}
