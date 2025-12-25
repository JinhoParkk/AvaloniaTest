using JinoOrder.Domain.Auth;

namespace JinoOrder.Infrastructure.Storage;

/// <summary>
/// 메모리 기반 토큰 저장소 (앱 종료 시 소멸)
/// 주로 AccessToken 저장용
/// </summary>
public class InMemoryTokenStorage : ITokenStorage
{
    private AuthTokens _tokens = AuthTokens.Empty;
    private readonly object _lock = new();

    public AuthTokens GetTokens()
    {
        lock (_lock)
        {
            return _tokens;
        }
    }

    public void SaveTokens(AuthTokens tokens)
    {
        lock (_lock)
        {
            _tokens = tokens;
        }
    }

    public void ClearTokens()
    {
        lock (_lock)
        {
            _tokens = AuthTokens.Empty;
        }
    }

    public void UpdateAccessToken(string accessToken)
    {
        lock (_lock)
        {
            _tokens = new AuthTokens(accessToken, _tokens.RefreshToken);
        }
    }
}
