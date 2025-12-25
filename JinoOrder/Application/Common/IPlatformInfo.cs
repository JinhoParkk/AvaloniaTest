namespace JinoOrder.Application.Common;

/// <summary>
/// 플랫폼 정보 - 모바일/데스크탑 구분 및 QA 테스트용 시뮬레이션 지원
/// </summary>
public interface IPlatformInfo
{
    /// <summary>
    /// 모바일 플랫폼 여부 (iOS, Android)
    /// </summary>
    bool IsMobile { get; }

    /// <summary>
    /// 플랫폼 유형: Desktop, Mobile, Web
    /// </summary>
    PlatformType Platform { get; }

    /// <summary>
    /// 운영체제 유형
    /// </summary>
    OSType OperatingSystem { get; }

    /// <summary>
    /// 화면 해상도
    /// </summary>
    ScreenResolution Resolution { get; }

    /// <summary>
    /// QA 시뮬레이션 모드 여부
    /// </summary>
    bool IsSimulated { get; }
}
