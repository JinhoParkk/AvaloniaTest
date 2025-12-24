using System;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Services;

public class NavigationService
{
    public event Action<ViewModelBase>? CurrentViewModelChanged;

    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModelChanged?.Invoke(viewModel);
    }
}
