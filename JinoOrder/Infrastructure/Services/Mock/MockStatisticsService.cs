using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JinoOrder.Application.Statistics;
using JinoOrder.Domain.Orders;
using JinoOrder.Domain.Statistics;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Infrastructure.Services.Mock;

/// <summary>
/// 통계 Mock 서비스
/// </summary>
public class MockStatisticsService : IStatisticsService
{
    private readonly MockDataStore _dataStore;
    private readonly ILogger<MockStatisticsService> _logger;

    public MockStatisticsService(MockDataStore dataStore, ILogger<MockStatisticsService> logger)
    {
        _dataStore = dataStore;
        _logger = logger;

        _logger.LogDebug("MockStatisticsService 초기화됨");
    }

    public Task<DailySummary> GetDailySummaryAsync(DateTime date)
    {
        _logger.LogDebug("일일 요약 조회: Date={Date}", date.ToShortDateString());

        var dayOrders = _dataStore.Orders.Where(o => o.OrderedAt.Date == date.Date).ToList();
        var completed = dayOrders.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Ready).ToList();
        var cancelled = dayOrders.Where(o => o.Status == OrderStatus.Cancelled).ToList();

        var summary = new DailySummary
        {
            Date = date,
            TotalOrders = dayOrders.Count,
            CompletedOrders = completed.Count,
            CancelledOrders = cancelled.Count,
            TotalSales = completed.Sum(o => o.FinalAmount),
            AverageOrderAmount = completed.Count > 0 ? completed.Average(o => o.FinalAmount) : 0
        };

        _logger.LogDebug("일일 요약 조회 완료: TotalOrders={TotalOrders}, TotalSales={TotalSales}",
            summary.TotalOrders, summary.TotalSales);

        return Task.FromResult(summary);
    }

    public Task<List<DailySummary>> GetWeeklySummaryAsync(DateTime startDate)
    {
        _logger.LogDebug("주간 요약 조회: StartDate={StartDate}", startDate.ToShortDateString());

        var summaries = new List<DailySummary>();
        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            var summary = GetDailySummaryAsync(date).Result;
            summaries.Add(summary);
        }

        _logger.LogDebug("주간 요약 조회 완료: Days={Days}", summaries.Count);
        return Task.FromResult(summaries);
    }

    public Task<List<PopularMenuItem>> GetPopularMenuItemsAsync(DateTime startDate, DateTime endDate, int limit = 10)
    {
        _logger.LogDebug("인기 메뉴 조회: StartDate={StartDate}, EndDate={EndDate}, Limit={Limit}",
            startDate.ToShortDateString(), endDate.ToShortDateString(), limit);

        var orderItems = _dataStore.Orders
            .Where(o => o.OrderedAt >= startDate && o.OrderedAt <= endDate
                       && (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Ready))
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItemId)
            .Select(g => new PopularMenuItem
            {
                MenuItemId = g.Key,
                MenuName = g.First().MenuName,
                OrderCount = g.Sum(i => i.Quantity),
                TotalSales = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.OrderCount)
            .Take(limit)
            .ToList();

        for (int i = 0; i < orderItems.Count; i++)
        {
            orderItems[i].Rank = i + 1;
        }

        _logger.LogDebug("인기 메뉴 {Count}개 조회됨", orderItems.Count);
        return Task.FromResult(orderItems);
    }

    public Task<List<HourlyStats>> GetHourlyStatsAsync(DateTime date)
    {
        _logger.LogDebug("시간대별 통계 조회: Date={Date}", date.ToShortDateString());

        var stats = _dataStore.Orders
            .Where(o => o.OrderedAt.Date == date.Date
                       && (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Ready))
            .GroupBy(o => o.OrderedAt.Hour)
            .Select(g => new HourlyStats
            {
                Hour = g.Key,
                OrderCount = g.Count(),
                Sales = g.Sum(o => o.FinalAmount)
            })
            .OrderBy(s => s.Hour)
            .ToList();

        _logger.LogDebug("시간대별 통계 {Count}개 시간대 조회됨", stats.Count);
        return Task.FromResult(stats);
    }
}
