using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JinoOrder.Domain.Common;
using JinoOrder.Domain.Settings;
using JinoOrder.Infrastructure.Storage;
using JinoOrder.Presentation.Common;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Presentation.Settings;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly PreferencesService _preferencesService;
    private readonly Action<AppSettings>? _onSettingsSaved;

    [ObservableProperty] private string _storeName = AppConstants.DefaultStoreName;
    [ObservableProperty] private decimal _minPickupTime = AppConstants.DefaultMinPickupTime;
    [ObservableProperty] private decimal _maxPickupTime = AppConstants.DefaultMaxPickupTime;
    [ObservableProperty] private TimeSpan _openTime = AppConstants.DefaultOpenTime;
    [ObservableProperty] private TimeSpan _closeTime = AppConstants.DefaultCloseTime;
    [ObservableProperty] private bool _enableOrderSound = true;
    [ObservableProperty] private bool _enableOrderNotification = true;
    [ObservableProperty] private bool _enableAutoOpenClose;
    [ObservableProperty] private bool _enableAutoAccept;
    [ObservableProperty] private decimal _autoAcceptPrepTime = 15;
    [ObservableProperty] private bool _showSaveMessage;

    public SettingsViewModel(PreferencesService preferencesService, ILogger<SettingsViewModel> logger, Action<AppSettings>? onSettingsSaved = null)
    {
        _preferencesService = preferencesService;
        _onSettingsSaved = onSettingsSaved;
        Logger = logger;

        Logger.LogDebug("SettingsViewModel 초기화됨");
        LoadSettings();
    }

    private void LoadSettings()
    {
        Logger.LogDebug("설정 로드 시작");
        var settings = _preferencesService.LoadSettings();
        ApplySettings(settings);
        Logger.LogDebug("설정 로드 완료: StoreName={StoreName}", settings.StoreName);
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

    private bool ValidateSettings()
    {
        ClearError();

        if (OpenTime >= CloseTime)
        {
            SetError(ValidationMessages.InvalidTimeRange);
            Logger.LogWarning("설정 검증 실패: 잘못된 영업 시간 범위");
            return false;
        }

        if (MinPickupTime < 1)
        {
            SetError(ValidationMessages.InvalidPickupTime);
            Logger.LogWarning("설정 검증 실패: 최소 픽업 시간이 너무 작음");
            return false;
        }

        if (MaxPickupTime > 0 && MinPickupTime > MaxPickupTime)
        {
            SetError(ValidationMessages.MinPickupTimeTooLarge);
            Logger.LogWarning("설정 검증 실패: 최소 픽업 시간이 최대보다 큼");
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!ValidateSettings())
        {
            return;
        }

        Logger.LogInformation("설정 저장 시도");

        await ExecuteAsync(async ct =>
        {
            var settings = GetCurrentSettings();
            _preferencesService.SaveSettings(settings);
            _onSettingsSaved?.Invoke(settings);

            Logger.LogInformation("설정 저장 완료: StoreName={StoreName}", settings.StoreName);

            ShowSaveMessage = true;
            await Task.Delay(TimingConstants.SaveMessageDisplayMs, ct);
            ShowSaveMessage = false;
        }, "설정 저장", showLoading: false);
    }

    [RelayCommand]
    private void Reset()
    {
        Logger.LogInformation("설정 초기화");
        _preferencesService.ResetSettings();
        var defaultSettings = new AppSettings();
        ApplySettings(defaultSettings);
        ClearError();
        Logger.LogInformation("설정 초기화 완료");
    }
}
