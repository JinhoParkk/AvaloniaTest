using Avalonia.Controls;
using AvaloniaApplication1.Services;
using DialogHostAvalonia;

namespace AvaloniaApplication1.iOS.Services;

public class iOSDialogService : IDialogService
{
    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var dialog = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 10,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Children =
                    {
                        new Button
                        {
                            Content = "Cancel",
                            Command = new RelayCommand(() => DialogHost.Close(null, false))
                        },
                        new Button
                        {
                            Content = "OK",
                            Command = new RelayCommand(() => DialogHost.Close(null, true))
                        }
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
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new Button
                {
                    Content = "OK",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Command = new RelayCommand(() => DialogHost.Close(null))
                }
            }
        };

        await DialogHost.Show(dialog, title);
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        var dialog = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Foreground = Avalonia.Media.Brushes.Red
                },
                new Button
                {
                    Content = "OK",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Command = new RelayCommand(() => DialogHost.Close(null))
                }
            }
        };

        await DialogHost.Show(dialog, title);
    }

    public async Task<string?> ShowInputAsync(string title, string message, string defaultValue = "")
    {
        var textBox = new TextBox { Text = defaultValue, Watermark = message };

        var dialog = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                textBox,
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 10,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Children =
                    {
                        new Button
                        {
                            Content = "Cancel",
                            Command = new RelayCommand(() => DialogHost.Close(null, null))
                        },
                        new Button
                        {
                            Content = "OK",
                            Command = new RelayCommand(() => DialogHost.Close(null, textBox.Text))
                        }
                    }
                }
            }
        };

        var result = await DialogHost.Show(dialog, title);
        return result as string;
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
