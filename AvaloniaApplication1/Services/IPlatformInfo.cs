namespace AvaloniaApplication1.Services;

/// <summary>
/// 플랫폼별 기능 및 특성 정보를 제공하는 인터페이스
/// </summary>
public interface IPlatformInfo
{
    /// <summary>
    /// 윈도우 컨트롤(최소화, 최대화, 닫기) 지원 여부
    /// Desktop: true, Mobile: false
    /// </summary>
    bool SupportsWindowControls { get; }

    /// <summary>
    /// 모바일 플랫폼 여부 (iOS, Android)
    /// </summary>
    bool IsMobile { get; }

    /// <summary>
    /// 플랫폼 이름 (Desktop, iOS, Android 등)
    /// </summary>
    string PlatformName { get; }

    /// <summary>
    /// 터치 기반 인터페이스 여부
    /// </summary>
    bool IsTouchPrimary { get; }
}
