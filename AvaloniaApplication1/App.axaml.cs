using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.Extensions;
using AvaloniaApplication1.Infrastructure.Http.Refit;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1;

public partial class App : Application
{
    /// <summary>
    /// 전역 ServiceProvider (DI 컨테이너)
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// 플랫폼별 서비스 제공자 (Desktop/iOS/Android에서 설정)
    /// </summary>
    public static IPlatformServiceProvider? PlatformServices { get; set; }

    /// <summary>
    /// Desktop 플랫폼용 MainWindow (파일 다이얼로그 등에 필요)
    /// </summary>
    public static Window? MainWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            // MainWindow 생성
            desktop.MainWindow = new MainWindow();
            MainWindow = desktop.MainWindow;

            // DI 컨테이너 구성
            ConfigureServices();

            // 앱 시작
            InitializeApp(vm => desktop.MainWindow.DataContext = vm);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // DI 컨테이너 구성
            ConfigureServices();

            // 앱 시작
            InitializeApp(vm =>
            {
                singleViewPlatform.MainView = new MainView { DataContext = vm };
            });
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// DI 컨테이너 구성
    /// </summary>
    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // 1. 핵심 서비스 등록
        services.AddCoreServices();

        // 2. Refit API 클라이언트 서비스 등록 (Java Retrofit 스타일)
        services.AddRefitApiServices(options =>
        {
            options.BaseUrl = "https://api.passorder.com";
            options.RefreshTokenEndpoint = "/auth/refresh";
        });

        // 3. ViewModel 등록
        services.AddViewModels();

        // 4. 플랫폼별 서비스 등록 (IPlatformServiceProvider 사용)
        PlatformServices?.ConfigureServices(services);

        // ServiceProvider 생성
        Services = services.BuildServiceProvider();

        // NavigationService 초기화
        var navigationService = Services.GetRequiredService<NavigationService>();
        navigationService.Initialize(Services);
    }

    /// <summary>
    /// 앱 초기화 및 시작 ViewModel 결정
    /// </summary>
    private void InitializeApp(Action<MainWindowViewModel> setDataContext)
    {
        // MainWindowViewModel 생성 (DI에서 resolve)
        var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();

        // Playground를 기본 시작 화면으로 설정
        var initialViewModel = Services.GetRequiredService<PlaygroundViewModel>();

        mainWindowViewModel.SetInitialViewModel(initialViewModel);
        setDataContext(mainWindowViewModel);
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}