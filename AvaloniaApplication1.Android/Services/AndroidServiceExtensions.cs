using AvaloniaApplication1.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.Android.Services;

public static class AndroidServiceExtensions
{
    public static IServiceCollection AddAndroidServices(this IServiceCollection services)
    {
        // Register platform-specific services
        services.AddSingleton<IDialogService, AndroidDialogService>();
        services.AddSingleton<IToastService, AndroidToastService>();
        services.AddSingleton<IFileService, AndroidFileService>();
        services.AddSingleton<ILocationService, AndroidLocationService>();

        return services;
    }
}
