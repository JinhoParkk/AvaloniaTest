using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // 플랫폼 정보
    private readonly IPlatformInfo? _platformInfo;

    // 매장 정보
    [ObservableProperty] private string _storeName = "패스오더 강남점";
    [ObservableProperty] private bool _isOpen = true;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private DateTime? _pausedUntil;
    [ObservableProperty] private int _minPickupTime = 15;
    [ObservableProperty] private int? _maxPickupTime = 30;

    // 탭 선택
    [ObservableProperty] private int _selectedTabIndex;

    // 상태 텍스트
    public string StatusText => GetStatusText();
    public string PickupTimeText => GetPickupTimeText();
    public string StatusColor => GetStatusColor();

    // 플랫폼별 UI 표시 여부
    public bool ShowWindowControls => _platformInfo?.SupportsWindowControls ?? true;
    public bool IsMobilePlatform => _platformInfo?.IsMobile ?? false;

    // 윈도우 컨트롤 이벤트
    public event Action? MinimizeRequested;
    public event Action? MaximizeRequested;
    public event Action? CloseRequested;

    public MainViewModel() : this(null)
    {
    }

    public MainViewModel(IPlatformInfo? platformInfo)
    {
        _platformInfo = platformInfo;
        // 임시 데이터 (나중에 API에서 가져옴)
        LoadStoreData();
    }

    private void LoadStoreData()
    {
        // TODO: API에서 매장 정보 로드
        // 현재는 임시 데이터 사용
    }

    private string GetStatusText()
    {
        if (IsOpen) return "영업중";
        if (IsPaused && PausedUntil.HasValue)
            return $"일시정지 ({PausedUntil:HH:mm}까지)";
        if (IsPaused)
            return "일시정지";
        return "영업종료";
    }

    private string GetPickupTimeText()
    {
        if (MaxPickupTime.HasValue)
            return $"{MinPickupTime}~{MaxPickupTime}분";
        return $"{MinPickupTime}분";
    }

    private string GetStatusColor()
    {
        if (IsOpen) return "#4CAF50"; // 녹색
        if (IsPaused) return "#FF9800"; // 주황색
        return "#F44336"; // 빨간색
    }

    // 매장 상태 변경 커맨드
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

    [RelayCommand]
    private void CancelPause()
    {
        IsPaused = false;
        PausedUntil = null;
        IsOpen = true;
        NotifyStatusChanged();
    }

    private void NotifyStatusChanged()
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));
    }

    // 윈도우 컨트롤 커맨드
    [RelayCommand]
    private void MinimizeWindow()
    {
        MinimizeRequested?.Invoke();
    }

    [RelayCommand]
    private void MaximizeWindow()
    {
        MaximizeRequested?.Invoke();
    }

    [RelayCommand]
    private void CloseWindow()
    {
        CloseRequested?.Invoke();
    }
}
