using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Domain.Common;

namespace AvaloniaApplication1.Infrastructure.Http;

/// <summary>
/// REST API 클라이언트 인터페이스
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// GET 요청
    /// </summary>
    Task<ApiResult<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// POST 요청
    /// </summary>
    Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// POST 요청 (응답 없음)
    /// </summary>
    Task<ApiResult<bool>> PostAsync<TRequest>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// PUT 요청
    /// </summary>
    Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// DELETE 요청
    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// 인증 없이 POST 요청 (로그인, 리프레시용)
    /// </summary>
    Task<ApiResult<TResponse>> PostWithoutAuthAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default);
}
