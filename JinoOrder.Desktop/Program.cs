using System;
using Avalonia;
using JinoOrder.Desktop.Services;

namespace JinoOrder.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Desktop 플랫폼 서비스 등록
        App.PlatformServices = new DesktopPlatformServiceProvider();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}