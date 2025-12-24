namespace AvaloniaApplication1.Domain.Auth;

/// <summary>
/// JWT 토큰 쌍 (AccessToken + RefreshToken)
/// </summary>
public class AuthTokens
{
    public string AccessToken { get; }
    public string RefreshToken { get; }

    /// <summary>
    /// AccessToken이 유효한지 여부
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(AccessToken);

    /// <summary>
    /// RefreshToken으로 갱신 가능한지 여부
    /// </summary>
    public bool CanRefresh => !string.IsNullOrEmpty(RefreshToken);

    public AuthTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken ?? string.Empty;
        RefreshToken = refreshToken ?? string.Empty;
    }

    /// <summary>
    /// 빈 토큰
    /// </summary>
    public static AuthTokens Empty => new(string.Empty, string.Empty);
}
