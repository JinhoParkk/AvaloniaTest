using System;
using System.Threading.Tasks;

namespace JinoOrder.Application.Common;

public class LocationData
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? Accuracy { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public interface ILocationService
{
    /// <summary>
    /// Gets the current device location
    /// </summary>
    /// <returns>Current location or null if unavailable</returns>
    Task<LocationData?> GetCurrentLocationAsync();

    /// <summary>
    /// Checks if location permission is granted
    /// </summary>
    /// <returns>True if permission is granted</returns>
    Task<bool> CheckPermissionAsync();

    /// <summary>
    /// Requests location permission
    /// </summary>
    /// <returns>True if permission was granted</returns>
    Task<bool> RequestPermissionAsync();
}
