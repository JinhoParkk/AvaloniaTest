using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.Desktop.Services;

/// <summary>
/// Desktop 플랫폼 정보 제공
/// </summary>
public class DesktopPlatformInfo : IPlatformInfo
{
    public bool SupportsWindowControls => true;

    public bool IsMobile => false;

    public string PlatformName => "Desktop";

    public bool IsTouchPrimary => false;
}
