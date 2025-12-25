using AvaloniaApplication1.Models.JinoOrder;

namespace AvaloniaApplication1.Services.JinoOrder;

/// <summary>
/// 지노오더 서비스 인터페이스
/// </summary>
public interface IJinoOrderService
{
    // 주문 관련
    Task<List<Order>> GetPendingOrdersAsync();
    Task<List<Order>> GetActiveOrdersAsync();
    Task<List<Order>> GetOrdersByDateAsync(DateTime date);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<bool> AcceptOrderAsync(int orderId, int estimatedMinutes);
    Task<bool> CompleteOrderAsync(int orderId);
    Task<bool> CancelOrderAsync(int orderId, string reason);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);

    // 메뉴 관련
    Task<List<MenuCategory>> GetCategoriesAsync();
    Task<List<MenuItem>> GetMenuItemsAsync();
    Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId);
    Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId);
    Task<bool> UpdateMenuItemAsync(MenuItem item);
    Task<bool> ToggleMenuItemAvailabilityAsync(int menuItemId, bool isAvailable);
    Task<bool> ToggleMenuItemSoldOutAsync(int menuItemId, bool isSoldOut);

    // 고객 관련
    Task<List<Customer>> GetCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    Task<Customer?> GetCustomerByPhoneAsync(string phone);
    Task<bool> AddPointsAsync(int customerId, decimal points, string description, int? orderId = null);
    Task<bool> UsePointsAsync(int customerId, decimal points, string description, int? orderId = null);
    Task<List<PointHistory>> GetPointHistoryAsync(int customerId);

    // 통계 관련
    Task<DailySummary> GetDailySummaryAsync(DateTime date);
    Task<List<DailySummary>> GetWeeklySummaryAsync(DateTime startDate);
    Task<List<PopularMenuItem>> GetPopularMenuItemsAsync(DateTime startDate, DateTime endDate, int limit = 10);
    Task<List<HourlyStats>> GetHourlyStatsAsync(DateTime date);

    // 실시간 이벤트
    event EventHandler<Order>? NewOrderReceived;
    event EventHandler<Order>? OrderStatusChanged;
}
