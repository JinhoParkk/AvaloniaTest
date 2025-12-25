using System;
using System.Threading.Tasks;
using JinoOrder.Application.Common;
using DesktopNotifications;

namespace JinoOrder.Desktop.Services;

public class DesktopToastService : IToastService
{
    private readonly INotificationManager? _notificationManager;

    public DesktopToastService(INotificationManager? notificationManager = null)
    {
        _notificationManager = notificationManager;
    }

    public async Task ShowAsync(string title, string message, ToastType type = ToastType.Information, int durationMs = 3000)
    {
        if (_notificationManager == null)
        {
            // Fallback to console if notification manager is not available
            Console.WriteLine($"[{type}] {title}: {message}");
            return;
        }

        try
        {
            var notification = new Notification
            {
                Title = title,
                Body = message
            };

            await _notificationManager.ShowNotification(notification);
        }
        catch (Exception ex)
        {
            // Fallback to console on error
            Console.WriteLine($"[{type}] {title}: {message} (Notification error: {ex.Message})");
        }
    }
}
