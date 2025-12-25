using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using JinoOrder.Application.Common;
using DialogHostAvalonia;

namespace JinoOrder.iOS.Services;

public class iOSDialogService : IDialogService
{
    // iOS Human Interface Guidelines: minimum touch target size
    private const int MinTouchTargetHeight = 44;
    private const int MobileSpacing = 16;
    private const int DialogMinWidth = 280;
    private const int ButtonMinWidth = 100;
    private const int MobileFontSize = 16;

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var dialog = new StackPanel
        {
            Spacing = MobileSpacing,
            MinWidth = DialogMinWidth,
            Margin = new Thickness(MobileSpacing),
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = MobileFontSize
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = MobileSpacing,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Children =
                    {
                        CreateTouchButton("Cancel", () => DialogHost.Close(null, false)),
                        CreateTouchButton("OK", () => DialogHost.Close(null, true), isPrimary: true)
                    }
                }
            }
        };

        var result = await DialogHost.Show(dialog, title);
        return result is true;
    }

    public async Task ShowInformationAsync(string title, string message)
    {
        var dialog = new StackPanel
        {
            Spacing = MobileSpacing,
            MinWidth = DialogMinWidth,
            Margin = new Thickness(MobileSpacing),
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = MobileFontSize
                },
                CreateTouchButton("OK", () => DialogHost.Close(null), isPrimary: true)
            }
        };

        await DialogHost.Show(dialog, title);
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        var dialog = new StackPanel
        {
            Spacing = MobileSpacing,
            MinWidth = DialogMinWidth,
            Margin = new Thickness(MobileSpacing),
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = MobileFontSize,
                    Foreground = Brushes.Red
                },
                CreateTouchButton("OK", () => DialogHost.Close(null), isPrimary: true)
            }
        };

        await DialogHost.Show(dialog, title);
    }

    public async Task<string?> ShowInputAsync(string title, string message, string defaultValue = "")
    {
        var textBox = new TextBox
        {
            Text = defaultValue,
            Watermark = message,
            MinHeight = MinTouchTargetHeight,
            FontSize = MobileFontSize,
            Padding = new Thickness(12, 8)
        };

        var dialog = new StackPanel
        {
            Spacing = MobileSpacing,
            MinWidth = DialogMinWidth,
            Margin = new Thickness(MobileSpacing),
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = MobileFontSize
                },
                textBox,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = MobileSpacing,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Children =
                    {
                        CreateTouchButton("Cancel", () => DialogHost.Close(null, null)),
                        CreateTouchButton("OK", () => DialogHost.Close(null, textBox.Text), isPrimary: true)
                    }
                }
            }
        };

        var result = await DialogHost.Show(dialog, title);
        return result as string;
    }

    private static Button CreateTouchButton(string content, Action onClick, bool isPrimary = false)
    {
        return new Button
        {
            Content = content,
            MinHeight = MinTouchTargetHeight,
            MinWidth = ButtonMinWidth,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = MobileFontSize,
            Padding = new Thickness(16, 8),
            Command = new RelayCommand(onClick)
        };
    }

    private class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute) => _execute = execute;

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }
}
