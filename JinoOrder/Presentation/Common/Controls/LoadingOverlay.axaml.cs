using Avalonia;
using Avalonia.Controls;

namespace JinoOrder.Presentation.Common.Controls;

public partial class LoadingOverlay : UserControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<LoadingOverlay, bool>(nameof(IsLoading));

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<LoadingOverlay, string>(nameof(Message), "로딩 중...");

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public LoadingOverlay()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsLoadingProperty)
        {
            UpdateLoadingState();
        }
    }

    private void UpdateLoadingState()
    {
        if (IsLoading)
        {
            Classes.Add("loading");
        }
        else
        {
            Classes.Remove("loading");
        }
    }
}
