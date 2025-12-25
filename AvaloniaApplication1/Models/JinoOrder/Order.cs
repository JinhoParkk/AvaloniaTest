namespace AvaloniaApplication1.Models.JinoOrder;

/// <summary>
/// 주문 상태
/// </summary>
public enum OrderStatus
{
    /// <summary>대기중 - 새 주문</summary>
    Pending,
    /// <summary>접수됨 - 주문 확인</summary>
    Accepted,
    /// <summary>준비중 - 제조 중</summary>
    Preparing,
    /// <summary>완료 - 픽업 대기</summary>
    Ready,
    /// <summary>픽업 완료</summary>
    Completed,
    /// <summary>취소됨</summary>
    Cancelled
}

/// <summary>
/// 주문
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int EstimatedMinutes { get; set; } = 10;
    public DateTime? EstimatedPickupTime { get; set; }
    public string? Memo { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    /// <summary>
    /// 총 주문 금액
    /// </summary>
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);

    /// <summary>
    /// 사용된 포인트
    /// </summary>
    public decimal UsedPoints { get; set; }

    /// <summary>
    /// 적립될 포인트
    /// </summary>
    public decimal EarnedPoints { get; set; }

    /// <summary>
    /// 최종 결제 금액
    /// </summary>
    public decimal FinalAmount => TotalAmount - UsedPoints;

    public string FormattedTotalAmount => $"{TotalAmount:N0}원";
    public string FormattedFinalAmount => $"{FinalAmount:N0}원";

    public string StatusText => Status switch
    {
        OrderStatus.Pending => "대기중",
        OrderStatus.Accepted => "접수됨",
        OrderStatus.Preparing => "준비중",
        OrderStatus.Ready => "완료",
        OrderStatus.Completed => "픽업완료",
        OrderStatus.Cancelled => "취소됨",
        _ => "알 수 없음"
    };

    public string StatusColor => Status switch
    {
        OrderStatus.Pending => "#FF8C00",
        OrderStatus.Accepted => "#0078D4",
        OrderStatus.Preparing => "#0078D4",
        OrderStatus.Ready => "#107C10",
        OrderStatus.Completed => "#666666",
        OrderStatus.Cancelled => "#E81123",
        _ => "#666666"
    };

    /// <summary>
    /// 대기 시간 (분)
    /// </summary>
    public int WaitingMinutes => (int)(DateTime.Now - OrderedAt).TotalMinutes;

    public string WaitingTimeText => WaitingMinutes < 1 ? "방금" : $"{WaitingMinutes}분 전";
}
