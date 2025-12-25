using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JinoOrder.Application.Statistics;
using JinoOrder.Domain.Statistics;
using JinoOrder.Presentation.Common;

namespace JinoOrder.Presentation.Statistics;

public partial class StatisticsViewModel : ViewModelBase
{
    private readonly IStatisticsService _statisticsService;

    [ObservableProperty] private DailySummary? _todaySummary;
    [ObservableProperty] private ObservableCollection<PopularMenuItem> _popularMenuItems = new();
    [ObservableProperty] private bool _isLoading;

    public StatisticsViewModel(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    public override void OnActivated()
    {
        base.OnActivated();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            await LoadTodaySummaryAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadTodaySummaryAsync()
    {
        TodaySummary = await _statisticsService.GetDailySummaryAsync(DateTime.Today);
        var popular = await _statisticsService.GetPopularMenuItemsAsync(
            DateTime.Today.AddDays(-7), DateTime.Today, 5);
        PopularMenuItems = new ObservableCollection<PopularMenuItem>(popular);
    }
}
