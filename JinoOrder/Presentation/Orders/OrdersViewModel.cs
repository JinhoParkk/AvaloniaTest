using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Application.Orders;
using JinoOrder.Domain.Orders;
using JinoOrder.Presentation.Common;

namespace JinoOrder.Presentation.Orders;

public partial class OrdersViewModel : ViewModelBase
{
    private readonly IOrderService _orderService;
    private readonly Func<int> _getPickupTime;

    [ObservableProperty] private ObservableCollection<Order> _pendingOrders = new();
    [ObservableProperty] private ObservableCollection<Order> _activeOrders = new();
    [ObservableProperty] private Order? _selectedOrder;
    [ObservableProperty] private int _pendingOrderCount;
    [ObservableProperty] private bool _isLoading;

    public bool HasPendingOrders => PendingOrderCount > 0;

    public OrdersViewModel(IOrderService orderService, Func<int>? getPickupTime = null)
    {
        _orderService = orderService;
        _getPickupTime = getPickupTime ?? (() => 10);
    }

    public override void OnActivated()
    {
        base.OnActivated();
        _orderService.NewOrderReceived += OnNewOrderReceived;
        _orderService.OrderStatusChanged += OnOrderStatusChanged;
        _ = LoadOrdersAsync();
    }

    public override void OnDeactivated()
    {
        _orderService.NewOrderReceived -= OnNewOrderReceived;
        _orderService.OrderStatusChanged -= OnOrderStatusChanged;
        base.OnDeactivated();
    }

    public async Task LoadOrdersAsync()
    {
        IsLoading = true;
        try
        {
            var pending = await _orderService.GetPendingOrdersAsync();
            var active = await _orderService.GetActiveOrdersAsync();

            PendingOrders = new ObservableCollection<Order>(pending);
            ActiveOrders = new ObservableCollection<Order>(active);
            PendingOrderCount = pending.Count;
            OnPropertyChanged(nameof(HasPendingOrders));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnNewOrderReceived(object? sender, Order order)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            PendingOrders.Insert(0, order);
            ActiveOrders.Insert(0, order);
            PendingOrderCount = PendingOrders.Count;
            OnPropertyChanged(nameof(HasPendingOrders));
        });
    }

    private void OnOrderStatusChanged(object? sender, Order order)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var pendingOrder = PendingOrders.FirstOrDefault(o => o.Id == order.Id);
            if (pendingOrder != null && order.Status != OrderStatus.Pending)
            {
                PendingOrders.Remove(pendingOrder);
                PendingOrderCount = PendingOrders.Count;
                OnPropertyChanged(nameof(HasPendingOrders));
            }

            var activeOrder = ActiveOrders.FirstOrDefault(o => o.Id == order.Id);
            if (activeOrder != null)
            {
                var index = ActiveOrders.IndexOf(activeOrder);
                ActiveOrders[index] = order;

                if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                {
                    ActiveOrders.Remove(order);
                }
            }
        });
    }

    [RelayCommand]
    private async Task AcceptOrder(Order order)
    {
        if (order == null) return;

        var success = await _orderService.AcceptOrderAsync(order.Id, _getPickupTime());
        if (success)
        {
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task CompleteOrder(Order order)
    {
        if (order == null) return;

        var success = await _orderService.CompleteOrderAsync(order.Id);
        if (success)
        {
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task CancelOrder(Order order)
    {
        if (order == null) return;

        var success = await _orderService.CancelOrderAsync(order.Id, "가게 사정으로 취소");
        if (success)
        {
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task MarkAsPickedUp(Order order)
    {
        if (order == null) return;

        var success = await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);
        if (success)
        {
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private void SelectOrder(Order order)
    {
        SelectedOrder = order;
    }
}
