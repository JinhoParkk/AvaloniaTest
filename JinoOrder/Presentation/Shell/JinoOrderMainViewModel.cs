using JinoOrder.Presentation.Common;
using JinoOrder.Infrastructure.Storage;
using JinoOrder.Application.Auth;
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Domain.Orders;
using JinoOrder.Domain.Menu;
using JinoOrder.Domain.Customers;
using JinoOrder.Domain.Statistics;
using JinoOrder.Application.Common;
using JinoOrder.Infrastructure.Services;
using JinoOrder.Application.Orders;
using JinoOrder.Application.Menu;
using JinoOrder.Application.Customers;
using JinoOrder.Application.Statistics;

namespace JinoOrder.Presentation.Shell;

public partial class JinoOrderMainViewModel : ViewModelBase
{
    private readonly IOrderService _orderService;
    private readonly IMenuService _menuService;
    private readonly ICustomerService _customerService;
    private readonly IStatisticsService _statisticsService;
    private readonly IPlatformInfo? _platformInfo;

    // 매장 정보
    [ObservableProperty] private string _storeName = "지노커피 강남점";
    [ObservableProperty] private bool _isOpen = true;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private DateTime? _pausedUntil;
    [ObservableProperty] private int _minPickupTime = 10;
    [ObservableProperty] private int? _maxPickupTime = 20;

    // 네비게이션
    [ObservableProperty] private string _selectedMenu = "orders";
    [ObservableProperty] private bool _isSidebarCollapsed;
    [ObservableProperty] private bool _isMoreMenuOpen;

    // 주문 데이터
    [ObservableProperty] private ObservableCollection<Order> _pendingOrders = new();
    [ObservableProperty] private ObservableCollection<Order> _activeOrders = new();
    [ObservableProperty] private Order? _selectedOrder;
    [ObservableProperty] private int _pendingOrderCount;

    // 메뉴 데이터
    [ObservableProperty] private ObservableCollection<MenuCategory> _categories = new();
    [ObservableProperty] private ObservableCollection<MenuItem> _menuItems = new();
    [ObservableProperty] private MenuCategory? _selectedCategory;
    [ObservableProperty] private MenuItem? _selectedMenuItem;

    // 고객 데이터
    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private Customer? _selectedCustomer;

    // 통계 데이터
    [ObservableProperty] private DailySummary? _todaySummary;
    [ObservableProperty] private ObservableCollection<PopularMenuItem> _popularMenuItems = new();

    // UI 상태
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    // 플랫폼 정보
    public bool IsMobile => _platformInfo?.IsMobile ?? false;
    public bool IsDesktop => !IsMobile;
    public bool ShowSidebar => !IsMobile;
    public bool ShowBottomNav => IsMobile;

    // 메뉴 선택 상태
    public bool IsOrdersSelected => SelectedMenu == "orders";
    public bool IsHistorySelected => SelectedMenu == "history";
    public bool IsMenuSelected => SelectedMenu == "menu";
    public bool IsCustomersSelected => SelectedMenu == "customers";
    public bool IsStatsSelected => SelectedMenu == "stats";
    public bool IsSettingsSelected => SelectedMenu == "settings";
    public bool HasPendingOrders => PendingOrderCount > 0;

    // 모바일 더보기 메뉴 선택 상태 (고객, 통계, 설정 중 하나라도 선택되면 true)
    public bool IsMoreSelected => IsCustomersSelected || IsStatsSelected || IsSettingsSelected;

    // 상태 텍스트
    public string StatusText => GetStatusText();
    public string StatusColor => GetStatusColor();
    public string PickupTimeText => MaxPickupTime.HasValue
        ? $"{MinPickupTime}~{MaxPickupTime}분"
        : $"{MinPickupTime}분";

    public JinoOrderMainViewModel()
    {
        var mockService = new MockJinoOrderService();
        _orderService = mockService;
        _menuService = mockService;
        _customerService = mockService;
        _statisticsService = mockService;
        _platformInfo = null;

        // 이벤트 구독
        _orderService.NewOrderReceived += OnNewOrderReceived;
        _orderService.OrderStatusChanged += OnOrderStatusChanged;

        // 초기 데이터 로드
        _ = LoadInitialDataAsync();
    }

