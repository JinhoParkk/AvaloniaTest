using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JinoOrder.Domain.Common;
using Microsoft.Extensions.Logging;
using Refit;

namespace JinoOrder.Infrastructure.Common;

/// <summary>
/// 중앙 예외 처리 헬퍼
/// </summary>
public static class ExceptionHandler
{
    /// <summary>
    /// 예외를 Result로 변환
    /// </summary>
    public static Result HandleException(Exception ex, ILogger? logger = null, string? operationName = null)
    {
        LogException(ex, logger, operationName);
        return ClassifyAndCreateResult(ex);
    }

    /// <summary>
    /// 예외를 Result<T>로 변환
    /// </summary>
    public static Result<T> HandleException<T>(Exception ex, ILogger? logger = null, string? operationName = null)
    {
        LogException(ex, logger, operationName);
        return ClassifyAndCreateResult<T>(ex);
    }

    /// <summary>
    /// 비동기 작업을 안전하게 실행하고 Result 반환
    /// </summary>
    public static async Task<Result> ExecuteAsync(
        Func<Task> action,
        ILogger? logger = null,
        string? operationName = null)
    {
        try
        {
            await action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return HandleException(ex, logger, operationName);
        }
    }

    /// <summary>
    /// 비동기 작업을 안전하게 실행하고 Result<T> 반환
    /// </summary>
    public static async Task<Result<T>> ExecuteAsync<T>(
        Func<Task<T>> action,
        ILogger? logger = null,
        string? operationName = null)
    {
        try
        {
            var result = await action();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return HandleException<T>(ex, logger, operationName);
        }
    }

    /// <summary>
    /// 동기 작업을 안전하게 실행하고 Result 반환
    /// </summary>
    public static Result Execute(
        Action action,
        ILogger? logger = null,
        string? operationName = null)
    {
        try
        {
            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return HandleException(ex, logger, operationName);
        }
    }

    /// <summary>
    /// 동기 작업을 안전하게 실행하고 Result<T> 반환
    /// </summary>
    public static Result<T> Execute<T>(
        Func<T> action,
        ILogger? logger = null,
        string? operationName = null)
    {
        try
        {
            var result = action();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return HandleException<T>(ex, logger, operationName);
        }
    }

    private static void LogException(Exception ex, ILogger? logger, string? operationName)
    {
        var operation = operationName ?? "알 수 없는 작업";

        switch (ex)
        {
            case TaskCanceledException:
                logger?.LogWarning("요청 시간이 초과되었습니다: {Operation}", operation);
                break;

            case OperationCanceledException:
                logger?.LogDebug("작업이 취소되었습니다: {Operation}", operation);
                break;

            case HttpRequestException httpEx:
                logger?.LogError(httpEx, "네트워크 오류 발생: {Operation}, StatusCode: {StatusCode}",
                    operation, httpEx.StatusCode);
                break;

            case ApiException apiEx:
                logger?.LogError(apiEx, "API 오류 발생: {Operation}, StatusCode: {StatusCode}, Content: {Content}",
                    operation, apiEx.StatusCode, apiEx.Content);
                break;

            case UnauthorizedAccessException:
                logger?.LogWarning("인증 오류 발생: {Operation}", operation);
                break;

            default:
                logger?.LogError(ex, "예기치 않은 오류 발생: {Operation}", operation);
                break;
        }
    }

    private static Result ClassifyAndCreateResult(Exception ex)
    {
        return ex switch
        {
            TaskCanceledException => Result.TimeoutFailure(),
            OperationCanceledException => Result.CancelledResult(),
            HttpRequestException httpEx => CreateHttpResult(httpEx),
            ApiException apiEx => CreateApiResult(apiEx),
            UnauthorizedAccessException => Result.AuthFailure(),
            TimeoutException => Result.TimeoutFailure(),
            ArgumentException argEx => Result.ValidationFailure(argEx.Message),
            _ => Result.Failure(ValidationMessages.ServerError, ErrorType.Unknown)
        };
    }

    private static Result<T> ClassifyAndCreateResult<T>(Exception ex)
    {
        return ex switch
        {
            TaskCanceledException => Result<T>.TimeoutFailure(),
            OperationCanceledException => Result<T>.CancelledResult(),
            HttpRequestException httpEx => CreateHttpResult<T>(httpEx),
            ApiException apiEx => CreateApiResult<T>(apiEx),
            UnauthorizedAccessException => Result<T>.AuthFailure(),
            TimeoutException => Result<T>.TimeoutFailure(),
            ArgumentException argEx => Result<T>.ValidationFailure(argEx.Message),
            _ => Result<T>.Failure(ValidationMessages.ServerError, ErrorType.Unknown)
        };
    }

