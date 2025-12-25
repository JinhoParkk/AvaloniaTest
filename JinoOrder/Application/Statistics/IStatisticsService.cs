using JinoOrder.Domain.Statistics;

namespace JinoOrder.Application.Statistics;

/// <summary>
/// 통계 서비스 인터페이스
/// </summary>
public interface IStatisticsService
{
    Task<DailySummary> GetDailySummaryAsync(DateTime date);
    Task<List<DailySummary>> GetWeeklySummaryAsync(DateTime startDate);
    Task<List<PopularMenuItem>> GetPopularMenuItemsAsync(DateTime startDate, DateTime endDate, int limit = 10);
    Task<List<HourlyStats>> GetHourlyStatsAsync(DateTime date);
}
