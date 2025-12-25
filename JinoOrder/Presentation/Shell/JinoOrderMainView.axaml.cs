using System;
using Avalonia.Controls;

namespace JinoOrder.Presentation.Shell;

public partial class JinoOrderMainView : UserControl
{
    public JinoOrderMainView()
    {
        InitializeComponent();
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
