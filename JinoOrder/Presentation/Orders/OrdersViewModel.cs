using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Application.Orders;
using JinoOrder.Domain.Common;
using JinoOrder.Domain.Orders;
using JinoOrder.Presentation.Common;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Presentation.Orders;

public partial class OrdersViewModel : ViewModelBase
{
    private readonly IOrderService _orderService;
    private Func<int> _getPickupTime;

    [ObservableProperty] private ObservableCollection<Order> _pendingOrders = new();
    [ObservableProperty] private ObservableCollection<Order> _activeOrders = new();
    [ObservableProperty] private Order? _selectedOrder;
    [ObservableProperty] private int _pendingOrderCount;

    public bool HasPendingOrders => PendingOrderCount > 0;

    public OrdersViewModel(IOrderService orderService, ILogger<OrdersViewModel> logger, Func<int>? getPickupTime = null)
    {
        _orderService = orderService;
        _getPickupTime = getPickupTime ?? (() => AppConstants.DefaultMinPickupTime);
        Logger = logger;

        Logger.LogDebug("OrdersViewModel 초기화됨");
    }

    /// <summary>
    /// 픽업 시간 콜백 설정 (JinoOrderMainViewModel에서 연결)
    /// </summary>
    public void SetPickupTimeCallback(Func<int> getPickupTime)
    {
        _getPickupTime = getPickupTime;
    }

    public override void OnActivated()
    {
        base.OnActivated();
        _orderService.NewOrderReceived += OnNewOrderReceived;
        _orderService.OrderStatusChanged += OnOrderStatusChanged;

        Logger.LogDebug("OrdersViewModel 활성화됨");
        ExecuteAndForget(async _ => await LoadOrdersAsync(), "주문 목록 로드");
    }

    public override void OnDeactivated()
    {
        _orderService.NewOrderReceived -= OnNewOrderReceived;
        _orderService.OrderStatusChanged -= OnOrderStatusChanged;

        Logger.LogDebug("OrdersViewModel 비활성화됨");
        base.OnDeactivated();
    }

    public async Task LoadOrdersAsync()
    {
        Logger.LogDebug("주문 목록 로드 시작");

        await ExecuteAsync(async ct =>
        {
            var pending = await _orderService.GetPendingOrdersAsync();
            var active = await _orderService.GetActiveOrdersAsync();

            PendingOrders = new ObservableCollection<Order>(pending);
            ActiveOrders = new ObservableCollection<Order>(active);
            PendingOrderCount = pending.Count;
            OnPropertyChanged(nameof(HasPendingOrders));

            Logger.LogDebug("주문 목록 로드 완료: 대기중={PendingCount}, 활성={ActiveCount}",
                pending.Count, active.Count);
        }, "주문 목록 로드");
    }

    private void OnNewOrderReceived(object? sender, Order order)
    {
        Logger.LogInformation("새 주문 수신: OrderId={OrderId}, CustomerName={CustomerName}",
            order.Id, order.CustomerName);

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
        Logger.LogDebug("주문 상태 변경 수신: OrderId={OrderId}, Status={Status}", order.Id, order.Status);

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
    private async Task AcceptOrder(Order? order)
    {
        if (order == null) return;

        Logger.LogInformation("주문 수락 시도: OrderId={OrderId}", order.Id);

        var result = await ExecuteAsync(async _ =>
        {
            var success = await _orderService.AcceptOrderAsync(order.Id, _getPickupTime());
            if (success)
            {
                Logger.LogInformation("주문 수락 완료: OrderId={OrderId}", order.Id);
                await LoadOrdersAsync();
            }
            else
            {
                Logger.LogWarning("주문 수락 실패: OrderId={OrderId}", order.Id);
                SetError("주문을 수락하지 못했습니다.");
            }
        }, "주문 수락");
    }

    [RelayCommand]
    private async Task CompleteOrder(Order? order)
    {
        if (order == null) return;

        Logger.LogInformation("주문 완료 처리 시도: OrderId={OrderId}", order.Id);

        await ExecuteAsync(async _ =>
        {
            var success = await _orderService.CompleteOrderAsync(order.Id);
            if (success)
            {
                Logger.LogInformation("주문 완료 처리됨: OrderId={OrderId}", order.Id);
                await LoadOrdersAsync();
            }
            else
            {
                Logger.LogWarning("주문 완료 처리 실패: OrderId={OrderId}", order.Id);
                SetError("주문을 완료 처리하지 못했습니다.");
            }
        }, "주문 완료");
    }

    [RelayCommand]
    private async Task CancelOrder(Order? order)
    {
        if (order == null) return;

        Logger.LogInformation("주문 취소 시도: OrderId={OrderId}", order.Id);

        await ExecuteAsync(async _ =>
        {
            var success = await _orderService.CancelOrderAsync(order.Id, CancellationReasons.StoreReason);
            if (success)
            {
                Logger.LogInformation("주문 취소됨: OrderId={OrderId}", order.Id);
                await LoadOrdersAsync();
            }
            else
            {
                Logger.LogWarning("주문 취소 실패: OrderId={OrderId}", order.Id);
                SetError("주문을 취소하지 못했습니다.");
            }
        }, "주문 취소");
    }

    [RelayCommand]
    private async Task MarkAsPickedUp(Order? order)
    {
        if (order == null) return;

        Logger.LogInformation("픽업 완료 처리 시도: OrderId={OrderId}", order.Id);

        await ExecuteAsync(async _ =>
        {
            var success = await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);
            if (success)
            {
                Logger.LogInformation("픽업 완료 처리됨: OrderId={OrderId}", order.Id);
                await LoadOrdersAsync();
            }
            else
            {
                Logger.LogWarning("픽업 완료 처리 실패: OrderId={OrderId}", order.Id);
                SetError("픽업 완료 처리를 하지 못했습니다.");
            }
        }, "픽업 완료");
    }

    [RelayCommand]
    private void SelectOrder(Order? order)
    {
        SelectedOrder = order;
        Logger.LogDebug("주문 선택됨: OrderId={OrderId}", order?.Id);
    }
}
