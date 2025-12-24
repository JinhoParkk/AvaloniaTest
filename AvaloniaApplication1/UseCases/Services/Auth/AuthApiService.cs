using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Domain.Common;
using AvaloniaApplication1.Infrastructure.Http;

namespace AvaloniaApplication1.UseCases.Services.Auth;

/// <summary>
/// 인증 API 서비스 구현
/// </summary>
public class AuthApiService : IAuthApiService
{
    private readonly IApiClient _apiClient;
    private readonly ITokenStorage _tokenStorage;
    private readonly AuthApiOptions _options;

    public AuthApiService(
        IApiClient apiClient,
        ITokenStorage tokenStorage,
        AuthApiOptions options)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _options = options;
    }

    public async Task<ApiResult<LoginResponse>> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var result = await _apiClient.PostWithoutAuthAsync<LoginRequest, LoginResponse>(
            _options.LoginEndpoint,
            request,
            cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            // 로그인 성공 시 토큰 저장
            var tokens = new AuthTokens(result.Data.AccessToken, result.Data.RefreshToken);
            _tokenStorage.SaveTokens(tokens);
        }

        return result;
    }

    public async Task<ApiResult<RefreshTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        var result = await _apiClient.PostWithoutAuthAsync<RefreshTokenRequest, RefreshTokenResponse>(
            _options.RefreshTokenEndpoint,
            request,
            cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            // 리프레시 성공 시 AccessToken 업데이트
            _tokenStorage.UpdateAccessToken(result.Data.AccessToken);

            // 새 RefreshToken도 있으면 전체 업데이트
            if (!string.IsNullOrEmpty(result.Data.RefreshToken))
            {
                var tokens = new AuthTokens(result.Data.AccessToken, result.Data.RefreshToken);
                _tokenStorage.SaveTokens(tokens);
            }
        }

        return result;
    }

    public async Task<ApiResult<bool>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        // 로그아웃 API가 있는 경우 호출
        if (!string.IsNullOrEmpty(_options.LogoutEndpoint))
        {
            var result = await _apiClient.PostAsync<object>(_options.LogoutEndpoint, new { }, cancellationToken);
            if (result.IsFailure)
            {
                // 로그아웃 API 실패해도 로컬 토큰은 삭제
            }
        }

        // 로컬 토큰 삭제
        _tokenStorage.ClearTokens();
        return ApiResult<bool>.Success(true);
    }
}

/// <summary>
/// 인증 API 옵션
/// </summary>
public class AuthApiOptions
{
    public string LoginEndpoint { get; set; } = "/auth/login";
    public string RefreshTokenEndpoint { get; set; } = "/auth/refresh";
    public string? LogoutEndpoint { get; set; } = "/auth/logout";
}
