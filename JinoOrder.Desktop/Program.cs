using System;
using System.Linq;
using Avalonia;
using JinoOrder.Desktop.Configuration;
using JinoOrder.Desktop.Services;
using JinoOrder.Domain.Common;

namespace JinoOrder.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // QA 시뮬레이션 옵션 파싱
        var simulationOptions = CommandLineParser.Parse(args);

        // Desktop 플랫폼 서비스 등록 (시뮬레이션 옵션 포함)
        App.PlatformServices = new DesktopPlatformServiceProvider(
            simulationOptions: simulationOptions
        );

        // 딥링크 인자 확인 (jinoorder:// 로 시작하는 인자)
        var deepLinkArg = args.FirstOrDefault(arg =>
            arg.StartsWith($"{AppConstants.DeepLinkScheme}://", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(deepLinkArg))
        {
            App.PendingDeepLink = deepLinkArg;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}