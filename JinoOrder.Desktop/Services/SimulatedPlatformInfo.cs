using JinoOrder.Application.Common;

namespace JinoOrder.Desktop.Services;

/// <summary>
/// QA 테스트용 시뮬레이션 플랫폼 정보
/// </summary>
public class SimulatedPlatformInfo : IPlatformInfo
{
    private readonly QASimulationOptions _options;
    private readonly IPlatformInfo _realPlatformInfo;

    public SimulatedPlatformInfo(QASimulationOptions options, IPlatformInfo realPlatformInfo)
    {
        _options = options;
        _realPlatformInfo = realPlatformInfo;
    }

    public bool IsMobile => Platform == PlatformType.Mobile;

    public bool IsTablet => _realPlatformInfo.IsTablet;

    public PlatformType Platform => _options.SimulatedPlatform ?? _realPlatformInfo.Platform;

    public OSType OperatingSystem => _options.SimulatedOS ?? _realPlatformInfo.OperatingSystem;

    public ScreenResolution Resolution => _options.SimulatedResolution ?? _realPlatformInfo.Resolution;

    public bool IsSimulated => true;
}
