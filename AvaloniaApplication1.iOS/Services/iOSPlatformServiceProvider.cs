using AvaloniaApplication1.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.iOS.Services;

/// <summary>
/// iOS 플랫폼 서비스 제공자
/// </summary>
public class iOSPlatformServiceProvider : IPlatformServiceProvider
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPlatformInfo, iOSPlatformInfo>();
        services.AddSingleton<IDialogService, iOSDialogService>();
        services.AddSingleton<IToastService, iOSToastService>();
        services.AddSingleton<IFileService, iOSFileService>();
        services.AddSingleton<ILocationService, iOSLocationService>();
    }
}
