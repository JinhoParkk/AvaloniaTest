using Avalonia.Controls;
using AvaloniaApplication1.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.Desktop.Services;

public static class DesktopServiceExtensions
{
    public static IServiceCollection AddDesktopServices(this IServiceCollection services, Window? mainWindow = null)
    {
        // Register platform-specific services
        services.AddSingleton<IDialogService, DesktopDialogService>();
        services.AddSingleton<IToastService, DesktopToastService>();
        services.AddSingleton<IFileService>(sp => new DesktopFileService(mainWindow));
        services.AddSingleton<ILocationService, DesktopLocationService>();

        return services;
    }
}
