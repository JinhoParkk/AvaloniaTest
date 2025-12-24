using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.Android.Services;

public class AndroidLocationService : ILocationService
{
    public async Task<LocationData?> GetCurrentLocationAsync()
    {
        try
        {
            var location = await Microsoft.Maui.Devices.Sensors.Geolocation.GetLocationAsync(new Microsoft.Maui.Devices.Sensors.GeolocationRequest
            {
                DesiredAccuracy = Microsoft.Maui.Devices.Sensors.GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location != null)
            {
                return new LocationData
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Altitude = location.Altitude,
                    Accuracy = location.Accuracy,
                    Timestamp = location.Timestamp
                };
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> CheckPermissionAsync()
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
        return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
    }

    public async Task<bool> RequestPermissionAsync()
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
        return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
    }
}
