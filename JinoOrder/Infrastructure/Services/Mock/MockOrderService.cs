using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JinoOrder.Application.Orders;
using JinoOrder.Domain.Common;
using JinoOrder.Domain.Orders;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Infrastructure.Services.Mock;

/// <summary>
/// 주문 Mock 서비스
/// </summary>
public class MockOrderService : IOrderService
{
    private readonly MockDataStore _dataStore;
    private readonly ILogger<MockOrderService> _logger;

    public event EventHandler<Order>? NewOrderReceived;
    public event EventHandler<Order>? OrderStatusChanged;

    public MockOrderService(MockDataStore dataStore, ILogger<MockOrderService> logger)
    {
        _dataStore = dataStore;
        _logger = logger;

        // 데이터 저장소의 이벤트를 전파
        _dataStore.NewOrderReceived += (s, order) => NewOrderReceived?.Invoke(this, order);
        _dataStore.OrderStatusChanged += (s, order) => OrderStatusChanged?.Invoke(this, order);

        _logger.LogDebug("MockOrderService 초기화됨");
    }

    public Task<List<Order>> GetPendingOrdersAsync()
    {
        _logger.LogDebug("대기 중 주문 조회");
        var pending = _dataStore.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.OrderedAt)
            .ToList();

        _logger.LogDebug("대기 중 주문 {Count}개 조회됨", pending.Count);
        return Task.FromResult(pending);
    }

    public Task<List<Order>> GetActiveOrdersAsync()
    {
        _logger.LogDebug("활성 주문 조회");
        var active = _dataStore.Orders
            .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.OrderedAt)
            .ToList();

        _logger.LogDebug("활성 주문 {Count}개 조회됨", active.Count);
        return Task.FromResult(active);
    }

    public Task<List<Order>> GetOrdersByDateAsync(DateTime date)
    {
        _logger.LogDebug("날짜별 주문 조회: {Date}", date.ToShortDateString());
        var orders = _dataStore.Orders
            .Where(o => o.OrderedAt.Date == date.Date)
            .OrderByDescending(o => o.OrderedAt)
            .ToList();

        _logger.LogDebug("해당 날짜 주문 {Count}개 조회됨", orders.Count);
        return Task.FromResult(orders);
    }

    public Task<Order?> GetOrderByIdAsync(int orderId)
    {
        _logger.LogDebug("주문 상세 조회: OrderId={OrderId}", orderId);
        var order = _dataStore.Orders.FirstOrDefault(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("주문을 찾을 수 없음: OrderId={OrderId}", orderId);
        }

        return Task.FromResult(order);
    }

    public Task<bool> AcceptOrderAsync(int orderId, int estimatedMinutes)
    {
        _logger.LogInformation("주문 수락 시도: OrderId={OrderId}, EstimatedMinutes={EstimatedMinutes}", orderId, estimatedMinutes);

        var order = _dataStore.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
        {
            _logger.LogWarning("주문 수락 실패 - 주문을 찾을 수 없음: OrderId={OrderId}", orderId);
            return Task.FromResult(false);
        }

        order.Status = OrderStatus.Accepted;
        order.AcceptedAt = DateTime.Now;
        order.EstimatedMinutes = estimatedMinutes;
        order.EstimatedPickupTime = DateTime.Now.AddMinutes(estimatedMinutes);

        _logger.LogInformation("주문 수락 완료: OrderId={OrderId}, EstimatedPickupTime={PickupTime}",
            orderId, order.EstimatedPickupTime);

        _dataStore.RaiseOrderStatusChanged(order);
        return Task.FromResult(true);
    }

    public Task<bool> CompleteOrderAsync(int orderId)
    {
        _logger.LogInformation("주문 완료 처리 시도: OrderId={OrderId}", orderId);

        var order = _dataStore.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
        {
            _logger.LogWarning("주문 완료 처리 실패 - 주문을 찾을 수 없음: OrderId={OrderId}", orderId);
            return Task.FromResult(false);
        }

        order.Status = OrderStatus.Ready;
        order.CompletedAt = DateTime.Now;

        _logger.LogInformation("주문 완료 처리 완료: OrderId={OrderId}", orderId);

        _dataStore.RaiseOrderStatusChanged(order);
        return Task.FromResult(true);
    }

    public Task<bool> CancelOrderAsync(int orderId, string reason)
    {
        _logger.LogInformation("주문 취소 시도: OrderId={OrderId}, Reason={Reason}", orderId, reason);

        var order = _dataStore.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
        {
            _logger.LogWarning("주문 취소 실패 - 주문을 찾을 수 없음: OrderId={OrderId}", orderId);
            return Task.FromResult(false);
        }

        order.Status = OrderStatus.Cancelled;
        order.Memo = reason;

        _logger.LogInformation("주문 취소 완료: OrderId={OrderId}", orderId);

        _dataStore.RaiseOrderStatusChanged(order);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        _logger.LogInformation("주문 상태 업데이트: OrderId={OrderId}, NewStatus={Status}", orderId, status);

        var order = _dataStore.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
        {
            _logger.LogWarning("주문 상태 업데이트 실패 - 주문을 찾을 수 없음: OrderId={OrderId}", orderId);
            return Task.FromResult(false);
        }

        order.Status = status;
        if (status == OrderStatus.Completed)
            order.CompletedAt = DateTime.Now;

        _logger.LogInformation("주문 상태 업데이트 완료: OrderId={OrderId}, Status={Status}", orderId, status);

        _dataStore.RaiseOrderStatusChanged(order);
        return Task.FromResult(true);
    }
}
