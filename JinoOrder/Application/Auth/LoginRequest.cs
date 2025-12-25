namespace JinoOrder.Application.Auth;

/// <summary>
/// 로그인 요청 DTO
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 로그인 응답 DTO
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string? Username { get; set; }
}

/// <summary>
/// 토큰 리프레시 요청 DTO
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// 토큰 리프레시 응답 DTO
/// </summary>
public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
}
