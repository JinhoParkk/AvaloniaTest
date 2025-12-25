using System.Threading;
using System.Threading.Tasks;
using JinoOrder.Application.Auth;
using Refit;

namespace JinoOrder.Infrastructure.Api.Refit;

/// <summary>
/// Refit 기반 인증 API 인터페이스
/// Java Retrofit 스타일로 HTTP 엔드포인트 정의
/// </summary>
public interface IAuthApi
{
    /// <summary>
    /// 로그인
    /// </summary>
    [Post("/auth/login")]
    Task<LoginResponse> LoginAsync([Body] LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 토큰 리프레시
    /// </summary>
    [Post("/auth/refresh")]
    Task<RefreshTokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 로그아웃
    /// </summary>
    [Post("/auth/logout")]
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
