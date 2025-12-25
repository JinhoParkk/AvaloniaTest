using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JinoOrder.Application.Customers;
using JinoOrder.Domain.Customers;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Infrastructure.Services.Mock;

/// <summary>
/// 고객 Mock 서비스
/// </summary>
public class MockCustomerService : ICustomerService
{
    private readonly MockDataStore _dataStore;
    private readonly ILogger<MockCustomerService> _logger;

    public MockCustomerService(MockDataStore dataStore, ILogger<MockCustomerService> logger)
    {
        _dataStore = dataStore;
        _logger = logger;

        _logger.LogDebug("MockCustomerService 초기화됨");
    }

    public Task<List<Customer>> GetCustomersAsync()
    {
        _logger.LogDebug("고객 목록 조회");
        var customers = _dataStore.Customers.OrderByDescending(c => c.LastVisitAt).ToList();
        _logger.LogDebug("고객 {Count}명 조회됨", customers.Count);
        return Task.FromResult(customers);
    }

    public Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        _logger.LogDebug("고객 상세 조회: CustomerId={CustomerId}", customerId);
        var customer = _dataStore.Customers.FirstOrDefault(c => c.Id == customerId);

        if (customer == null)
        {
            _logger.LogWarning("고객을 찾을 수 없음: CustomerId={CustomerId}", customerId);
        }

        return Task.FromResult(customer);
    }

    public Task<Customer?> GetCustomerByPhoneAsync(string phone)
    {
        _logger.LogDebug("전화번호로 고객 조회: Phone={Phone}", phone);
        var customer = _dataStore.Customers.FirstOrDefault(c => c.Phone == phone);

        if (customer == null)
        {
            _logger.LogDebug("전화번호로 고객을 찾을 수 없음: Phone={Phone}", phone);
        }

        return Task.FromResult(customer);
    }

    public Task<bool> AddPointsAsync(int customerId, decimal points, string description, int? orderId = null)
    {
        _logger.LogInformation("포인트 적립: CustomerId={CustomerId}, Points={Points}, Description={Description}",
            customerId, points, description);

        var customer = _dataStore.Customers.FirstOrDefault(c => c.Id == customerId);
        if (customer == null)
        {
            _logger.LogWarning("포인트 적립 실패 - 고객을 찾을 수 없음: CustomerId={CustomerId}", customerId);
            return Task.FromResult(false);
        }

        customer.Points += points;
        _dataStore.PointHistories.Add(new PointHistory
        {
            Id = _dataStore.PointHistories.Count + 1,
            CustomerId = customerId,
            Amount = points,
            Description = description,
            CreatedAt = DateTime.Now,
            OrderId = orderId
        });

        _logger.LogInformation("포인트 적립 완료: CustomerId={CustomerId}, NewBalance={Balance}", customerId, customer.Points);
        return Task.FromResult(true);
    }

    public Task<bool> UsePointsAsync(int customerId, decimal points, string description, int? orderId = null)
    {
        _logger.LogInformation("포인트 사용: CustomerId={CustomerId}, Points={Points}, Description={Description}",
            customerId, points, description);

        var customer = _dataStore.Customers.FirstOrDefault(c => c.Id == customerId);
        if (customer == null)
        {
            _logger.LogWarning("포인트 사용 실패 - 고객을 찾을 수 없음: CustomerId={CustomerId}", customerId);
            return Task.FromResult(false);
        }

        if (customer.Points < points)
        {
            _logger.LogWarning("포인트 사용 실패 - 포인트 부족: CustomerId={CustomerId}, Balance={Balance}, Requested={Requested}",
                customerId, customer.Points, points);
            return Task.FromResult(false);
        }

        customer.Points -= points;
        _dataStore.PointHistories.Add(new PointHistory
        {
            Id = _dataStore.PointHistories.Count + 1,
            CustomerId = customerId,
            Amount = -points,
            Description = description,
            CreatedAt = DateTime.Now,
            OrderId = orderId
        });

        _logger.LogInformation("포인트 사용 완료: CustomerId={CustomerId}, NewBalance={Balance}", customerId, customer.Points);
        return Task.FromResult(true);
    }

    public Task<List<PointHistory>> GetPointHistoryAsync(int customerId)
    {
        _logger.LogDebug("포인트 이력 조회: CustomerId={CustomerId}", customerId);
        var history = _dataStore.PointHistories
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        _logger.LogDebug("포인트 이력 {Count}건 조회됨", history.Count);
        return Task.FromResult(history);
    }
}
