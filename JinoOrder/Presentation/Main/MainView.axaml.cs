using System;
using Avalonia.Controls;
using JinoOrder.Presentation.Common;
using JinoOrder.Presentation.Main;

namespace JinoOrder.Presentation.Main;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.MinimizeRequested += OnMinimizeRequested;
            viewModel.MaximizeRequested += OnMaximizeRequested;
            viewModel.CloseRequested += OnCloseRequested;
        }
    }

    private void OnMinimizeRequested()
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window != null)
        {
            window.WindowState = WindowState.Minimized;
        }
    }

    private void OnMaximizeRequested()
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window != null)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    private void OnCloseRequested()
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        window?.Close();
    }
}
