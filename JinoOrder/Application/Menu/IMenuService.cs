using JinoOrder.Domain.Menu;

namespace JinoOrder.Application.Menu;

/// <summary>
/// 메뉴 서비스 인터페이스
/// </summary>
public interface IMenuService
{
    Task<List<MenuCategory>> GetCategoriesAsync();
    Task<List<MenuItem>> GetMenuItemsAsync();
    Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId);
    Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId);
    Task<bool> UpdateMenuItemAsync(MenuItem item);
    Task<bool> ToggleMenuItemAvailabilityAsync(int menuItemId, bool isAvailable);
    Task<bool> ToggleMenuItemSoldOutAsync(int menuItemId, bool isSoldOut);
}
