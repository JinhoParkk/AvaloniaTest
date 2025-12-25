using JinoOrder.Application.Common;
using UIKit;

namespace JinoOrder.iOS.Services;

public class iOSPlatformInfo : IPlatformInfo
{
    // iPhone은 모바일, iPad는 데스크톱으로 취급 (더 넓은 화면)
    public bool IsMobile => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone;

    public bool IsTablet => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
}
