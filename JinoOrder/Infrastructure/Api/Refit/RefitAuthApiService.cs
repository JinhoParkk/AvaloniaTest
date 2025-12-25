using System;
using System.Threading;
using System.Threading.Tasks;
using JinoOrder.Domain.Auth;
using JinoOrder.Domain.Common;
using JinoOrder.Application.Auth;
using Refit;

namespace JinoOrder.Infrastructure.Api.Refit;

/// <summary>
/// Refit을 사용하는 AuthApiService 구현
/// </summary>
public class RefitAuthApiService : IAuthApiService
{
    private readonly IAuthApi _authApi;
    private readonly ITokenStorage _tokenStorage;

    public RefitAuthApiService(IAuthApi authApi, ITokenStorage tokenStorage)
    {
        _authApi = authApi;
        _tokenStorage = tokenStorage;
    }

    public async Task<ApiResult<LoginResponse>> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var response = await _authApi.LoginAsync(request, cancellationToken);

            // 로그인 성공 시 토큰 저장
            var tokens = new AuthTokens(response.AccessToken, response.RefreshToken);
            _tokenStorage.SaveTokens(tokens);

            return ApiResult<LoginResponse>.Success(response);
        }
        catch (ApiException ex)
        {
            return ApiResult<LoginResponse>.Failure(
                ex.Content ?? ex.Message,
                (int)ex.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResult<LoginResponse>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<RefreshTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            };

            var response = await _authApi.RefreshTokenAsync(request, cancellationToken);

            // 리프레시 성공 시 AccessToken 업데이트
            _tokenStorage.UpdateAccessToken(response.AccessToken);

            // 새 RefreshToken도 있으면 전체 업데이트
            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                var tokens = new AuthTokens(response.AccessToken, response.RefreshToken);
                _tokenStorage.SaveTokens(tokens);
            }

            return ApiResult<RefreshTokenResponse>.Success(response);
        }
        catch (ApiException ex)
        {
            return ApiResult<RefreshTokenResponse>.Failure(
                ex.Content ?? ex.Message,
                (int)ex.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResult<RefreshTokenResponse>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<bool>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _authApi.LogoutAsync(cancellationToken);
        }
        catch (ApiException)
        {
            // 로그아웃 API 실패해도 로컬 토큰은 삭제
        }
        catch
        {
            // 네트워크 오류도 무시
        }

        // 로컬 토큰 삭제
        _tokenStorage.ClearTokens();
        return ApiResult<bool>.Success(true);
    }
}
