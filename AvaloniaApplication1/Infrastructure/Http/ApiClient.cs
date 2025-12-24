using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Domain.Common;
using AvaloniaApplication1.Infrastructure.Json;

namespace AvaloniaApplication1.Infrastructure.Http;

/// <summary>
/// REST API 클라이언트 구현
/// 401 응답 시 자동 토큰 리프레시 및 재시도
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorage _tokenStorage;
    private readonly ITokenRefreshService _tokenRefreshService;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions =
        JsonSerializerOptionsExtensions.CreateApiOptions();

    public ApiClient(
        HttpClient httpClient,
        ITokenStorage tokenStorage,
        ITokenRefreshService tokenRefreshService)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
        _tokenRefreshService = tokenRefreshService;
    }

    public async Task<ApiResult<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync<T>(
            async () =>
            {
                var request = CreateRequest(HttpMethod.Get, endpoint);
                return await _httpClient.SendAsync(request, cancellationToken);
            },
            cancellationToken);
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync<TResponse>(
            async () =>
            {
                var request = CreateRequest(HttpMethod.Post, endpoint);
                request.Content = JsonContent.Create(data, options: JsonOptions);
                return await _httpClient.SendAsync(request, cancellationToken);
            },
            cancellationToken);
    }

    public async Task<ApiResult<bool>> PostAsync<TRequest>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<TRequest, object>(endpoint, data, cancellationToken);
        return result.IsSuccess
            ? ApiResult<bool>.Success(true)
            : ApiResult<bool>.Failure(result.Error!);
    }

    public async Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync<TResponse>(
            async () =>
            {
                var request = CreateRequest(HttpMethod.Put, endpoint);
                request.Content = JsonContent.Create(data, options: JsonOptions);
                return await _httpClient.SendAsync(request, cancellationToken);
            },
            cancellationToken);
    }

    public async Task<ApiResult<bool>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteWithRetryAsync<object>(
            async () =>
            {
                var request = CreateRequest(HttpMethod.Delete, endpoint);
                return await _httpClient.SendAsync(request, cancellationToken);
            },
            cancellationToken);

        return result.IsSuccess
            ? ApiResult<bool>.Success(true)
            : ApiResult<bool>.Failure(result.Error!);
    }

    public async Task<ApiResult<TResponse>> PostWithoutAuthAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(data, options: JsonOptions)
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await ProcessResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<TResponse>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<TResponse>.Failure("Request timeout");
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, endpoint);

        var tokens = _tokenStorage.GetTokens();
        if (tokens.IsValid)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        }

        return request;
    }

    private async Task<ApiResult<T>> ExecuteWithRetryAsync<T>(
        Func<Task<HttpResponseMessage>> executeRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await executeRequest();

            // 401이면 토큰 리프레시 시도
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshed = await TryRefreshTokenAsync(cancellationToken);
                if (refreshed)
                {
                    // 리프레시 성공 시 재시도
                    response = await executeRequest();
                }
                else
                {
                    // 리프레시 실패 시 로그아웃 처리
                    _tokenStorage.ClearTokens();
                    return ApiResult<T>.Unauthorized("Session expired. Please login again.");
                }
            }

            return await ProcessResponseAsync<T>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("Request timeout");
        }
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

    private async Task<ApiResult<T>> ProcessResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            if (response.Content.Headers.ContentLength == 0)
            {
                return ApiResult<T>.Success(default!);
            }

            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return ApiResult<T>.Success(data!);
        }

        var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
        return ApiResult<T>.Failure(errorMessage, (int)response.StatusCode);
    }
}
