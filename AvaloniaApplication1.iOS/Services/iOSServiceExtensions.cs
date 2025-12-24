using AvaloniaApplication1.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.iOS.Services;

public static class iOSServiceExtensions
{
    public static IServiceCollection AddiOSServices(this IServiceCollection services)
    {
        // Register platform-specific services
        services.AddSingleton<IDialogService, iOSDialogService>();
        services.AddSingleton<IToastService, iOSToastService>();
        services.AddSingleton<IFileService, iOSFileService>();
        services.AddSingleton<ILocationService, iOSLocationService>();

        return services;
    }
}
