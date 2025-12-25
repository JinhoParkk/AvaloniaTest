using JinoOrder.Presentation.Common;
using JinoOrder.Application.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Presentation.Orders;
using JinoOrder.Presentation.Menu;
using JinoOrder.Presentation.Customers;
using JinoOrder.Presentation.Statistics;
using JinoOrder.Presentation.Settings;

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
    [ObservableProperty] private string _selectedMenu = "orders";
    [ObservableProperty] private bool _isSidebarCollapsed;
    [ObservableProperty] private bool _isMoreMenuOpen;

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
        IPlatformInfo? platformInfo = null)
    {
        Orders = orders;
        Menu = menu;
        Customers = customers;
        Statistics = statistics;
        StoreState = storeState;
        Settings = settings;
        _platformInfo = platformInfo;

        // Orders의 PickupTime을 StoreState에서 가져오도록 연결
        // OrdersViewModel이 StoreState.MinPickupTime을 사용하도록 설정

        // 설정 저장 시 StoreState 업데이트
        Settings.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.ShowSaveMessage) && Settings.ShowSaveMessage)
            {
                // 설정이 저장될 때 StoreState 리로드
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

        // 모든 자식 ViewModel 활성화
        Orders.OnActivated();
        Menu.OnActivated();
        Customers.OnActivated();
        Statistics.OnActivated();
        StoreState.OnActivated();
    }

    public override void OnDeactivated()
    {
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
