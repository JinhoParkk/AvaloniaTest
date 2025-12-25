using CommunityToolkit.Mvvm.ComponentModel;

namespace JinoOrder.Presentation.Common;

public abstract class ViewModelBase : ObservableObject
{
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
}
