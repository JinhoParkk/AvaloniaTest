using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaApplication1.Services;
using DialogHostAvalonia;

namespace AvaloniaApplication1.Desktop.Services;

public class DesktopDialogService : IDialogService
{
    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var dialog = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    HorizontalAlignment = HorizontalAlignment.Right,
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
                new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                new Button
                {
                    Content = "OK",
                    HorizontalAlignment = HorizontalAlignment.Right,
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
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Red
                },
                new Button
                {
                    Content = "OK",
                    HorizontalAlignment = HorizontalAlignment.Right,
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
                new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                textBox,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    HorizontalAlignment = HorizontalAlignment.Right,
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

    private class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute) => _execute = execute;

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }
}
