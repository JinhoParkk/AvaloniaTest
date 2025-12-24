using System;
using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaApplication1.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    private readonly NavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public MainWindowViewModel(
        NavigationService navigationService,
        IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;

        // 초기 ViewModel은 NavigationService에서 설정
        _currentViewModel = null!;
        _navigationService.CurrentViewModelChanged += OnNavigationRequested;
    }

    /// <summary>
    /// 초기 ViewModel 설정 (App 초기화 시 호출)
    /// </summary>
    public void SetInitialViewModel(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
    }

    /// <summary>
    /// ViewModel 타입으로 네비게이션 (DI에서 resolve)
    /// </summary>
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewModel = viewModel;
    }

    private void OnNavigationRequested(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
    }
}
