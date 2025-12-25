using System;
using Avalonia.Controls;
using AvaloniaApplication1.Services;
using DesktopNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.Desktop.Services;

/// <summary>
/// Desktop 플랫폼 서비스 제공자
/// </summary>
public class DesktopPlatformServiceProvider : IPlatformServiceProvider
{
    private readonly Window? _mainWindow;

    public DesktopPlatformServiceProvider(Window? mainWindow = null)
    {
        _mainWindow = mainWindow;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPlatformInfo, DesktopPlatformInfo>();
        services.AddSingleton<IDialogService, DesktopDialogService>();

        // Desktop notifications
        services.AddSingleton<INotificationManager>(_ => CreateNotificationManager());
        services.AddSingleton<IToastService, DesktopToastService>();

        services.AddSingleton<IFileService>(_ => new DesktopFileService(_mainWindow ?? App.MainWindow));
        services.AddSingleton<ILocationService, DesktopLocationService>();
    }

    private static INotificationManager? CreateNotificationManager()
    {
        try
        {
            // DesktopNotifications.Avalonia platform-specific notification managers
            if (OperatingSystem.IsWindows())
            {
                return new DesktopNotifications.Windows.WindowsNotificationManager();
            }
            else if (OperatingSystem.IsLinux())
            {
                return new DesktopNotifications.FreeDesktop.FreeDesktopNotificationManager();
            }
            // Note: macOS requires DesktopNotifications.Apple package which needs additional setup
            // For now, return null and fallback to console on macOS
        }
        catch (Exception)
        {
            // Return null if notification manager creation fails
            // DesktopToastService will fallback to console output
        }

        return null;
    }
}
