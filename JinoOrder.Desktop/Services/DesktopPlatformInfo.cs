using JinoOrder.Application.Common;

namespace JinoOrder.Desktop.Services;

public class DesktopPlatformInfo : IPlatformInfo
{
    public bool IsMobile => false;

    public bool IsTablet => false;

    public PlatformType Platform => PlatformType.Desktop;

    public OSType OperatingSystem
    {
        get
        {
            if (System.OperatingSystem.IsWindows()) return OSType.Windows;
            if (System.OperatingSystem.IsMacOS()) return OSType.MacOS;
            if (System.OperatingSystem.IsLinux()) return OSType.Linux;
            return OSType.Windows; // Fallback
        }
    }

    public ScreenResolution Resolution => ScreenResolution.Desktop1080p; // 기본값

    public bool IsSimulated => false;
}
