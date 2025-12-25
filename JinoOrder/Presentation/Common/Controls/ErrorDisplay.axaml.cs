using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace JinoOrder.Presentation.Common.Controls;

public partial class ErrorDisplay : UserControl
{
    public static readonly StyledProperty<string?> ErrorMessageProperty =
        AvaloniaProperty.Register<ErrorDisplay, string?>(nameof(ErrorMessage));

    public static readonly StyledProperty<bool> CanDismissProperty =
        AvaloniaProperty.Register<ErrorDisplay, bool>(nameof(CanDismiss), true);

    public static readonly StyledProperty<ICommand?> DismissCommandProperty =
        AvaloniaProperty.Register<ErrorDisplay, ICommand?>(nameof(DismissCommand));

    public string? ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public bool CanDismiss
    {
        get => GetValue(CanDismissProperty);
        set => SetValue(CanDismissProperty, value);
    }

    public ICommand? DismissCommand
    {
        get => GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public ErrorDisplay()
    {
        InitializeComponent();

        // Default dismiss command
        DismissCommand = new RelayCommand(() => ErrorMessage = null);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ErrorMessageProperty)
        {
            UpdateErrorState();
        }
    }

    private void UpdateErrorState()
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            Classes.Add("hasError");
        }
        else
        {
            Classes.Remove("hasError");
        }
    }
}
