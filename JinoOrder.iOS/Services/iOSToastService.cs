using System.Linq;
using System.Threading.Tasks;
using JinoOrder.Application.Common;
using UIKit;

namespace JinoOrder.iOS.Services;

public class iOSToastService : IToastService
{
    public Task ShowAsync(string title, string message, ToastType type = ToastType.Information, int durationMs = 3000)
    {
        UIApplication.SharedApplication.InvokeOnMainThread(() =>
        {
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            // iOS 13+ compatible: use ConnectedScenes instead of deprecated KeyWindow
            var windowScene = UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .FirstOrDefault();
            var rootViewController = windowScene?.Windows
                .FirstOrDefault(w => w.IsKeyWindow)?.RootViewController;

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
