using System;
using System.Collections.Generic;
using JinoOrder.Presentation.Common;
using JinoOrder.Presentation.Main;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace JinoOrder.Infrastructure.Services;

/// <summary>
/// 타입 기반 네비게이션 서비스 (DI 통합, 뒤로/앞으로 가기 지원)
/// </summary>
public partial class NavigationService : ObservableObject
{
    private IServiceProvider? _serviceProvider;
    private readonly Stack<ViewModelBase> _backStack = new();
    private readonly Stack<ViewModelBase> _forwardStack = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(CanGoForward))]
    private ViewModelBase? _currentViewModel;

    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;

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
        NavigateToInternal(viewModel);
    }

    /// <summary>
    /// 인스턴스 기반 네비게이션 (하위 호환)
    /// </summary>
    public void NavigateTo(ViewModelBase viewModel)
    {
        NavigateToInternal(viewModel);
    }

    private void NavigateToInternal(ViewModelBase viewModel)
    {
        if (CurrentViewModel != null)
        {
            _backStack.Push(CurrentViewModel);
            CurrentViewModel.OnDeactivated();
        }

        _forwardStack.Clear();

        CurrentViewModel = viewModel;
        viewModel.OnActivated();

        CurrentViewModelChanged?.Invoke(viewModel);
    }

    /// <summary>
    /// 뒤로 가기
    /// </summary>
    public bool GoBack()
    {
        if (!CanGoBack) return false;

        if (CurrentViewModel != null)
        {
            _forwardStack.Push(CurrentViewModel);
            CurrentViewModel.OnDeactivated();
        }

        CurrentViewModel = _backStack.Pop();
        CurrentViewModel.OnActivated();

        CurrentViewModelChanged?.Invoke(CurrentViewModel);
        return true;
    }

    /// <summary>
    /// 앞으로 가기
    /// </summary>
    public bool GoForward()
    {
        if (!CanGoForward) return false;

        if (CurrentViewModel != null)
        {
            _backStack.Push(CurrentViewModel);
            CurrentViewModel.OnDeactivated();
        }

        CurrentViewModel = _forwardStack.Pop();
        CurrentViewModel.OnActivated();

        CurrentViewModelChanged?.Invoke(CurrentViewModel);
        return true;
    }

    /// <summary>
    /// 네비게이션 히스토리 초기화
    /// </summary>
    public void ClearHistory()
    {
        _backStack.Clear();
        _forwardStack.Clear();
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }
}
