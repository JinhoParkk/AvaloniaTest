using System;
using System.Net.Http;
using System.Text.Json;
using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.UseCases.Services.Auth;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace AvaloniaApplication1.Infrastructure.Http.Refit;

/// <summary>
/// Refit 기반 API 클라이언트 서비스 등록 확장 메서드
/// </summary>
public static class RefitServiceExtensions
{
    private static readonly RefitSettings DefaultRefitSettings = new()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        })
    };

    /// <summary>
    /// Refit 기반 API 클라이언트 서비스 등록
    /// </summary>
    public static IServiceCollection AddRefitApiServices(
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

        // TokenRefreshService (HttpClient 직접 사용)
        services.AddSingleton<ITokenRefreshService>(sp =>
        {
            var httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(options.BaseUrl))
            {
                httpClient.BaseAddress = new Uri(options.BaseUrl);
            }
            httpClient.Timeout = options.Timeout;
            return new TokenRefreshService(httpClient, options);
        });

        // DelegatingHandler 등록
        services.AddTransient<AuthenticatedHttpClientHandler>();
        services.AddTransient<TokenRefreshHandler>();

        // 인증이 필요 없는 API용 HttpClient (로그인, 리프레시)
        services.AddHttpClient("AuthApi", client =>
        {
            if (!string.IsNullOrEmpty(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
            client.Timeout = options.Timeout;
        });

        // IAuthApi (인증 불필요) - Refit 등록
        services.AddSingleton<IAuthApi>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("AuthApi");
            return RestService.For<IAuthApi>(httpClient, DefaultRefitSettings);
        });

        // 인증이 필요한 API용 HttpClient
        services.AddHttpClient("AuthenticatedApi", client =>
        {
            if (!string.IsNullOrEmpty(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
            client.Timeout = options.Timeout;
        })
        .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
        .AddHttpMessageHandler<TokenRefreshHandler>();

        // AuthApiService (기존 인터페이스 유지)
        services.AddTransient<IAuthApiService, RefitAuthApiService>();

        // AuthenticationService
        services.AddSingleton<IAuthenticationService, AuthenticationService>();

        // 기존 IApiClient도 유지 (점진적 마이그레이션 지원)
        services.AddHttpClient(ApiClientOptions.HttpClientName, client =>
        {
            if (!string.IsNullOrEmpty(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
            client.Timeout = options.Timeout;
        });

        services.AddTransient<IApiClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(ApiClientOptions.HttpClientName);
            var tokenStorage = sp.GetRequiredService<ITokenStorage>();
            var refreshService = sp.GetRequiredService<ITokenRefreshService>();
            return new ApiClient(httpClient, tokenStorage, refreshService);
        });

        return services;
    }
}