    private static Result CreateHttpResult(HttpRequestException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized => Result.AuthFailure(),
            HttpStatusCode.Forbidden => Result.AuthFailure("접근 권한이 없습니다."),
            HttpStatusCode.NotFound => Result.Failure("요청한 리소스를 찾을 수 없습니다.", ErrorType.NotFound),
            HttpStatusCode.Conflict => Result.Failure("데이터 충돌이 발생했습니다.", ErrorType.Conflict),
            HttpStatusCode.InternalServerError => Result.Failure(ValidationMessages.ServerError, ErrorType.ServerError),
            HttpStatusCode.ServiceUnavailable => Result.Failure("서비스를 일시적으로 사용할 수 없습니다.", ErrorType.ServerError),
            _ => Result.NetworkFailure()
        };
    }

    private static Result<T> CreateHttpResult<T>(HttpRequestException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized => Result<T>.AuthFailure(),
            HttpStatusCode.Forbidden => Result<T>.AuthFailure("접근 권한이 없습니다."),
            HttpStatusCode.NotFound => Result<T>.Failure("요청한 리소스를 찾을 수 없습니다.", ErrorType.NotFound),
            HttpStatusCode.Conflict => Result<T>.Failure("데이터 충돌이 발생했습니다.", ErrorType.Conflict),
            HttpStatusCode.InternalServerError => Result<T>.Failure(ValidationMessages.ServerError, ErrorType.ServerError),
            HttpStatusCode.ServiceUnavailable => Result<T>.Failure("서비스를 일시적으로 사용할 수 없습니다.", ErrorType.ServerError),
            _ => Result<T>.NetworkFailure()
        };
    }

    private static Result CreateApiResult(ApiException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized => Result.AuthFailure(),
            HttpStatusCode.Forbidden => Result.AuthFailure("접근 권한이 없습니다."),
            HttpStatusCode.NotFound => Result.Failure("요청한 리소스를 찾을 수 없습니다.", ErrorType.NotFound),
            HttpStatusCode.BadRequest => Result.ValidationFailure(ex.Content ?? "잘못된 요청입니다."),
            HttpStatusCode.Conflict => Result.Failure("데이터 충돌이 발생했습니다.", ErrorType.Conflict),
            HttpStatusCode.InternalServerError => Result.Failure(ValidationMessages.ServerError, ErrorType.ServerError),
            _ => Result.Failure(ex.Content ?? ValidationMessages.NetworkError, ErrorType.Unknown)
        };
    }

    private static Result<T> CreateApiResult<T>(ApiException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized => Result<T>.AuthFailure(),
            HttpStatusCode.Forbidden => Result<T>.AuthFailure("접근 권한이 없습니다."),
            HttpStatusCode.NotFound => Result<T>.Failure("요청한 리소스를 찾을 수 없습니다.", ErrorType.NotFound),
            HttpStatusCode.BadRequest => Result<T>.ValidationFailure(ex.Content ?? "잘못된 요청입니다."),
            HttpStatusCode.Conflict => Result<T>.Failure("데이터 충돌이 발생했습니다.", ErrorType.Conflict),
            HttpStatusCode.InternalServerError => Result<T>.Failure(ValidationMessages.ServerError, ErrorType.ServerError),
            _ => Result<T>.Failure(ex.Content ?? ValidationMessages.NetworkError, ErrorType.Unknown)
        };
    }

    /// <summary>
    /// 사용자 친화적 에러 메시지 반환
    /// </summary>
    public static string GetUserFriendlyMessage(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Network => ValidationMessages.NetworkError,
            ErrorType.Timeout => ValidationMessages.TimeoutError,
            ErrorType.Auth => ValidationMessages.InvalidCredentials,
            ErrorType.NotFound => "요청한 항목을 찾을 수 없습니다.",
            ErrorType.Validation => ValidationMessages.InvalidInput,
            ErrorType.ServerError => ValidationMessages.ServerError,
            ErrorType.Cancelled => ValidationMessages.OperationCancelled,
            _ => "알 수 없는 오류가 발생했습니다."
        };
    }
}
