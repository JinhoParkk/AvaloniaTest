using System;
using System.Threading.Tasks;
using JinoOrder.Application.Common;

namespace JinoOrder.Desktop.Services;

public class DesktopLocationService : ILocationService
{
    public Task<LocationData?> GetCurrentLocationAsync()
    {
        // Desktop platforms typically don't have built-in location services
        // This would require platform-specific implementations or external APIs
        throw new NotSupportedException("Location services are not supported on desktop platforms by default. Consider using IP-based geolocation or external APIs.");
    }

    public Task<bool> CheckPermissionAsync()
    {
        return Task.FromResult(false);
    }

    public Task<bool> RequestPermissionAsync()
    {
        return Task.FromResult(false);
    }
}
