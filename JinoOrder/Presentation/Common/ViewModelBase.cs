using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JinoOrder.Presentation.Common;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private bool _disposed;

    /// <summary>
    /// 화면이 활성화될 때 호출됩니다.
    /// </summary>
    public virtual void OnActivated()
    {
    }

    /// <summary>
    /// 화면이 비활성화될 때 호출됩니다.
    /// </summary>
    public virtual void OnDeactivated()
    {
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
