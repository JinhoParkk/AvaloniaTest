using JinoOrder.Presentation.Common;
using JinoOrder.Application.Common;
using JinoOrder.Domain.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Presentation.Orders;
using JinoOrder.Presentation.Menu;
using JinoOrder.Presentation.Customers;
using JinoOrder.Presentation.Statistics;
using JinoOrder.Presentation.Settings;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Presentation.Shell;

public partial class JinoOrderMainViewModel : ViewModelBase
{
    private readonly IPlatformInfo? _platformInfo;

    // 자식 ViewModels
    public OrdersViewModel Orders { get; }
    public MenuManagementViewModel Menu { get; }
    public CustomersViewModel Customers { get; }
    public StatisticsViewModel Statistics { get; }
    public StoreStateViewModel StoreState { get; }
    public SettingsViewModel Settings { get; }

    // 네비게이션 상태
    [ObservableProperty] private string _selectedMenu = Routes.Orders;
    [ObservableProperty] private bool _isSidebarCollapsed;
    [ObservableProperty] private bool _isMoreMenuOpen;

    // 플랫폼 정보
    public bool IsMobile => _platformInfo?.IsMobile ?? false;
    public bool IsDesktop => !IsMobile;
    public bool ShowSidebar => !IsMobile;
    public bool ShowBottomNav => IsMobile;

    // 메뉴 선택 상태
    public bool IsOrdersSelected => SelectedMenu == Routes.Orders;
    public bool IsHistorySelected => SelectedMenu == Routes.History;
    public bool IsMenuSelected => SelectedMenu == Routes.Menu;
    public bool IsCustomersSelected => SelectedMenu == Routes.Customers;
    public bool IsStatsSelected => SelectedMenu == Routes.Statistics;
    public bool IsSettingsSelected => SelectedMenu == Routes.Settings;

    // 모바일 더보기 메뉴 선택 상태
    public bool IsMoreSelected => IsCustomersSelected || IsStatsSelected || IsSettingsSelected;

    // Orders에서 pending count 가져오기 (바인딩 호환성)
    public int PendingOrderCount => Orders.PendingOrderCount;
    public bool HasPendingOrders => Orders.HasPendingOrders;

    // StoreState에서 가져오기 (바인딩 호환성)
    public string StoreName => StoreState.StoreName;
    public bool IsOpen => StoreState.IsOpen;
    public bool IsPaused => StoreState.IsPaused;
    public string StatusText => StoreState.StatusText;
    public string StatusColor => StoreState.StatusColor;
    public string PickupTimeText => StoreState.PickupTimeText;
    public int MinPickupTime => StoreState.MinPickupTime;

    public JinoOrderMainViewModel(
        OrdersViewModel orders,
        MenuManagementViewModel menu,
        CustomersViewModel customers,
        StatisticsViewModel statistics,
        StoreStateViewModel storeState,
        SettingsViewModel settings,
        IPlatformInfo? platformInfo,
        ILogger<JinoOrderMainViewModel> logger)
    {
        Orders = orders;
        Menu = menu;
        Customers = customers;
        Statistics = statistics;
        StoreState = storeState;
        Settings = settings;
        _platformInfo = platformInfo;
        Logger = logger;

        Logger.LogDebug("JinoOrderMainViewModel 초기화됨");

        // Orders의 PickupTime을 StoreState에서 가져오도록 연결
        Orders.SetPickupTimeCallback(() => StoreState.MinPickupTime);

        // 설정 저장 시 StoreState 업데이트
        Settings.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.ShowSaveMessage) && Settings.ShowSaveMessage)
            {
                Logger.LogDebug("설정 저장 감지 - StoreState 업데이트");
                StoreState.OnActivated();
                NotifyStoreStateChanged();
            }
        };

        // Orders의 PendingOrderCount 변경 시 알림
        Orders.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(OrdersViewModel.PendingOrderCount))
            {
                OnPropertyChanged(nameof(PendingOrderCount));
                OnPropertyChanged(nameof(HasPendingOrders));
            }
        };

        // StoreState 변경 시 알림
        StoreState.PropertyChanged += (s, e) =>
        {
            NotifyStoreStateChanged();
        };
    }

    private void NotifyStoreStateChanged()
    {
        OnPropertyChanged(nameof(StoreName));
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(IsPaused));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(PickupTimeText));
        OnPropertyChanged(nameof(MinPickupTime));
    }

    public override void OnActivated()
    {
        base.OnActivated();

        Logger.LogDebug("JinoOrderMainViewModel 활성화됨");

        // 모든 자식 ViewModel 활성화
        Orders.OnActivated();
        Menu.OnActivated();
        Customers.OnActivated();
        Statistics.OnActivated();
        StoreState.OnActivated();
    }

    public override void OnDeactivated()
    {
        Logger.LogDebug("JinoOrderMainViewModel 비활성화됨");

        // 모든 자식 ViewModel 비활성화
        Orders.OnDeactivated();
        Menu.OnDeactivated();
        Customers.OnDeactivated();
        Statistics.OnDeactivated();
        StoreState.OnDeactivated();

        base.OnDeactivated();
    }

    #region 네비게이션 커맨드

    [RelayCommand]
    private void SelectMenu(string menu)
    {
        if (!Routes.IsValidRoute(menu))
        {
            Logger.LogWarning("잘못된 메뉴 라우트: {Menu}", menu);
            return;
        }

        Logger.LogDebug("메뉴 선택: {Menu}", menu);
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

    #region 호환성을 위한 래퍼 커맨드 (XAML 바인딩 호환)

    // StoreState 커맨드 래핑
    [RelayCommand]
    private void ToggleOpen() => StoreState.ToggleOpenCommand.Execute(null);

    [RelayCommand]
    private void SetPause(int minutes) => StoreState.SetPauseCommand.Execute(minutes);

    #endregion
}
