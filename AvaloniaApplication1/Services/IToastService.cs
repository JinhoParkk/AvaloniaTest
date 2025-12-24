using System.Threading.Tasks;

namespace AvaloniaApplication1.Services;

public enum ToastType
{
    Information,
    Success,
    Warning,
    Error
}

public interface IToastService
{
    /// <summary>
    /// Shows a toast notification
    /// </summary>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="type">Toast type</param>
    /// <param name="durationMs">Duration in milliseconds (optional)</param>
    Task ShowAsync(string title, string message, ToastType type = ToastType.Information, int durationMs = 3000);
}
