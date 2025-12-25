using System;
using Avalonia.Controls;
using JinoOrder.Application.Common;
using DesktopNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace JinoOrder.Desktop.Services;

/// <summary>
/// Desktop 플랫폼 서비스 제공자
/// </summary>
public class DesktopPlatformServiceProvider : IPlatformServiceProvider
{
    private readonly Window? _mainWindow;
    private readonly QASimulationOptions _simulationOptions;

    public DesktopPlatformServiceProvider(
        Window? mainWindow = null,
        QASimulationOptions? simulationOptions = null)
    {
        _mainWindow = mainWindow;
        _simulationOptions = simulationOptions ?? QASimulationOptions.None;
    }

    /// <summary>
    /// 시뮬레이션 옵션 반환 (윈도우 크기 설정용)
    /// </summary>
    public QASimulationOptions SimulationOptions => _simulationOptions;

    public void ConfigureServices(IServiceCollection services)
    {
        // 시뮬레이션 모드에 따라 플랫폼 정보 등록
        if (_simulationOptions.IsEnabled)
        {
            var realPlatformInfo = new DesktopPlatformInfo();
            var simulatedInfo = new SimulatedPlatformInfo(_simulationOptions, realPlatformInfo);
            services.AddSingleton<IPlatformInfo>(simulatedInfo);

            // 시뮬레이션 정보 로깅
            Console.WriteLine($"[QA Mode] Platform: {simulatedInfo.Platform}");
            Console.WriteLine($"[QA Mode] OS: {simulatedInfo.OperatingSystem}");
            Console.WriteLine($"[QA Mode] Resolution: {simulatedInfo.Resolution}");
        }
        else
        {
            services.AddSingleton<IPlatformInfo, DesktopPlatformInfo>();
        }

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
