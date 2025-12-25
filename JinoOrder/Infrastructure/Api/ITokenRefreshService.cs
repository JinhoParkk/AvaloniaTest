using System;
using System.Threading;
using System.Threading.Tasks;

namespace JinoOrder.Infrastructure.Api;

/// <summary>
/// 토큰 리프레시 서비스 인터페이스
/// </summary>
public interface ITokenRefreshService
{
    /// <summary>
    /// RefreshToken으로 새로운 AccessToken 발급
    /// </summary>
    /// <param name="refreshToken">현재 RefreshToken</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>새로운 AccessToken, 실패 시 null</returns>
    Task<string?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// 토큰 리프레시 실패 시 호출되는 이벤트
    /// </summary>
    event EventHandler? RefreshFailed;
}
