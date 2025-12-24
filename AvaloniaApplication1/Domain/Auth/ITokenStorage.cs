namespace AvaloniaApplication1.Domain.Auth;

/// <summary>
/// 토큰 저장소 인터페이스
/// </summary>
public interface ITokenStorage
{
    /// <summary>
    /// 현재 저장된 토큰 반환
    /// </summary>
    AuthTokens GetTokens();

    /// <summary>
    /// 토큰 저장
    /// </summary>
    void SaveTokens(AuthTokens tokens);

    /// <summary>
    /// 토큰 삭제 (로그아웃)
    /// </summary>
    void ClearTokens();

    /// <summary>
    /// AccessToken만 업데이트 (리프레시 시)
    /// </summary>
    void UpdateAccessToken(string accessToken);
}
