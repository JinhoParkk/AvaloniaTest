namespace JinoOrder.Domain.Menu;

/// <summary>
/// 메뉴 아이템
/// </summary>
public class MenuItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsSoldOut { get; set; }
    public bool IsPopular { get; set; }
    public bool IsNew { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 옵션 그룹들 (샷 추가, 사이즈 등)
    /// </summary>
    public List<MenuOptionGroup> OptionGroups { get; set; } = new();

    public string FormattedPrice => $"{Price:N0}원";
}
