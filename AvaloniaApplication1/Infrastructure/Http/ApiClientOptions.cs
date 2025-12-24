using System;

namespace AvaloniaApplication1.Infrastructure.Http;

/// <summary>
/// API 클라이언트 설정
/// </summary>
public class ApiClientOptions
{
    /// <summary>
    /// API 기본 URL (예: https://api.passorder.com)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 요청 타임아웃 (기본: 30초)
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 토큰 리프레시 엔드포인트 (예: /auth/refresh)
    /// </summary>
    public string RefreshTokenEndpoint { get; set; } = "/auth/refresh";

    /// <summary>
    /// HttpClient 이름 (Named HttpClient용)
    /// </summary>
    public const string HttpClientName = "PassOrderApi";
}
