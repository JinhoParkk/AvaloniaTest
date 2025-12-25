namespace JinoOrder.Domain.Common;

/// <summary>
/// API 호출 결과를 래핑하는 제네릭 Result 타입
/// </summary>
public class ApiResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; }
    public ApiError? Error { get; }

    private ApiResult(bool isSuccess, T? data, ApiError? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static ApiResult<T> Success(T data)
        => new(true, data, null);

    public static ApiResult<T> Failure(string message, int statusCode = 0)
        => new(false, default, new ApiError(message, statusCode));

    public static ApiResult<T> Failure(ApiError error)
        => new(false, default, error);

    public static ApiResult<T> Unauthorized(string message = "Unauthorized")
        => new(false, default, new ApiError(message, 401));
}

/// <summary>
/// API 에러 정보
/// </summary>
public class ApiError
{
    public string Message { get; }
    public int StatusCode { get; }
    public bool IsUnauthorized => StatusCode == 401;
    public bool IsNotFound => StatusCode == 404;
    public bool IsServerError => StatusCode >= 500;

    public ApiError(string message, int statusCode)
    {
        Message = message;
        StatusCode = statusCode;
    }
}
