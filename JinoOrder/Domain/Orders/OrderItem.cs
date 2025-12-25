namespace JinoOrder.Domain.Orders;

/// <summary>
/// 주문 아이템
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 선택된 옵션들
    /// </summary>
    public List<OrderItemOption> SelectedOptions { get; set; } = new();

    /// <summary>
    /// 옵션 추가 금액 합계
    /// </summary>
    public decimal OptionsPrice => SelectedOptions.Sum(o => o.AdditionalPrice);

    /// <summary>
    /// 아이템 총 금액 (단가 + 옵션) * 수량
    /// </summary>
    public decimal TotalPrice => (UnitPrice + OptionsPrice) * Quantity;

    public string FormattedUnitPrice => $"{UnitPrice:N0}원";
    public string FormattedTotalPrice => $"{TotalPrice:N0}원";

    /// <summary>
    /// 옵션 요약 텍스트
    /// </summary>
    public string OptionsSummary => SelectedOptions.Count > 0
        ? string.Join(", ", SelectedOptions.Select(o => o.OptionName))
        : "";
}

/// <summary>
/// 주문 아이템에 선택된 옵션
/// </summary>
public class OrderItemOption
{
    public int OptionId { get; set; }
    public string OptionGroupName { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; }
}
