using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JinoOrder.Application.Menu;
using JinoOrder.Domain.Menu;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Infrastructure.Services.Mock;

/// <summary>
/// 메뉴 Mock 서비스
/// </summary>
public class MockMenuService : IMenuService
{
    private readonly MockDataStore _dataStore;
    private readonly ILogger<MockMenuService> _logger;

    public MockMenuService(MockDataStore dataStore, ILogger<MockMenuService> logger)
    {
        _dataStore = dataStore;
        _logger = logger;

        _logger.LogDebug("MockMenuService 초기화됨");
    }

    public Task<List<MenuCategory>> GetCategoriesAsync()
    {
        _logger.LogDebug("카테고리 목록 조회");
        var categories = _dataStore.Categories.ToList();
        _logger.LogDebug("카테고리 {Count}개 조회됨", categories.Count);
        return Task.FromResult(categories);
    }

    public Task<List<MenuItem>> GetMenuItemsAsync()
    {
        _logger.LogDebug("전체 메뉴 아이템 조회");
        var items = _dataStore.MenuItems.ToList();
        _logger.LogDebug("메뉴 아이템 {Count}개 조회됨", items.Count);
        return Task.FromResult(items);
    }

    public Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        _logger.LogDebug("카테고리별 메뉴 조회: CategoryId={CategoryId}", categoryId);
        var items = _dataStore.MenuItems.Where(m => m.CategoryId == categoryId).ToList();
        _logger.LogDebug("카테고리 {CategoryId}의 메뉴 {Count}개 조회됨", categoryId, items.Count);
        return Task.FromResult(items);
    }

    public Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId)
    {
        _logger.LogDebug("메뉴 아이템 상세 조회: MenuItemId={MenuItemId}", menuItemId);
        var item = _dataStore.MenuItems.FirstOrDefault(m => m.Id == menuItemId);

        if (item == null)
        {
            _logger.LogWarning("메뉴 아이템을 찾을 수 없음: MenuItemId={MenuItemId}", menuItemId);
        }

        return Task.FromResult(item);
    }

    public Task<bool> UpdateMenuItemAsync(MenuItem item)
    {
        _logger.LogInformation("메뉴 아이템 업데이트: MenuItemId={MenuItemId}, Name={Name}", item.Id, item.Name);

        var existing = _dataStore.MenuItems.FirstOrDefault(m => m.Id == item.Id);
        if (existing == null)
        {
            _logger.LogWarning("메뉴 업데이트 실패 - 메뉴를 찾을 수 없음: MenuItemId={MenuItemId}", item.Id);
            return Task.FromResult(false);
        }

        var index = _dataStore.MenuItems.IndexOf(existing);
        _dataStore.MenuItems[index] = item;

        _logger.LogInformation("메뉴 아이템 업데이트 완료: MenuItemId={MenuItemId}", item.Id);
        return Task.FromResult(true);
    }

    public Task<bool> ToggleMenuItemAvailabilityAsync(int menuItemId, bool isAvailable)
    {
        _logger.LogInformation("메뉴 판매 상태 변경: MenuItemId={MenuItemId}, IsAvailable={IsAvailable}", menuItemId, isAvailable);

        var item = _dataStore.MenuItems.FirstOrDefault(m => m.Id == menuItemId);
        if (item == null)
        {
            _logger.LogWarning("메뉴 판매 상태 변경 실패 - 메뉴를 찾을 수 없음: MenuItemId={MenuItemId}", menuItemId);
            return Task.FromResult(false);
        }

        item.IsAvailable = isAvailable;
        _logger.LogInformation("메뉴 판매 상태 변경 완료: MenuItemId={MenuItemId}, IsAvailable={IsAvailable}", menuItemId, isAvailable);
        return Task.FromResult(true);
    }

    public Task<bool> ToggleMenuItemSoldOutAsync(int menuItemId, bool isSoldOut)
    {
        _logger.LogInformation("메뉴 품절 상태 변경: MenuItemId={MenuItemId}, IsSoldOut={IsSoldOut}", menuItemId, isSoldOut);

        var item = _dataStore.MenuItems.FirstOrDefault(m => m.Id == menuItemId);
        if (item == null)
        {
            _logger.LogWarning("메뉴 품절 상태 변경 실패 - 메뉴를 찾을 수 없음: MenuItemId={MenuItemId}", menuItemId);
            return Task.FromResult(false);
        }

        item.IsSoldOut = isSoldOut;
        _logger.LogInformation("메뉴 품절 상태 변경 완료: MenuItemId={MenuItemId}, IsSoldOut={IsSoldOut}", menuItemId, isSoldOut);
        return Task.FromResult(true);
    }
}
