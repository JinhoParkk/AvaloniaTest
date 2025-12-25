namespace AvaloniaApplication1.Models.JinoOrder;

/// <summary>
/// 메뉴 옵션 그룹 (예: 사이즈, 샷 추가)
/// </summary>
public class MenuOptionGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool AllowMultiple { get; set; }
    public int MaxSelections { get; set; } = 1;
    public List<MenuOption> Options { get; set; } = new();
}

/// <summary>
/// 메뉴 옵션 항목
/// </summary>
public class MenuOption
{
    public int Id { get; set; }
    public int OptionGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; }
    public bool IsDefault { get; set; }

    public string FormattedPrice => AdditionalPrice > 0 ? $"+{AdditionalPrice:N0}원" : "";
}
