using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.UseCases.Services.Auth;

namespace AvaloniaApplication1.Infrastructure.Http;

/// <summary>
/// 토큰 리프레시 서비스 구현
/// </summary>
public class TokenRefreshService : ITokenRefreshService
{
    private readonly HttpClient _httpClient;
    private readonly ApiClientOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public event EventHandler? RefreshFailed;

    public TokenRefreshService(HttpClient httpClient, ApiClientOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<string?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };

            var response = await _httpClient.PostAsJsonAsync(
                _options.RefreshTokenEndpoint,
                request,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                OnRefreshFailed();
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>(JsonOptions, cancellationToken);
            return result?.AccessToken;
        }
        catch (Exception)
        {
            OnRefreshFailed();
            return null;
        }
    }

    private void OnRefreshFailed()
    {
        RefreshFailed?.Invoke(this, EventArgs.Empty);
    }
}
