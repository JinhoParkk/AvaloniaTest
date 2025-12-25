using System.Timers;
using JinoOrder.Application.Customers;
using JinoOrder.Application.Menu;
using JinoOrder.Application.Orders;
using JinoOrder.Application.Statistics;
using JinoOrder.Domain.Customers;
using JinoOrder.Domain.Menu;
using JinoOrder.Domain.Orders;
using JinoOrder.Domain.Statistics;
using Timer = System.Timers.Timer;

namespace JinoOrder.Infrastructure.Services;

/// <summary>
/// Mock 지노오더 서비스 (API 없이 동작하는 더미 데이터)
/// </summary>
public class MockJinoOrderService : IOrderService, IMenuService, ICustomerService, IStatisticsService, IDisposable
{
    private readonly List<MenuCategory> _categories;
    private readonly List<MenuItem> _menuItems;
    private readonly List<Order> _orders;
    private readonly List<Customer> _customers;
    private readonly List<PointHistory> _pointHistories;
    private readonly Timer _newOrderTimer;
    private readonly Random _random = new();
    private int _nextOrderId = 1000;

    public event EventHandler<Order>? NewOrderReceived;
    public event EventHandler<Order>? OrderStatusChanged;

    public MockJinoOrderService()
    {
        _categories = GenerateCategories();
        _menuItems = GenerateMenuItems();
        _customers = GenerateCustomers();
        _orders = GenerateOrders();
        _pointHistories = GeneratePointHistories();

        // 데모용: 30초마다 새 주문 생성
        _newOrderTimer = new Timer(30000);
        _newOrderTimer.Elapsed += OnNewOrderTimerElapsed;
    }

    public void StartSimulation()
    {
        _newOrderTimer.Start();
    }

    public void StopSimulation()
    {
        _newOrderTimer.Stop();
    }

