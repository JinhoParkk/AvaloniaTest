using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Application.Menu;
using JinoOrder.Domain.Menu;
using JinoOrder.Presentation.Common;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Presentation.Menu;

public partial class MenuManagementViewModel : ViewModelBase
{
    private readonly IMenuService _menuService;

    [ObservableProperty] private ObservableCollection<MenuCategory> _categories = new();
    [ObservableProperty] private ObservableCollection<MenuItem> _menuItems = new();
    [ObservableProperty] private MenuCategory? _selectedCategory;
    [ObservableProperty] private MenuItem? _selectedMenuItem;

    public MenuManagementViewModel(IMenuService menuService, ILogger<MenuManagementViewModel> logger)
    {
        _menuService = menuService;
        Logger = logger;

        Logger.LogDebug("MenuManagementViewModel 초기화됨");
    }

    public override void OnActivated()
    {
        base.OnActivated();
        Logger.LogDebug("MenuManagementViewModel 활성화됨");
        ExecuteAndForget(async _ => await LoadDataAsync(), "메뉴 데이터 로드");
    }

    private async Task LoadDataAsync()
    {
        Logger.LogDebug("메뉴 데이터 로드 시작");

        await ExecuteAsync(async _ =>
        {
            await Task.WhenAll(
                LoadCategoriesAsync(),
                LoadMenuItemsAsync()
            );
            Logger.LogDebug("메뉴 데이터 로드 완료");
        }, "메뉴 데이터 로드");
    }

    public async Task LoadCategoriesAsync()
    {
        var categories = await _menuService.GetCategoriesAsync();
        Categories = new ObservableCollection<MenuCategory>(categories);
        if (categories.Count > 0)
            SelectedCategory = categories[0];

        Logger.LogDebug("카테고리 {Count}개 로드됨", categories.Count);
    }

    public async Task LoadMenuItemsAsync()
    {
        var items = await _menuService.GetMenuItemsAsync();
        MenuItems = new ObservableCollection<MenuItem>(items);

        Logger.LogDebug("메뉴 아이템 {Count}개 로드됨", items.Count);
    }

    [RelayCommand]
    private async Task SelectCategory(MenuCategory? category)
    {
        if (category == null) return;

        SelectedCategory = category;
        Logger.LogDebug("카테고리 선택됨: CategoryId={CategoryId}, Name={Name}", category.Id, category.Name);

        await ExecuteAsync(async _ =>
        {
            var items = await _menuService.GetMenuItemsByCategoryAsync(category.Id);
            MenuItems = new ObservableCollection<MenuItem>(items);
            Logger.LogDebug("카테고리별 메뉴 {Count}개 로드됨", items.Count);
        }, "카테고리별 메뉴 로드", showLoading: false);
    }

    [RelayCommand]
    private async Task ToggleSoldOut(MenuItem? item)
    {
        if (item == null) return;

        var newSoldOut = !item.IsSoldOut;
        Logger.LogInformation("메뉴 품절 상태 변경 시도: MenuItemId={MenuItemId}, IsSoldOut={IsSoldOut}",
            item.Id, newSoldOut);

        await ExecuteAsync(async _ =>
        {
            var success = await _menuService.ToggleMenuItemSoldOutAsync(item.Id, newSoldOut);
            if (success)
            {
                item.IsSoldOut = newSoldOut;
                Logger.LogInformation("메뉴 품절 상태 변경 완료: MenuItemId={MenuItemId}", item.Id);
                await LoadMenuItemsAsync();
            }
            else
            {
                Logger.LogWarning("메뉴 품절 상태 변경 실패: MenuItemId={MenuItemId}", item.Id);
                SetError("품절 상태를 변경하지 못했습니다.");
            }
        }, "품절 상태 변경", showLoading: false);
    }

    [RelayCommand]
    private async Task ToggleAvailable(MenuItem? item)
    {
        if (item == null) return;

        var newAvailable = !item.IsAvailable;
        Logger.LogInformation("메뉴 판매 상태 변경 시도: MenuItemId={MenuItemId}, IsAvailable={IsAvailable}",
            item.Id, newAvailable);

        await ExecuteAsync(async _ =>
        {
            var success = await _menuService.ToggleMenuItemAvailabilityAsync(item.Id, newAvailable);
            if (success)
            {
                item.IsAvailable = newAvailable;
                Logger.LogInformation("메뉴 판매 상태 변경 완료: MenuItemId={MenuItemId}", item.Id);
                await LoadMenuItemsAsync();
            }
            else
            {
                Logger.LogWarning("메뉴 판매 상태 변경 실패: MenuItemId={MenuItemId}", item.Id);
                SetError("판매 상태를 변경하지 못했습니다.");
            }
        }, "판매 상태 변경", showLoading: false);
    }
}
