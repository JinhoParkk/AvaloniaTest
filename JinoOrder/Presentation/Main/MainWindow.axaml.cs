using Avalonia.Controls;
using JinoOrder.Application.Common;

namespace JinoOrder.Presentation.Main;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ApplySimulatedResolution();
    }

    private void ApplySimulatedResolution()
    {
        // 시뮬레이션 해상도가 설정된 경우 윈도우 크기 적용
        var options = App.PlatformServices?.SimulationOptions;
        if (options is { IsEnabled: true, SimulatedResolution: { } resolution })
        {
            Width = resolution.Width;
            Height = resolution.Height;
            CanResize = false; // 테스트 일관성을 위해 크기 고정
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
