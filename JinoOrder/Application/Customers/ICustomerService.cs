using JinoOrder.Domain.Customers;

namespace JinoOrder.Application.Customers;

/// <summary>
/// 고객 서비스 인터페이스
/// </summary>
public interface ICustomerService
{
    Task<List<Customer>> GetCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    Task<Customer?> GetCustomerByPhoneAsync(string phone);
    Task<bool> AddPointsAsync(int customerId, decimal points, string description, int? orderId = null);
    Task<bool> UsePointsAsync(int customerId, decimal points, string description, int? orderId = null);
    Task<List<PointHistory>> GetPointHistoryAsync(int customerId);
}
