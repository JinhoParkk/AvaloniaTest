using System;
using System.Threading.Tasks;
using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.Desktop.Services;

public class DesktopToastService : IToastService
{
    public Task ShowAsync(string title, string message, ToastType type = ToastType.Information, int durationMs = 3000)
    {
        // TODO: Implement proper desktop notifications
        // For now, just write to console
        Console.WriteLine($"[{type}] {title}: {message}");
        return Task.CompletedTask;
    }
}

