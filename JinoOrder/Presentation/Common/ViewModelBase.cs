using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JinoOrder.Domain.Common;
using JinoOrder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Presentation.Common;

public abstract partial class ViewModelBase : ObservableObject, IDisposable
{
    private bool _disposed;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    /// <summary>
    /// 로거 (자식 클래스에서 주입받아 설정)
    /// </summary>
    protected ILogger? Logger { get; set; }

    /// <summary>
    /// 취소 토큰
    /// </summary>
    protected CancellationToken CancellationToken => _cts?.Token ?? CancellationToken.None;

    /// <summary>
    /// 화면이 활성화될 때 호출됩니다.
    /// </summary>
    public virtual void OnActivated()
    {
        _cts = new CancellationTokenSource();
        ClearError();
    }

    /// <summary>
    /// 화면이 비활성화될 때 호출됩니다.
    /// </summary>
    public virtual void OnDeactivated()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// 에러 메시지를 설정합니다.
    /// </summary>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = !string.IsNullOrEmpty(message);
    }

    /// <summary>
    /// 에러를 지웁니다.
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    /// <summary>
    /// Result의 에러를 설정합니다.
    /// </summary>
    protected void SetError(Result result)
    {
        if (result.IsFailure)
        {
            SetError(result.Error ?? ExceptionHandler.GetUserFriendlyMessage(result.ErrorType));
        }
    }

    /// <summary>
    /// 비동기 작업을 안전하게 실행합니다. (로딩 상태, 에러 처리 포함)
    /// </summary>
    protected async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task> action,
        string? operationName = null,
        bool showLoading = true)
    {
        if (showLoading) IsLoading = true;
        ClearError();

        try
        {
            await action(CancellationToken);
            return Result.Success();
        }
        catch (OperationCanceledException) when (CancellationToken.IsCancellationRequested)
        {
            Logger?.LogDebug("작업이 취소되었습니다: {Operation}", operationName);
            return Result.CancelledResult();
        }
        catch (Exception ex)
        {
            var result = ExceptionHandler.HandleException(ex, Logger, operationName);
            SetError(result);
            return result;
        }
        finally
        {
            if (showLoading) IsLoading = false;
        }
    }

    /// <summary>
    /// 비동기 작업을 안전하게 실행하고 결과를 반환합니다.
    /// </summary>
    protected async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        string? operationName = null,
        bool showLoading = true)
    {
        if (showLoading) IsLoading = true;
        ClearError();

        try
        {
            var result = await action(CancellationToken);
            return Result<T>.Success(result);
        }
        catch (OperationCanceledException) when (CancellationToken.IsCancellationRequested)
        {
            Logger?.LogDebug("작업이 취소되었습니다: {Operation}", operationName);
            return Result<T>.CancelledResult();
        }
        catch (Exception ex)
        {
            var result = ExceptionHandler.HandleException<T>(ex, Logger, operationName);
            SetError(result);
            return result;
        }
        finally
        {
            if (showLoading) IsLoading = false;
        }
    }

    /// <summary>
    /// Fire-and-forget 작업을 안전하게 실행합니다. (예외 무시 방지)
    /// </summary>
    protected async void ExecuteAndForget(
        Func<CancellationToken, Task> action,
        string? operationName = null)
    {
        try
        {
            await action(CancellationToken);
        }
        catch (OperationCanceledException) when (CancellationToken.IsCancellationRequested)
        {
            Logger?.LogDebug("Fire-and-forget 작업이 취소되었습니다: {Operation}", operationName);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Fire-and-forget 작업에서 오류 발생: {Operation}", operationName);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            OnDeactivated();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
