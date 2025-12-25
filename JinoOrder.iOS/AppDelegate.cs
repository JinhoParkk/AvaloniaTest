using Foundation;
using Avalonia;
using Avalonia.iOS;
using JinoOrder.iOS.Services;

namespace JinoOrder.iOS;

[Register("AppDelegate")]
#pragma warning disable CA1711
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        // iOS 플랫폼 서비스 등록
        App.PlatformServices = new iOSPlatformServiceProvider();

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}