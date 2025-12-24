using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    private const double BaseWidth = 1024;
    private const double BaseHeight = 768;

    public MainWindow()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ClientSizeProperty)
        {
            UpdateScaling();
        }
    }

    private void UpdateScaling()
    {
        var size = ClientSize;

        if (size.Width >= BaseWidth && size.Height >= BaseHeight)
        {
            // 큰 화면: ViewBox 비활성화, 레이아웃이 자연스럽게 늘어남
            RootViewbox.Stretch = Stretch.None;
            RootContent.Width = double.NaN;
            RootContent.Height = double.NaN;
        }
        else
        {
            // 작은 화면: ViewBox로 비율 유지하며 축소
            RootViewbox.Stretch = Stretch.Uniform;
            RootContent.Width = BaseWidth;
            RootContent.Height = BaseHeight;
        }
    }
}