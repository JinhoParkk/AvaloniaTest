using JinoOrder.Application.Common;

namespace JinoOrder.iOS.Services;

public class iOSPlatformInfo : IPlatformInfo
{
    public bool IsMobile => true;
}
