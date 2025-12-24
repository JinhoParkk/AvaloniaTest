using System;
using System.Net.Http;
using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.UseCases.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.Infrastructure.Http;

/// <summary>
/// HTTP 클라이언트 관련 서비스 등록 확장 메서드
/// </summary>
public static class HttpClientServiceExtensions
{
    /// <summary>
    /// REST API 클라이언트 서비스 등록
    /// </summary>
    public static IServiceCollection AddApiClientServices(
        this IServiceCollection services,
        Action<ApiClientOptions>? configureOptions = null)
    {
        // 옵션 설정
        var options = new ApiClientOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // AuthApiOptions 설정
        var authOptions = new AuthApiOptions();
        services.AddSingleton(authOptions);

        // TokenStorage (싱글톤 - 앱 전체에서 하나의 토큰 상태 유지)
        services.AddSingleton<ITokenStorage, InMemoryTokenStorage>();

        // HttpClient 등록 (HttpClientFactory 사용)
        services.AddHttpClient(ApiClientOptions.HttpClientName, client =>
        {
            if (!string.IsNullOrEmpty(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
            client.Timeout = options.Timeout;
        });

        // TokenRefreshService
        services.AddSingleton<ITokenRefreshService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(ApiClientOptions.HttpClientName);
            var apiOptions = sp.GetRequiredService<ApiClientOptions>();
            return new TokenRefreshService(httpClient, apiOptions);
        });

        // ApiClient
        services.AddTransient<IApiClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(ApiClientOptions.HttpClientName);
            var tokenStorage = sp.GetRequiredService<ITokenStorage>();
            var refreshService = sp.GetRequiredService<ITokenRefreshService>();
            return new ApiClient(httpClient, tokenStorage, refreshService);
        });

        // AuthApiService
        services.AddTransient<IAuthApiService, AuthApiService>();

        // AuthenticationService
        services.AddSingleton<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
