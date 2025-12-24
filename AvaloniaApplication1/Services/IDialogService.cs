using System.Threading.Tasks;

namespace AvaloniaApplication1.Services;

public interface IDialogService
{
    /// <summary>
    /// Shows a confirmation dialog
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    /// <returns>True if user confirmed, false otherwise</returns>
    Task<bool> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Shows an information dialog
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    Task ShowInformationAsync(string title, string message);

    /// <summary>
    /// Shows an error dialog
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Error message</param>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows an input dialog
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Prompt message</param>
    /// <param name="defaultValue">Default input value</param>
    /// <returns>User input or null if cancelled</returns>
    Task<string?> ShowInputAsync(string title, string message, string defaultValue = "");
}
