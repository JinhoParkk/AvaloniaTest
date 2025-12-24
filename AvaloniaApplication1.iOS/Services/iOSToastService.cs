using AvaloniaApplication1.Services;
using UIKit;

namespace AvaloniaApplication1.iOS.Services;

public class iOSToastService : IToastService
{
    public Task ShowAsync(string title, string message, ToastType type = ToastType.Information, int durationMs = 3000)
    {
        var fullMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";

        UIApplication.SharedApplication.InvokeOnMainThread(() =>
        {
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            var rootViewController = UIApplication.SharedApplication.KeyWindow?.RootViewController;
            rootViewController?.PresentViewController(alert, true, null);

            Task.Delay(durationMs).ContinueWith(_ =>
            {
                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    alert.DismissViewController(true, null);
                });
            });
        });

        return Task.CompletedTask;
    }
}
