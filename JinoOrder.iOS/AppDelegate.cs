using System;
using Foundation;
using UIKit;
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

    /// <summary>
    /// 딥링크 URL로 앱이 열릴 때 호출됨
    /// </summary>
    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        if (url != null)
        {
            var uriString = url.AbsoluteString;
            if (!string.IsNullOrEmpty(uriString))
            {
                // 앱이 이미 초기화되었으면 바로 처리, 아니면 대기열에 추가
                if (App.Services != null)
                {
                    App.HandleDeepLink(uriString);
                }
                else
                {
                    App.PendingDeepLink = uriString;
                }
            }
        }

        return base.OpenUrl(application, url, options);
    }

    /// <summary>
    /// 딥링크 URL로 앱이 처음 실행될 때 호출됨
    /// </summary>
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // 앱이 딥링크로 시작되었는지 확인
        if (launchOptions != null &&
            launchOptions.ContainsKey(UIApplication.LaunchOptionsUrlKey))
        {
            var url = launchOptions[UIApplication.LaunchOptionsUrlKey] as NSUrl;
            if (url != null)
            {
                App.PendingDeepLink = url.AbsoluteString;
            }
        }

        return base.FinishedLaunching(application, launchOptions);
    }
}