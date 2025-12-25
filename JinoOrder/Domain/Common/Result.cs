using System;

namespace JinoOrder.Domain.Common;

/// <summary>
/// 에러 유형
/// </summary>
public enum ErrorType
{
    None,
    Validation,
    Network,
    Timeout,
    Auth,
    NotFound,
    Conflict,
    ServerError,
    Cancelled,
    Unknown
}

/// <summary>
/// 작업 결과를 나타내는 제네릭 타입 (값 없음)
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ErrorType.None);

    public static Result Failure(string error, ErrorType errorType = ErrorType.Unknown)
        => new(false, error, errorType);

    public static Result Failure(Exception ex)
    {
        var (error, errorType) = ClassifyException(ex);
        return new Result(false, error, errorType);
    }

    public static Result ValidationFailure(string error)
        => new(false, error, ErrorType.Validation);

    public static Result NetworkFailure(string? error = null)
        => new(false, error ?? ValidationMessages.NetworkError, ErrorType.Network);

    public static Result TimeoutFailure(string? error = null)
        => new(false, error ?? ValidationMessages.TimeoutError, ErrorType.Timeout);

    public static Result AuthFailure(string? error = null)
        => new(false, error ?? ValidationMessages.InvalidCredentials, ErrorType.Auth);

    public static Result CancelledResult(string? error = null)
        => new(false, error ?? ValidationMessages.OperationCancelled, ErrorType.Cancelled);

    private static (string error, ErrorType type) ClassifyException(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException => (ValidationMessages.OperationCancelled, ErrorType.Cancelled),
            TimeoutException => (ValidationMessages.TimeoutError, ErrorType.Timeout),
            System.Net.Http.HttpRequestException => (ValidationMessages.NetworkError, ErrorType.Network),
            UnauthorizedAccessException => (ValidationMessages.InvalidCredentials, ErrorType.Auth),
            _ => (ex.Message, ErrorType.Unknown)
        };
    }
}

/// <summary>
/// 작업 결과를 나타내는 제네릭 타입 (값 포함)
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error, ErrorType errorType)
        : base(isSuccess, error, errorType)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null, ErrorType.None);

    public new static Result<T> Failure(string error, ErrorType errorType = ErrorType.Unknown)
        => new(false, default, error, errorType);

    public new static Result<T> Failure(Exception ex)
    {
        var (error, errorType) = ClassifyException(ex);
        return new Result<T>(false, default, error, errorType);
    }

    public new static Result<T> ValidationFailure(string error)
        => new(false, default, error, ErrorType.Validation);

    public new static Result<T> NetworkFailure(string? error = null)
        => new(false, default, error ?? ValidationMessages.NetworkError, ErrorType.Network);

    public new static Result<T> TimeoutFailure(string? error = null)
        => new(false, default, error ?? ValidationMessages.TimeoutError, ErrorType.Timeout);

    public new static Result<T> AuthFailure(string? error = null)
        => new(false, default, error ?? ValidationMessages.InvalidCredentials, ErrorType.Auth);

    public new static Result<T> CancelledResult(string? error = null)
        => new(false, default, error ?? ValidationMessages.OperationCancelled, ErrorType.Cancelled);

    private static (string error, ErrorType type) ClassifyException(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException => (ValidationMessages.OperationCancelled, ErrorType.Cancelled),
            TimeoutException => (ValidationMessages.TimeoutError, ErrorType.Timeout),
            System.Net.Http.HttpRequestException => (ValidationMessages.NetworkError, ErrorType.Network),
            UnauthorizedAccessException => (ValidationMessages.InvalidCredentials, ErrorType.Auth),
            _ => (ex.Message, ErrorType.Unknown)
        };
    }

    /// <summary>
    /// 값이 있으면 변환, 없으면 실패 전파
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure || Value == null)
            return Result<TNew>.Failure(Error ?? "값이 없습니다.", ErrorType);

        return Result<TNew>.Success(mapper(Value));
    }

    /// <summary>
    /// 값이 있으면 비동기 변환
    /// </summary>
    public async System.Threading.Tasks.Task<Result<TNew>> MapAsync<TNew>(Func<T, System.Threading.Tasks.Task<TNew>> mapper)
    {
        if (IsFailure || Value == null)
            return Result<TNew>.Failure(Error ?? "값이 없습니다.", ErrorType);

        var newValue = await mapper(Value);
        return Result<TNew>.Success(newValue);
    }

    /// <summary>
    /// 성공 시 값 반환, 실패 시 기본값 반환
    /// </summary>
    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess && Value != null ? Value : defaultValue;
    }

    /// <summary>
    /// 성공 시 값 반환, 실패 시 팩토리 함수 호출
    /// </summary>
    public T GetValueOrElse(Func<T> factory)
    {
        return IsSuccess && Value != null ? Value : factory();
    }

    /// <summary>
    /// 암시적 변환: T -> Result<T>
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}
