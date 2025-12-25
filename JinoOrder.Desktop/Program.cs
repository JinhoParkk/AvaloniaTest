using System;
using Avalonia;
using JinoOrder.Desktop.Configuration;
using JinoOrder.Desktop.Services;

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

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}