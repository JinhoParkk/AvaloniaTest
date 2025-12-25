using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Domain.Settings;
using JinoOrder.Infrastructure.Storage;
using JinoOrder.Presentation.Common;

namespace JinoOrder.Presentation.Shell;

public partial class StoreStateViewModel : ViewModelBase
{
    private readonly PreferencesService _preferencesService;

    [ObservableProperty] private string _storeName = "지노커피 강남점";
    [ObservableProperty] private bool _isOpen = true;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private DateTime? _pausedUntil;
    [ObservableProperty] private int _minPickupTime = 10;
    [ObservableProperty] private int? _maxPickupTime = 20;

    public string StatusText => GetStatusText();
    public string StatusColor => GetStatusColor();
    public string PickupTimeText => MaxPickupTime.HasValue
        ? $"{MinPickupTime}~{MaxPickupTime}분"
        : $"{MinPickupTime}분";

    public StoreStateViewModel(PreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    public override void OnActivated()
    {
        base.OnActivated();
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _preferencesService.LoadSettings();
        ApplySettings(settings);
    }

    public void ApplySettings(AppSettings settings)
    {
        StoreName = settings.StoreName;
        MinPickupTime = settings.MinPickupTime;
        MaxPickupTime = settings.MaxPickupTime;
        OnPropertyChanged(nameof(PickupTimeText));
    }

    private string GetStatusText()
    {
        if (IsOpen) return "영업중";
        if (IsPaused && PausedUntil.HasValue)
            return $"일시정지 ({PausedUntil:HH:mm}까지)";
        if (IsPaused) return "일시정지";
        return "영업종료";
    }

    private string GetStatusColor()
    {
        if (IsOpen) return "#4CAF50";
        if (IsPaused) return "#FF9800";
        return "#F44336";
    }

    [RelayCommand]
    private void ToggleOpen()
    {
        if (IsOpen)
        {
            IsOpen = false;
            IsPaused = false;
            PausedUntil = null;
        }
        else
        {
            IsOpen = true;
            IsPaused = false;
            PausedUntil = null;
        }
        NotifyStatusChanged();
    }

    [RelayCommand]
    private void SetPause(int minutes)
    {
        IsOpen = false;
        IsPaused = true;
        PausedUntil = DateTime.Now.AddMinutes(minutes);
        NotifyStatusChanged();
    }

    private void NotifyStatusChanged()
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));
    }
}
