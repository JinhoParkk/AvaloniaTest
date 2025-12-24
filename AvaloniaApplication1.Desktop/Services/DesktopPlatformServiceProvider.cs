using Avalonia.Controls;
using AvaloniaApplication1.Services;
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
        services.AddSingleton<IDialogService, DesktopDialogService>();
        services.AddSingleton<IToastService, DesktopToastService>();
        services.AddSingleton<IFileService>(_ => new DesktopFileService(_mainWindow ?? App.MainWindow));
        services.AddSingleton<ILocationService, DesktopLocationService>();
    }
}
