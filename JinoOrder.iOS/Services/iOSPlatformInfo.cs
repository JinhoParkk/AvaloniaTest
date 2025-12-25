using JinoOrder.Application.Common;
using UIKit;

namespace JinoOrder.iOS.Services;

public class iOSPlatformInfo : IPlatformInfo
{
    // iPhone은 모바일, iPad는 태블릿으로 취급
    public bool IsMobile => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone;

    public bool IsTablet => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;

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