    public JinoOrderMainViewModel(
        IOrderService orderService,
        IMenuService menuService,
        ICustomerService customerService,
        IStatisticsService statisticsService,
        IPlatformInfo? platformInfo)
    {
        _orderService = orderService;
        _menuService = menuService;
        _customerService = customerService;
        _statisticsService = statisticsService;
        _platformInfo = platformInfo;

        // 이벤트 구독
        _orderService.NewOrderReceived += OnNewOrderReceived;
        _orderService.OrderStatusChanged += OnOrderStatusChanged;

        // 초기 데이터 로드
        _ = LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        IsLoading = true;
        try
        {
            await Task.WhenAll(
                LoadOrdersAsync(),
                LoadCategoriesAsync(),
                LoadMenuItemsAsync(),
                LoadCustomersAsync(),
                LoadTodaySummaryAsync()
            );
        }
        catch (Exception ex)
        {
            ErrorMessage = $"데이터 로드 실패: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadOrdersAsync()
    {
        var pending = await _orderService.GetPendingOrdersAsync();
        var active = await _orderService.GetActiveOrdersAsync();

        PendingOrders = new ObservableCollection<Order>(pending);
        ActiveOrders = new ObservableCollection<Order>(active);
        PendingOrderCount = pending.Count;
        OnPropertyChanged(nameof(HasPendingOrders));
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _menuService.GetCategoriesAsync();
        Categories = new ObservableCollection<MenuCategory>(categories);
        if (categories.Count > 0)
            SelectedCategory = categories[0];
    }

    private async Task LoadMenuItemsAsync()
    {
        var items = await _menuService.GetMenuItemsAsync();
        MenuItems = new ObservableCollection<MenuItem>(items);
    }

    private async Task LoadCustomersAsync()
    {
        var customers = await _customerService.GetCustomersAsync();
        Customers = new ObservableCollection<Customer>(customers);
    }

    private async Task LoadTodaySummaryAsync()
    {
        TodaySummary = await _statisticsService.GetDailySummaryAsync(DateTime.Today);
        var popular = await _statisticsService.GetPopularMenuItemsAsync(
            DateTime.Today.AddDays(-7), DateTime.Today, 5);
        PopularMenuItems = new ObservableCollection<PopularMenuItem>(popular);
    }

    private void OnNewOrderReceived(object? sender, Order order)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            PendingOrders.Insert(0, order);
            ActiveOrders.Insert(0, order);
            PendingOrderCount = PendingOrders.Count;
            OnPropertyChanged(nameof(HasPendingOrders));
            // TODO: 알림 표시, 사운드 재생
        });
    }

    private void OnOrderStatusChanged(object? sender, Order order)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            // 주문 리스트 업데이트
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

    #region 네비게이션 커맨드

    [RelayCommand]
    private void SelectMenu(string menu)
    {
        SelectedMenu = menu;
        NotifyMenuSelectionChanged();
    }

    private void NotifyMenuSelectionChanged()
    {
        OnPropertyChanged(nameof(IsOrdersSelected));
        OnPropertyChanged(nameof(IsHistorySelected));
        OnPropertyChanged(nameof(IsMenuSelected));
        OnPropertyChanged(nameof(IsCustomersSelected));
        OnPropertyChanged(nameof(IsStatsSelected));
        OnPropertyChanged(nameof(IsSettingsSelected));
        OnPropertyChanged(nameof(IsMoreSelected));
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
    }

    [RelayCommand]
    private void ToggleMoreMenu()
    {
        IsMoreMenuOpen = !IsMoreMenuOpen;
    }

    [RelayCommand]
    private void CloseMoreMenu()
    {
        IsMoreMenuOpen = false;
    }

    [RelayCommand]
    private void SelectMoreMenuItem(string menu)
    {
        IsMoreMenuOpen = false;
        SelectMenu(menu);
    }

    #endregion

    #region 주문 관련 커맨드

    [RelayCommand]
    private async Task AcceptOrder(Order order)
    {
        if (order == null) return;

        var success = await _orderService.AcceptOrderAsync(order.Id, MinPickupTime);
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
            await LoadTodaySummaryAsync();
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
            await LoadTodaySummaryAsync();
        }
    }

    [RelayCommand]
    private void SelectOrder(Order order)
    {
        SelectedOrder = order;
    }

    #endregion

    #region 메뉴 관련 커맨드

    [RelayCommand]
    private async Task SelectCategory(MenuCategory category)
    {
        SelectedCategory = category;
        if (category != null)
        {
            var items = await _menuService.GetMenuItemsByCategoryAsync(category.Id);
            MenuItems = new ObservableCollection<MenuItem>(items);
        }
    }

    [RelayCommand]
    private async Task ToggleSoldOut(MenuItem item)
    {
        if (item == null) return;

        var success = await _menuService.ToggleMenuItemSoldOutAsync(item.Id, !item.IsSoldOut);
        if (success)
        {
            item.IsSoldOut = !item.IsSoldOut;
            await LoadMenuItemsAsync();
        }
    }

    [RelayCommand]
    private async Task ToggleAvailable(MenuItem item)
    {
        if (item == null) return;

        var success = await _menuService.ToggleMenuItemAvailabilityAsync(item.Id, !item.IsAvailable);
        if (success)
        {
            item.IsAvailable = !item.IsAvailable;
            await LoadMenuItemsAsync();
        }
    }

    #endregion

    #region 매장 상태 관련

    private string GetStatusText()
    {
        if (IsOpen) return "영업중";
        if (IsPaused && PausedUntil.HasValue)
            return $"일시정지 ({PausedUntil:HH:mm}까지)";
        if (IsPaused) return "일시정지";
        return "영업종료";
    }

    private string GetStatusColor()
    {
        if (IsOpen) return "#4CAF50";
        if (IsPaused) return "#FF9800";
        return "#F44336";
    }

    [RelayCommand]
    private void ToggleOpen()
    {
        if (IsOpen)
        {
            IsOpen = false;
            IsPaused = false;
            PausedUntil = null;
        }
        else
        {
            IsOpen = true;
            IsPaused = false;
            PausedUntil = null;
        }
        NotifyStatusChanged();
    }

    [RelayCommand]
    private void SetPause(int minutes)
    {
        IsOpen = false;
        IsPaused = true;
        PausedUntil = DateTime.Now.AddMinutes(minutes);
        NotifyStatusChanged();
    }

    private void NotifyStatusChanged()
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));
    }

    #endregion
}
