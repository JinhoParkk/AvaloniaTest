using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.iOS.Services;

/// <summary>
/// iOS 플랫폼 정보 제공
/// </summary>
public class iOSPlatformInfo : IPlatformInfo
{
    public bool SupportsWindowControls => false;

    public bool IsMobile => true;

    public string PlatformName => "iOS";

    public bool IsTouchPrimary => true;
}
