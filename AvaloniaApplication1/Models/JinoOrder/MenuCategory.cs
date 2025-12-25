namespace AvaloniaApplication1.Models.JinoOrder;

/// <summary>
/// 메뉴 카테고리
/// </summary>
public class MenuCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
