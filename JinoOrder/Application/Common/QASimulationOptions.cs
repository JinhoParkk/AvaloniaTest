using System;
using System.Collections.Generic;

namespace JinoOrder.Application.Common;

/// <summary>
/// QA 플랫폼 시뮬레이션 옵션
/// </summary>
public class QASimulationOptions
{
    /// <summary>
    /// 시뮬레이션 모드 활성화 여부
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// 시뮬레이션할 플랫폼 유형
    /// </summary>
    public PlatformType? SimulatedPlatform { get; init; }

    /// <summary>
    /// 시뮬레이션할 OS 유형
    /// </summary>
    public OSType? SimulatedOS { get; init; }

    /// <summary>
    /// 시뮬레이션할 화면 해상도
    /// </summary>
    public ScreenResolution? SimulatedResolution { get; init; }

    /// <summary>
    /// 사용된 디바이스 프리셋 이름
    /// </summary>
    public string? DevicePreset { get; init; }

    /// <summary>
    /// 비활성화된 기본 옵션
    /// </summary>
    public static QASimulationOptions None => new() { IsEnabled = false };

    /// <summary>
    /// 디바이스 프리셋 목록
    /// </summary>
    public static readonly Dictionary<string, (PlatformType Platform, OSType OS, ScreenResolution Resolution)> DevicePresets =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["iPhone14"] = (PlatformType.Mobile, OSType.iOS, ScreenResolution.iPhone14),
            ["iPhone14ProMax"] = (PlatformType.Mobile, OSType.iOS, ScreenResolution.iPhone14ProMax),
            ["iPadPro12"] = (PlatformType.Mobile, OSType.iOS, ScreenResolution.iPadPro12),
            ["GalaxyS23"] = (PlatformType.Mobile, OSType.Android, ScreenResolution.GalaxyS23),
            ["Pixel7"] = (PlatformType.Mobile, OSType.Android, ScreenResolution.Pixel7),
            ["Desktop1080p"] = (PlatformType.Desktop, OSType.Windows, ScreenResolution.Desktop1080p),
            ["Desktop1440p"] = (PlatformType.Desktop, OSType.Windows, ScreenResolution.Desktop1440p),
            ["Desktop4K"] = (PlatformType.Desktop, OSType.Windows, ScreenResolution.Desktop4K),
        };
}
