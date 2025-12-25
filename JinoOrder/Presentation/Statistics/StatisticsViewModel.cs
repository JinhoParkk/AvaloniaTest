using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JinoOrder.Application.Statistics;
using JinoOrder.Domain.Common;
using JinoOrder.Domain.Statistics;
using JinoOrder.Presentation.Common;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Presentation.Statistics;

public partial class StatisticsViewModel : ViewModelBase
{
    private readonly IStatisticsService _statisticsService;

    [ObservableProperty] private DailySummary? _todaySummary;
    [ObservableProperty] private ObservableCollection<PopularMenuItem> _popularMenuItems = new();

    public StatisticsViewModel(IStatisticsService statisticsService, ILogger<StatisticsViewModel> logger)
    {
        _statisticsService = statisticsService;
        Logger = logger;

        Logger.LogDebug("StatisticsViewModel 초기화됨");
    }

    public override void OnActivated()
    {
        base.OnActivated();
        Logger.LogDebug("StatisticsViewModel 활성화됨");
        ExecuteAndForget(async _ => await LoadDataAsync(), "통계 데이터 로드");
    }

    private async Task LoadDataAsync()
    {
        Logger.LogDebug("통계 데이터 로드 시작");

        await ExecuteAsync(async _ =>
        {
            await LoadTodaySummaryAsync();
            Logger.LogDebug("통계 데이터 로드 완료");
        }, "통계 데이터 로드");
    }

    public async Task LoadTodaySummaryAsync()
    {
        TodaySummary = await _statisticsService.GetDailySummaryAsync(DateTime.Today);
        Logger.LogDebug("일일 요약 로드됨: TotalOrders={TotalOrders}, TotalSales={TotalSales}",
            TodaySummary?.TotalOrders, TodaySummary?.TotalSales);

        var popular = await _statisticsService.GetPopularMenuItemsAsync(
            DateTime.Today.AddDays(-TimingConstants.PopularItemsDaysBack),
            DateTime.Today,
            TimingConstants.PopularItemsCount);
        PopularMenuItems = new ObservableCollection<PopularMenuItem>(popular);
        Logger.LogDebug("인기 메뉴 {Count}개 로드됨", popular.Count);
    }
}
