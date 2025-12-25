using JinoOrder.Presentation.Common;

namespace JinoOrder.Application.Common;

/// <summary>
/// ViewModel 인스턴스를 생성하는 팩토리 인터페이스
/// Service Locator 패턴 대신 DI를 캡슐화합니다.
/// </summary>
public interface IViewModelFactory
{
    /// <summary>
    /// 지정된 타입의 ViewModel을 생성합니다.
    /// </summary>
    TViewModel Create<TViewModel>() where TViewModel : ViewModelBase;
}
