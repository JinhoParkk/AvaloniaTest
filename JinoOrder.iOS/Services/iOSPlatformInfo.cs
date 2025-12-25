using JinoOrder.Application.Common;
using UIKit;

namespace JinoOrder.iOS.Services;

public class iOSPlatformInfo : IPlatformInfo
{
    public bool IsMobile => true;

    public PlatformType Platform => PlatformType.Mobile;

    public OSType OperatingSystem => OSType.iOS;

    public ScreenResolution Resolution
    {
        get
        {
            var screen = UIScreen.MainScreen;
            return new ScreenResolution(
                (int)screen.Bounds.Width,
                (int)screen.Bounds.Height
            );
        }
    }

    public bool IsSimulated => false;
}
