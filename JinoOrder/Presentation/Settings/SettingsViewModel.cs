using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Domain.Settings;
using JinoOrder.Infrastructure.Storage;
using JinoOrder.Presentation.Common;

namespace JinoOrder.Presentation.Settings;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly PreferencesService _preferencesService;
    private readonly Action<AppSettings>? _onSettingsSaved;

    [ObservableProperty] private string _storeName = "지노커피";
    [ObservableProperty] private decimal _minPickupTime = 10;
    [ObservableProperty] private decimal _maxPickupTime = 20;
    [ObservableProperty] private TimeSpan _openTime = new TimeSpan(9, 0, 0);
    [ObservableProperty] private TimeSpan _closeTime = new TimeSpan(22, 0, 0);
    [ObservableProperty] private bool _enableOrderSound = true;
    [ObservableProperty] private bool _enableOrderNotification = true;
    [ObservableProperty] private bool _enableAutoOpenClose;
    [ObservableProperty] private bool _enableAutoAccept;
    [ObservableProperty] private decimal _autoAcceptPrepTime = 15;
    [ObservableProperty] private bool _showSaveMessage;

    public SettingsViewModel()
    {
        _preferencesService = new PreferencesService();
        LoadSettings();
    }

    public SettingsViewModel(PreferencesService preferencesService, Action<AppSettings>? onSettingsSaved = null)
    {
        _preferencesService = preferencesService;
        _onSettingsSaved = onSettingsSaved;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _preferencesService.LoadSettings();
        ApplySettings(settings);
    }

    private void ApplySettings(AppSettings settings)
    {
        StoreName = settings.StoreName;
        MinPickupTime = settings.MinPickupTime;
        MaxPickupTime = settings.MaxPickupTime ?? 0;
        OpenTime = settings.OpenTime;
        CloseTime = settings.CloseTime;
        EnableOrderSound = settings.EnableOrderSound;
        EnableOrderNotification = settings.EnableOrderNotification;
        EnableAutoOpenClose = settings.EnableAutoOpenClose;
        EnableAutoAccept = settings.EnableAutoAccept;
        AutoAcceptPrepTime = settings.AutoAcceptPrepTime;
    }

    private AppSettings GetCurrentSettings()
    {
        return new AppSettings
        {
            StoreName = StoreName,
            MinPickupTime = (int)MinPickupTime,
            MaxPickupTime = MaxPickupTime > 0 ? (int)MaxPickupTime : null,
            OpenTime = OpenTime,
            CloseTime = CloseTime,
            EnableOrderSound = EnableOrderSound,
            EnableOrderNotification = EnableOrderNotification,
            EnableAutoOpenClose = EnableAutoOpenClose,
            EnableAutoAccept = EnableAutoAccept,
            AutoAcceptPrepTime = (int)AutoAcceptPrepTime
        };
    }

    [RelayCommand]
    private async Task Save()
    {
        var settings = GetCurrentSettings();
        _preferencesService.SaveSettings(settings);
        _onSettingsSaved?.Invoke(settings);

        ShowSaveMessage = true;
        await Task.Delay(2000);
        ShowSaveMessage = false;
    }

    [RelayCommand]
    private void Reset()
    {
        _preferencesService.ResetSettings();
        var defaultSettings = new AppSettings();
        ApplySettings(defaultSettings);
    }
}
