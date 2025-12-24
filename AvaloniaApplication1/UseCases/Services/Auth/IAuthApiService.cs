using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Domain.Common;

namespace AvaloniaApplication1.UseCases.Services.Auth;

/// <summary>
/// 인증 API 서비스 인터페이스
/// </summary>
public interface IAuthApiService
{
    /// <summary>
    /// 로그인
    /// </summary>
    Task<ApiResult<LoginResponse>> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 토큰 리프레시
    /// </summary>
    Task<ApiResult<RefreshTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 로그아웃
    /// </summary>
    Task<ApiResult<bool>> LogoutAsync(CancellationToken cancellationToken = default);
}
