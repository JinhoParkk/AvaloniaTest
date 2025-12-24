using System;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Infrastructure.Http;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.UseCases.Services.Auth;

namespace AvaloniaApplication1.Services;

/// <summary>
/// 인증 서비스 - REST API 연동
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthApiService _authApiService;
    private readonly ITokenStorage _tokenStorage;
    private readonly ITokenRefreshService _tokenRefreshService;
    private readonly PreferencesService _preferencesService;

    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public event EventHandler? LoginFailed;
    public event EventHandler? SessionExpired;

    public AuthenticationService(
        IAuthApiService authApiService,
        ITokenStorage tokenStorage,
        ITokenRefreshService tokenRefreshService,
        PreferencesService preferencesService)
    {
        _authApiService = authApiService;
        _tokenStorage = tokenStorage;
        _tokenRefreshService = tokenRefreshService;
        _preferencesService = preferencesService;

        // 토큰 리프레시 실패 시 세션 만료 이벤트 발생
        _tokenRefreshService.RefreshFailed += OnRefreshFailed;
    }

    public async Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var result = await _authApiService.LoginAsync(username, password, cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            CurrentUser = new User
            {
                Username = result.Data.Username ?? username,
                Token = result.Data.AccessToken
            };

            return true;
        }

        LoginFailed?.Invoke(this, EventArgs.Empty);
        return false;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _authApiService.LogoutAsync(cancellationToken);
        CurrentUser = null;
        _preferencesService.ClearAutoLogin();
    }

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;

        // 저장된 토큰이 있으면 TokenStorage에도 설정
        if (!string.IsNullOrEmpty(user.Token))
        {
            var savedUser = _preferencesService.LoadAutoLogin();
            if (savedUser != null)
            {
                // RefreshToken은 별도로 저장되어야 함
                // 현재는 AccessToken만 복원
                _tokenStorage.SaveTokens(new AuthTokens(user.Token, string.Empty));
            }
        }
    }

    private void OnRefreshFailed(object? sender, EventArgs e)
    {
        CurrentUser = null;
        SessionExpired?.Invoke(this, EventArgs.Empty);
    }
}
