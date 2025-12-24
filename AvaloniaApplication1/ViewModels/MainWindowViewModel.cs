using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    private readonly NavigationService _navigationService;

    public MainWindowViewModel(NavigationService navigationService, ViewModelBase initialViewModel)
    {
        _navigationService = navigationService;
        _currentViewModel = initialViewModel;

        _navigationService.CurrentViewModelChanged += OnNavigationRequested;
    }

    private void OnNavigationRequested(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
    }
}
