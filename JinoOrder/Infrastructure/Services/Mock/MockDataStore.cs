using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using JinoOrder.Domain.Customers;
using JinoOrder.Domain.Menu;
using JinoOrder.Domain.Orders;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace JinoOrder.Infrastructure.Services.Mock;

/// <summary>
/// Mock 데이터 중앙 저장소 (서비스 간 공유)
/// </summary>
public class MockDataStore : IDisposable
{
    private readonly ILogger<MockDataStore> _logger;
    private readonly Random _random = new();
    private readonly Timer _newOrderTimer;
    private int _nextOrderId = 1000;

    public List<MenuCategory> Categories { get; }
    public List<MenuItem> MenuItems { get; }
    public List<Order> Orders { get; }
    public List<Customer> Customers { get; }
    public List<PointHistory> PointHistories { get; }

    public event EventHandler<Order>? NewOrderReceived;
    public event EventHandler<Order>? OrderStatusChanged;

    public MockDataStore(ILogger<MockDataStore> logger)
    {
        _logger = logger;

        _logger.LogInformation("MockDataStore 초기화 중...");

        Categories = GenerateCategories();
        MenuItems = GenerateMenuItems();
        Customers = GenerateCustomers();
        Orders = GenerateOrders();
        PointHistories = GeneratePointHistories();

        // 데모용: 30초마다 새 주문 생성
        _newOrderTimer = new Timer(Domain.Common.TimingConstants.NewOrderTimerIntervalMs);
        _newOrderTimer.Elapsed += OnNewOrderTimerElapsed;

        _logger.LogInformation("MockDataStore 초기화 완료. 카테고리: {CategoryCount}, 메뉴: {MenuCount}, 고객: {CustomerCount}, 주문: {OrderCount}",
            Categories.Count, MenuItems.Count, Customers.Count, Orders.Count);
    }

    public void StartSimulation()
    {
        _logger.LogInformation("주문 시뮬레이션 시작");
        _newOrderTimer.Start();
    }

    public void StopSimulation()
    {
        _logger.LogInformation("주문 시뮬레이션 중지");
        _newOrderTimer.Stop();
    }

    public void RaiseOrderStatusChanged(Order order)
    {
        _logger.LogDebug("주문 상태 변경 이벤트 발생: OrderId={OrderId}, Status={Status}", order.Id, order.Status);
        OrderStatusChanged?.Invoke(this, order);
    }

    public int GetNextOrderId() => _nextOrderId++;

    private void OnNewOrderTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var newOrder = GenerateRandomOrder();
        Orders.Insert(0, newOrder);
        _logger.LogInformation("새 주문 생성됨: OrderId={OrderId}, Customer={CustomerName}", newOrder.Id, newOrder.CustomerName);
        NewOrderReceived?.Invoke(this, newOrder);
    }

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
                CustomerName = Customers[_random.Next(0, 5)].Name,
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
                        MenuName = MenuItems[_random.Next(0, MenuItems.Count)].Name,
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
        var customer = Customers[_random.Next(0, Customers.Count)];
        var menuItem = MenuItems[_random.Next(0, MenuItems.Count)];

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

        foreach (var customer in Customers)
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
        _logger.LogInformation("MockDataStore 종료");
        _newOrderTimer.Dispose();
    }
}
