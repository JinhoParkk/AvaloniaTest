using JinoOrder.Domain.Orders;

namespace JinoOrder.Application.Orders;

/// <summary>
/// 주문 서비스 인터페이스
/// </summary>
public interface IOrderService
{
    Task<List<Order>> GetPendingOrdersAsync();
    Task<List<Order>> GetActiveOrdersAsync();
    Task<List<Order>> GetOrdersByDateAsync(DateTime date);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<bool> AcceptOrderAsync(int orderId, int estimatedMinutes);
    Task<bool> CompleteOrderAsync(int orderId);
    Task<bool> CancelOrderAsync(int orderId, string reason);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);

    // 실시간 이벤트
    event EventHandler<Order>? NewOrderReceived;
    event EventHandler<Order>? OrderStatusChanged;
}
