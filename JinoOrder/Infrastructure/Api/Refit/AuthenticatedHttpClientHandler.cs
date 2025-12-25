using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using JinoOrder.Domain.Auth;

namespace JinoOrder.Infrastructure.Api.Refit;

/// <summary>
/// 인증 헤더를 자동으로 추가하는 DelegatingHandler
/// Bearer 토큰을 요청에 추가
/// </summary>
public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage;

    public AuthenticatedHttpClientHandler(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokens = _tokenStorage.GetTokens();
        if (tokens.IsValid)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

/// <summary>
/// 401 응답 시 자동으로 토큰 리프레시 및 재시도하는 DelegatingHandler
/// </summary>
public class TokenRefreshHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage;
    private readonly ITokenRefreshService _tokenRefreshService;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    /// <summary>
    /// 토큰 리프레시 실패 시 발생하는 이벤트
    /// </summary>
    public event EventHandler? TokenRefreshFailed;

    public TokenRefreshHandler(
        ITokenStorage tokenStorage,
        ITokenRefreshService tokenRefreshService)
    {
        _tokenStorage = tokenStorage;
        _tokenRefreshService = tokenRefreshService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // 401이면 토큰 리프레시 시도
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshTokenAsync(cancellationToken);
            if (refreshed)
            {
                // 리프레시 성공 시 재시도
                var newRequest = await CloneRequestAsync(request);
                var tokens = _tokenStorage.GetTokens();
                if (tokens.IsValid)
                {
                    newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
                }
                response.Dispose();
                response = await base.SendAsync(newRequest, cancellationToken);
            }
            else
            {
                // 리프레시 실패 시 토큰 삭제 및 이벤트 발생
                _tokenStorage.ClearTokens();
                TokenRefreshFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var tokens = _tokenStorage.GetTokens();
            if (!tokens.CanRefresh)
            {
                return false;
            }

            var newAccessToken = await _tokenRefreshService.RefreshAsync(tokens.RefreshToken, cancellationToken);
            if (string.IsNullOrEmpty(newAccessToken))
            {
                return false;
            }

            _tokenStorage.UpdateAccessToken(newAccessToken);
            return true;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // Content 복제
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Headers 복제 (Authorization 제외)
        foreach (var header in request.Headers)
        {
            if (!header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