    private void OnNewOrderTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var newOrder = GenerateRandomOrder();
        _orders.Insert(0, newOrder);
        NewOrderReceived?.Invoke(this, newOrder);
    }

    #region 주문 관련

    public Task<List<Order>> GetPendingOrdersAsync()
    {
        var pending = _orders
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.OrderedAt)
            .ToList();
        return Task.FromResult(pending);
    }

    public Task<List<Order>> GetActiveOrdersAsync()
    {
        var active = _orders
            .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.OrderedAt)
            .ToList();
        return Task.FromResult(active);
    }

    public Task<List<Order>> GetOrdersByDateAsync(DateTime date)
    {
        var orders = _orders
            .Where(o => o.OrderedAt.Date == date.Date)
            .OrderByDescending(o => o.OrderedAt)
            .ToList();
        return Task.FromResult(orders);
    }

    public Task<Order?> GetOrderByIdAsync(int orderId)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        return Task.FromResult(order);
    }

    public Task<bool> AcceptOrderAsync(int orderId, int estimatedMinutes)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return Task.FromResult(false);

        order.Status = OrderStatus.Accepted;
        order.AcceptedAt = DateTime.Now;
        order.EstimatedMinutes = estimatedMinutes;
        order.EstimatedPickupTime = DateTime.Now.AddMinutes(estimatedMinutes);

        OrderStatusChanged?.Invoke(this, order);
        return Task.FromResult(true);
    }

    public Task<bool> CompleteOrderAsync(int orderId)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return Task.FromResult(false);

        order.Status = OrderStatus.Ready;
        order.CompletedAt = DateTime.Now;

        OrderStatusChanged?.Invoke(this, order);
        return Task.FromResult(true);
    }

    public Task<bool> CancelOrderAsync(int orderId, string reason)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return Task.FromResult(false);

        order.Status = OrderStatus.Cancelled;
        order.Memo = reason;

        OrderStatusChanged?.Invoke(this, order);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return Task.FromResult(false);

        order.Status = status;
        if (status == OrderStatus.Completed)
            order.CompletedAt = DateTime.Now;

        OrderStatusChanged?.Invoke(this, order);
        return Task.FromResult(true);
    }

    #endregion

    #region 메뉴 관련

    public Task<List<MenuCategory>> GetCategoriesAsync()
    {
        return Task.FromResult(_categories.ToList());
    }

    public Task<List<MenuItem>> GetMenuItemsAsync()
    {
        return Task.FromResult(_menuItems.ToList());
    }

    public Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        var items = _menuItems.Where(m => m.CategoryId == categoryId).ToList();
        return Task.FromResult(items);
    }

    public Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId)
    {
        var item = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
        return Task.FromResult(item);
    }

    public Task<bool> UpdateMenuItemAsync(MenuItem item)
    {
        var existing = _menuItems.FirstOrDefault(m => m.Id == item.Id);
        if (existing == null) return Task.FromResult(false);

        var index = _menuItems.IndexOf(existing);
        _menuItems[index] = item;
        return Task.FromResult(true);
    }

    public Task<bool> ToggleMenuItemAvailabilityAsync(int menuItemId, bool isAvailable)
    {
        var item = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
        if (item == null) return Task.FromResult(false);

        item.IsAvailable = isAvailable;
        return Task.FromResult(true);
    }

    public Task<bool> ToggleMenuItemSoldOutAsync(int menuItemId, bool isSoldOut)
    {
        var item = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
        if (item == null) return Task.FromResult(false);

        item.IsSoldOut = isSoldOut;
        return Task.FromResult(true);
    }

    #endregion

    #region 고객 관련

    public Task<List<Customer>> GetCustomersAsync()
    {
        return Task.FromResult(_customers.OrderByDescending(c => c.LastVisitAt).ToList());
    }

    public Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == customerId);
        return Task.FromResult(customer);
    }

    public Task<Customer?> GetCustomerByPhoneAsync(string phone)
    {
        var customer = _customers.FirstOrDefault(c => c.Phone == phone);
        return Task.FromResult(customer);
    }

    public Task<bool> AddPointsAsync(int customerId, decimal points, string description, int? orderId = null)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == customerId);
        if (customer == null) return Task.FromResult(false);

        customer.Points += points;
        _pointHistories.Add(new PointHistory
        {
            Id = _pointHistories.Count + 1,
            CustomerId = customerId,
            Amount = points,
            Description = description,
            CreatedAt = DateTime.Now,
            OrderId = orderId
        });

        return Task.FromResult(true);
    }

    public Task<bool> UsePointsAsync(int customerId, decimal points, string description, int? orderId = null)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == customerId);
        if (customer == null || customer.Points < points) return Task.FromResult(false);

        customer.Points -= points;
        _pointHistories.Add(new PointHistory
        {
            Id = _pointHistories.Count + 1,
            CustomerId = customerId,
            Amount = -points,
            Description = description,
            CreatedAt = DateTime.Now,
            OrderId = orderId
        });

        return Task.FromResult(true);
    }

    public Task<List<PointHistory>> GetPointHistoryAsync(int customerId)
    {
        var history = _pointHistories
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
        return Task.FromResult(history);
    }

    #endregion

    #region 통계 관련

    public Task<DailySummary> GetDailySummaryAsync(DateTime date)
    {
        var dayOrders = _orders.Where(o => o.OrderedAt.Date == date.Date).ToList();
        var completed = dayOrders.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Ready).ToList();
        var cancelled = dayOrders.Where(o => o.Status == OrderStatus.Cancelled).ToList();

        var summary = new DailySummary
        {
            Date = date,
            TotalOrders = dayOrders.Count,
            CompletedOrders = completed.Count,
            CancelledOrders = cancelled.Count,
            TotalSales = completed.Sum(o => o.FinalAmount),
            AverageOrderAmount = completed.Count > 0 ? completed.Average(o => o.FinalAmount) : 0
        };

        return Task.FromResult(summary);
    }

    public Task<List<DailySummary>> GetWeeklySummaryAsync(DateTime startDate)
    {
        var summaries = new List<DailySummary>();
        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            var summary = GetDailySummaryAsync(date).Result;
            summaries.Add(summary);
        }
        return Task.FromResult(summaries);
    }

    public Task<List<PopularMenuItem>> GetPopularMenuItemsAsync(DateTime startDate, DateTime endDate, int limit = 10)
    {
        var orderItems = _orders
            .Where(o => o.OrderedAt >= startDate && o.OrderedAt <= endDate
                       && (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Ready))
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItemId)
            .Select(g => new PopularMenuItem
            {
                MenuItemId = g.Key,
                MenuName = g.First().MenuName,
                OrderCount = g.Sum(i => i.Quantity),
                TotalSales = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.OrderCount)
            .Take(limit)
            .ToList();

        for (int i = 0; i < orderItems.Count; i++)
        {
            orderItems[i].Rank = i + 1;
        }

        return Task.FromResult(orderItems);
    }

    public Task<List<HourlyStats>> GetHourlyStatsAsync(DateTime date)
    {
        var stats = _orders
            .Where(o => o.OrderedAt.Date == date.Date
                       && (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Ready))
            .GroupBy(o => o.OrderedAt.Hour)
            .Select(g => new HourlyStats
            {
                Hour = g.Key,
                OrderCount = g.Count(),
                Sales = g.Sum(o => o.FinalAmount)
            })
            .OrderBy(s => s.Hour)
            .ToList();

        return Task.FromResult(stats);
    }

    #endregion

    #region 데이터 생성

    private List<MenuCategory> GenerateCategories() => new()
    {
        new() { Id = 1, Name = "커피", DisplayOrder = 1 },
        new() { Id = 2, Name = "음료", DisplayOrder = 2 },
        new() { Id = 3, Name = "티", DisplayOrder = 3 },
        new() { Id = 4, Name = "디저트", DisplayOrder = 4 },
        new() { Id = 5, Name = "시즌 한정", DisplayOrder = 5 }
    };

    private List<MenuItem> GenerateMenuItems()
    {
        var sizeOptions = new MenuOptionGroup
        {
            Id = 1, Name = "사이즈", IsRequired = true, AllowMultiple = false,
            Options = new List<MenuOption>
            {
                new() { Id = 1, OptionGroupId = 1, Name = "Regular", AdditionalPrice = 0, IsDefault = true },
                new() { Id = 2, OptionGroupId = 1, Name = "Large", AdditionalPrice = 500 }
            }
        };

        var shotOptions = new MenuOptionGroup
        {
            Id = 2, Name = "샷 추가", IsRequired = false, AllowMultiple = true, MaxSelections = 3,
            Options = new List<MenuOption>
            {
                new() { Id = 3, OptionGroupId = 2, Name = "샷 추가", AdditionalPrice = 500 }
            }
        };

        var tempOptions = new MenuOptionGroup
        {
            Id = 3, Name = "온도", IsRequired = true, AllowMultiple = false,
            Options = new List<MenuOption>
            {
                new() { Id = 4, OptionGroupId = 3, Name = "ICE", AdditionalPrice = 0, IsDefault = true },
                new() { Id = 5, OptionGroupId = 3, Name = "HOT", AdditionalPrice = 0 }
            }
        };

        return new List<MenuItem>
        {
            // 커피
            new() { Id = 1, CategoryId = 1, Name = "아메리카노", Description = "깊고 진한 에스프레소의 맛", Price = 4500, IsPopular = true, DisplayOrder = 1, OptionGroups = new() { sizeOptions, shotOptions, tempOptions } },
            new() { Id = 2, CategoryId = 1, Name = "카페라떼", Description = "부드러운 우유와 에스프레소의 조화", Price = 5000, IsPopular = true, DisplayOrder = 2, OptionGroups = new() { sizeOptions, shotOptions, tempOptions } },
            new() { Id = 3, CategoryId = 1, Name = "바닐라 라떼", Description = "달콤한 바닐라 향이 가미된 라떼", Price = 5500, DisplayOrder = 3, OptionGroups = new() { sizeOptions, shotOptions, tempOptions } },
            new() { Id = 4, CategoryId = 1, Name = "카라멜 마키아토", Description = "카라멜 소스의 달콤함", Price = 5500, DisplayOrder = 4, OptionGroups = new() { sizeOptions, shotOptions, tempOptions } },
            new() { Id = 5, CategoryId = 1, Name = "콜드브루", Description = "12시간 저온 추출 커피", Price = 5000, IsNew = true, DisplayOrder = 5, OptionGroups = new() { sizeOptions } },
            new() { Id = 6, CategoryId = 1, Name = "에스프레소", Description = "진한 에스프레소 한 잔", Price = 3500, DisplayOrder = 6, OptionGroups = new() { shotOptions } },

            // 음료
            new() { Id = 7, CategoryId = 2, Name = "초코 라떼", Description = "진한 초콜릿과 우유의 조화", Price = 5500, DisplayOrder = 1, OptionGroups = new() { sizeOptions, tempOptions } },
            new() { Id = 8, CategoryId = 2, Name = "딸기 스무디", Description = "신선한 딸기로 만든 스무디", Price = 6000, IsPopular = true, DisplayOrder = 2, OptionGroups = new() { sizeOptions } },
            new() { Id = 9, CategoryId = 2, Name = "망고 스무디", Description = "달콤한 망고 스무디", Price = 6000, DisplayOrder = 3, OptionGroups = new() { sizeOptions } },
            new() { Id = 10, CategoryId = 2, Name = "레몬에이드", Description = "상큼한 레몬에이드", Price = 5000, DisplayOrder = 4, OptionGroups = new() { sizeOptions } },

            // 티
            new() { Id = 11, CategoryId = 3, Name = "얼그레이", Description = "향긋한 베르가못 향", Price = 4500, DisplayOrder = 1, OptionGroups = new() { tempOptions } },
            new() { Id = 12, CategoryId = 3, Name = "페퍼민트", Description = "상쾌한 페퍼민트 티", Price = 4500, DisplayOrder = 2, OptionGroups = new() { tempOptions } },
            new() { Id = 13, CategoryId = 3, Name = "캐모마일", Description = "편안함을 주는 캐모마일", Price = 4500, DisplayOrder = 3, OptionGroups = new() { tempOptions } },
            new() { Id = 14, CategoryId = 3, Name = "유자차", Description = "달콤한 유자차", Price = 5000, DisplayOrder = 4, OptionGroups = new() { tempOptions } },

            // 디저트
            new() { Id = 15, CategoryId = 4, Name = "티라미수 케이크", Description = "진한 커피 향의 티라미수", Price = 6500, DisplayOrder = 1 },
            new() { Id = 16, CategoryId = 4, Name = "치즈케이크", Description = "부드러운 뉴욕 치즈케이크", Price = 6000, IsPopular = true, DisplayOrder = 2 },
            new() { Id = 17, CategoryId = 4, Name = "마카롱 세트", Description = "다양한 맛의 마카롱 3개", Price = 5500, DisplayOrder = 3 },
            new() { Id = 18, CategoryId = 4, Name = "크로와상", Description = "버터향 가득한 크로와상", Price = 4000, DisplayOrder = 4 },

            // 시즌 한정
            new() { Id = 19, CategoryId = 5, Name = "크리스마스 라떼", Description = "시나몬과 진저브레드 향", Price = 6500, IsNew = true, DisplayOrder = 1, OptionGroups = new() { sizeOptions, tempOptions } },
            new() { Id = 20, CategoryId = 5, Name = "민트 초코 프라페", Description = "시원한 민트초코의 청량감", Price = 6500, IsNew = true, DisplayOrder = 2, OptionGroups = new() { sizeOptions } }
        };
    }

    private List<Customer> GenerateCustomers() => new()
    {
        new() { Id = 1, Name = "김지노", Phone = "01012345678", Points = 15000, TotalOrders = 45, TotalSpent = 225000, LastVisitAt = DateTime.Now.AddHours(-2), CreatedAt = DateTime.Now.AddMonths(-6) },
        new() { Id = 2, Name = "이커피", Phone = "01023456789", Points = 8500, TotalOrders = 28, TotalSpent = 140000, LastVisitAt = DateTime.Now.AddDays(-1), CreatedAt = DateTime.Now.AddMonths(-4) },
        new() { Id = 3, Name = "박라떼", Phone = "01034567890", Points = 22000, TotalOrders = 62, TotalSpent = 310000, LastVisitAt = DateTime.Now.AddHours(-5), CreatedAt = DateTime.Now.AddMonths(-8) },
        new() { Id = 4, Name = "최모카", Phone = "01045678901", Points = 5000, TotalOrders = 15, TotalSpent = 75000, LastVisitAt = DateTime.Now.AddDays(-3), CreatedAt = DateTime.Now.AddMonths(-2) },
        new() { Id = 5, Name = "정에스", Phone = "01056789012", Points = 12500, TotalOrders = 38, TotalSpent = 190000, LastVisitAt = DateTime.Now.AddHours(-8), CreatedAt = DateTime.Now.AddMonths(-5) }
    };

    private List<Order> GenerateOrders()
    {
        var orders = new List<Order>();
        var now = DateTime.Now;

        // 대기중 주문 2개
        orders.Add(new Order
        {
            Id = _nextOrderId++,
            OrderNumber = "A001",
            CustomerId = 1,
            CustomerName = "김지노",
            CustomerPhone = "01012345678",
            Status = OrderStatus.Pending,
            OrderedAt = now.AddMinutes(-3),
            Items = new List<OrderItem>
            {
                new() { Id = 1, MenuItemId = 1, MenuName = "아메리카노", UnitPrice = 4500, Quantity = 2, SelectedOptions = new() { new() { OptionName = "Large", AdditionalPrice = 500 } } },
                new() { Id = 2, MenuItemId = 16, MenuName = "치즈케이크", UnitPrice = 6000, Quantity = 1 }
            },
            EarnedPoints = 160
        });

        orders.Add(new Order
        {
            Id = _nextOrderId++,
            OrderNumber = "A002",
            CustomerName = "게스트",
            Status = OrderStatus.Pending,
            OrderedAt = now.AddMinutes(-1),
            Items = new List<OrderItem>
            {
                new() { Id = 3, MenuItemId = 8, MenuName = "딸기 스무디", UnitPrice = 6000, Quantity = 1, SelectedOptions = new() { new() { OptionName = "Large", AdditionalPrice = 500 } } }
            },
            EarnedPoints = 65
        });

        // 준비중 주문 1개
        orders.Add(new Order
        {
            Id = _nextOrderId++,
            OrderNumber = "A003",
            CustomerId = 3,
            CustomerName = "박라떼",
            CustomerPhone = "01034567890",
            Status = OrderStatus.Preparing,
            OrderedAt = now.AddMinutes(-8),
            AcceptedAt = now.AddMinutes(-5),
            EstimatedMinutes = 10,
            EstimatedPickupTime = now.AddMinutes(2),
            Items = new List<OrderItem>
            {
                new() { Id = 4, MenuItemId = 2, MenuName = "카페라떼", UnitPrice = 5000, Quantity = 1, SelectedOptions = new() { new() { OptionName = "HOT" } } },
                new() { Id = 5, MenuItemId = 17, MenuName = "마카롱 세트", UnitPrice = 5500, Quantity = 2 }
            },
            EarnedPoints = 160
        });

        // 완료된 주문 (픽업 대기)
        orders.Add(new Order
        {
            Id = _nextOrderId++,
            OrderNumber = "A004",
            CustomerId = 2,
            CustomerName = "이커피",
            CustomerPhone = "01023456789",
            Status = OrderStatus.Ready,
            OrderedAt = now.AddMinutes(-15),
            AcceptedAt = now.AddMinutes(-12),
            CompletedAt = now.AddMinutes(-2),
            Items = new List<OrderItem>
            {
                new() { Id = 6, MenuItemId = 5, MenuName = "콜드브루", UnitPrice = 5000, Quantity = 1 }
            },
            EarnedPoints = 50
        });

        // 과거 완료 주문들
        for (int i = 0; i < 20; i++)
        {
            var orderTime = now.AddDays(-_random.Next(0, 7)).AddHours(-_random.Next(1, 10));
            orders.Add(new Order
            {
                Id = _nextOrderId++,
                OrderNumber = $"P{100 + i:000}",
                CustomerId = _random.Next(1, 6),
                CustomerName = _customers[_random.Next(0, 5)].Name,
                Status = OrderStatus.Completed,
                OrderedAt = orderTime,
                AcceptedAt = orderTime.AddMinutes(2),
                CompletedAt = orderTime.AddMinutes(10),
                Items = new List<OrderItem>
                {
                    new()
                    {
                        Id = 100 + i,
                        MenuItemId = _random.Next(1, 20),
                        MenuName = _menuItems[_random.Next(0, _menuItems.Count)].Name,
                        UnitPrice = _random.Next(4, 7) * 1000,
                        Quantity = _random.Next(1, 3)
                    }
                }
            });
        }

        return orders;
    }

    private Order GenerateRandomOrder()
    {
        var customer = _customers[_random.Next(0, _customers.Count)];
        var menuItem = _menuItems[_random.Next(0, _menuItems.Count)];

        return new Order
        {
            Id = _nextOrderId++,
            OrderNumber = $"N{_nextOrderId - 1000:000}",
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            Status = OrderStatus.Pending,
            OrderedAt = DateTime.Now,
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = _nextOrderId * 10,
                    MenuItemId = menuItem.Id,
                    MenuName = menuItem.Name,
                    UnitPrice = menuItem.Price,
                    Quantity = _random.Next(1, 3)
                }
            },
            EarnedPoints = menuItem.Price * 0.01m
        };
    }

    private List<PointHistory> GeneratePointHistories()
    {
        var histories = new List<PointHistory>();
        var id = 1;

        foreach (var customer in _customers)
        {
            for (int i = 0; i < 5; i++)
            {
                histories.Add(new PointHistory
                {
                    Id = id++,
                    CustomerId = customer.Id,
                    Amount = _random.Next(50, 500),
                    Description = "주문 적립",
                    CreatedAt = DateTime.Now.AddDays(-_random.Next(1, 30))
                });
            }
        }

        return histories.OrderByDescending(h => h.CreatedAt).ToList();
    }

    #endregion

    public void Dispose()
    {
        _newOrderTimer.Dispose();
    }
}
