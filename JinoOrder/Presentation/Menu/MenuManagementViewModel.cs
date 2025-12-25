using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Application.Menu;
using JinoOrder.Domain.Menu;
using JinoOrder.Presentation.Common;

namespace JinoOrder.Presentation.Menu;

public partial class MenuManagementViewModel : ViewModelBase
{
    private readonly IMenuService _menuService;

    [ObservableProperty] private ObservableCollection<MenuCategory> _categories = new();
    [ObservableProperty] private ObservableCollection<MenuItem> _menuItems = new();
    [ObservableProperty] private MenuCategory? _selectedCategory;
    [ObservableProperty] private MenuItem? _selectedMenuItem;
    [ObservableProperty] private bool _isLoading;

    public MenuManagementViewModel(IMenuService menuService)
    {
        _menuService = menuService;
    }

    public override void OnActivated()
    {
        base.OnActivated();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            await Task.WhenAll(
                LoadCategoriesAsync(),
                LoadMenuItemsAsync()
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadCategoriesAsync()
    {
        var categories = await _menuService.GetCategoriesAsync();
        Categories = new ObservableCollection<MenuCategory>(categories);
        if (categories.Count > 0)
            SelectedCategory = categories[0];
    }

    public async Task LoadMenuItemsAsync()
    {
        var items = await _menuService.GetMenuItemsAsync();
        MenuItems = new ObservableCollection<MenuItem>(items);
    }

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
}
