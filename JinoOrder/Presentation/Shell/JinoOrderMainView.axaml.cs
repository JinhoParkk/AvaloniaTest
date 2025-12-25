using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using JinoOrder.Presentation.Orders;
using JinoOrder.Presentation.Menu;
using JinoOrder.Presentation.Customers;
using JinoOrder.Presentation.Statistics;
using JinoOrder.Presentation.History;
using JinoOrder.Presentation.Settings;

namespace JinoOrder.Presentation.Shell;

public partial class JinoOrderMainView : UserControl
{
    private Frame? _contentFrame;
    private NavigationView? _navView;

    public JinoOrderMainView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _contentFrame = this.FindControl<Frame>("ContentFrame");
        _navView = this.FindControl<NavigationView>("NavView");

        // 초기 페이지로 주문접수 선택
        if (_navView?.MenuItems.Count > 0)
        {
            _navView.SelectedItem = _navView.MenuItems[0];
            NavigateToPage("orders");
        }
    }

    private void NavigationView_SelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavigateToPage("settings");
            return;
        }

        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateToPage(tag);

            // ViewModel 동기화
            if (DataContext is JinoOrderMainViewModel viewModel)
            {
                viewModel.SelectMenuCommand.Execute(tag);
            }
        }
    }

    private void NavigateToPage(string tag)
    {
        if (_contentFrame == null) return;

        Type? pageType = tag switch
        {
            "orders" => typeof(OrdersPage),
            "history" => typeof(HistoryPage),
            "menu" => typeof(MenuPage),
            "customers" => typeof(CustomersPage),
            "stats" => typeof(StatsPage),
            "settings" => typeof(SettingsPage),
            _ => null
        };

        if (pageType != null)
        {
            _contentFrame.Navigate(pageType, DataContext);
        }
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
