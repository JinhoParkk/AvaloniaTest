namespace JinoOrder.Domain.Customers;

/// <summary>
/// 고객
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal Points { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastVisitAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public string FormattedPoints => $"{Points:N0}P";
    public string FormattedTotalSpent => $"{TotalSpent:N0}원";
    public string MaskedPhone => Phone.Length >= 4
        ? $"***-****-{Phone[^4..]}"
        : Phone;
}

/// <summary>
/// 포인트 내역
/// </summary>
public class PointHistory
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? OrderId { get; set; }

    public string FormattedAmount => Amount >= 0 ? $"+{Amount:N0}P" : $"{Amount:N0}P";
    public string AmountColor => Amount >= 0 ? "#107C10" : "#E81123";
}
