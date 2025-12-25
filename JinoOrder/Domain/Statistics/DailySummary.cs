namespace JinoOrder.Domain.Statistics;

/// <summary>
/// 일별 요약
/// </summary>
public class DailySummary
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal AverageOrderAmount { get; set; }

    public string FormattedTotalSales => $"{TotalSales:N0}원";
    public string FormattedAverageOrderAmount => $"{AverageOrderAmount:N0}원";
    public string DateText => Date.ToString("M월 d일 (ddd)");
}

/// <summary>
/// 인기 메뉴 통계
/// </summary>
public class PopularMenuItem
{
    public int MenuItemId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
    public int Rank { get; set; }

    public string FormattedTotalSales => $"{TotalSales:N0}원";
}

/// <summary>
/// 시간대별 주문 통계
/// </summary>
public class HourlyStats
{
    public int Hour { get; set; }
    public int OrderCount { get; set; }
    public decimal Sales { get; set; }

    public string HourText => $"{Hour}시";
    public string FormattedSales => $"{Sales:N0}원";
}
