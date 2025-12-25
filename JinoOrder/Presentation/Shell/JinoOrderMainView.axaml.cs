using System;
using Avalonia.Controls;

namespace JinoOrder.Presentation.Shell;

public partial class JinoOrderMainView : UserControl
{
    public JinoOrderMainView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is JinoOrderMainViewModel viewModel)
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
