using System;
using AvaloniaApplication1.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.Services;

/// <summary>
/// 타입 기반 네비게이션 서비스 (DI 통합)
/// </summary>
public class NavigationService
{
    private IServiceProvider? _serviceProvider;

    public event Action<ViewModelBase>? CurrentViewModelChanged;

    /// <summary>
    /// ServiceProvider 초기화 (App 시작 시 호출)
    /// </summary>
    public void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 타입 기반 네비게이션 (권장)
    /// </summary>
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        if (_serviceProvider is null)
            throw new InvalidOperationException("NavigationService has not been initialized.");

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewModelChanged?.Invoke(viewModel);
    }

    /// <summary>
    /// 인스턴스 기반 네비게이션 (하위 호환)
    /// </summary>
    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModelChanged?.Invoke(viewModel);
    }
}
