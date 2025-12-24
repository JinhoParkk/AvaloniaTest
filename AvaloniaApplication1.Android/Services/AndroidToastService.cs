using Android.Widget;
using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.Android.Services;

public class AndroidToastService : IToastService
{
    public Task ShowAsync(string title, string message, ToastType type = ToastType.Information, int durationMs = 3000)
    {
        var fullMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";
        var duration = durationMs > 2000 ? ToastLength.Long : ToastLength.Short;

        global::Android.App.Application.Context.MainExecutor?.Execute(() =>
        {
            Toast.MakeText(global::Android.App.Application.Context, fullMessage, duration)?.Show();
        });

        return Task.CompletedTask;
    }
}
