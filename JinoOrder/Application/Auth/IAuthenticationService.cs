using System.Threading;
using System.Threading.Tasks;
using JinoOrder.Domain.Common;

namespace JinoOrder.Application.Auth;

/// <summary>
/// 인증 서비스 인터페이스
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// 현재 로그인된 사용자
    /// </summary>
    User? CurrentUser { get; }

    /// <summary>
    /// 로그인 여부
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// 로그인 (API 연동)
    /// </summary>
    Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// 로그아웃
    /// </summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 현재 사용자 설정 (자동 로그인용)
    /// </summary>
    void SetCurrentUser(User user);

    /// <summary>
    /// 로그인 실패 시 호출되는 이벤트
    /// </summary>
    event System.EventHandler? LoginFailed;

    /// <summary>
    /// 세션 만료 시 호출되는 이벤트
    /// </summary>
    event System.EventHandler? SessionExpired;
}
