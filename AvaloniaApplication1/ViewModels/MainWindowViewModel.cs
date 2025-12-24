using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    private readonly NavigationService _navigationService;

    public bool CanGoBack => _navigationService.CanGoBack;
    public bool CanGoForward => _navigationService.CanGoForward;

    public MainWindowViewModel(NavigationService navigationService)
    {
        _navigationService = navigationService;

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

    private void OnNavigationRequested(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    [RelayCommand]
    private void GoForward()
    {
        _navigationService.GoForward();
    }
}
