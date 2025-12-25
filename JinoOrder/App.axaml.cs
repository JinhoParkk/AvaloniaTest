using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using JinoOrder.Application.Common;
using JinoOrder.Extensions;
using JinoOrder.Infrastructure.Api.Refit;
using JinoOrder.Infrastructure.Services;
using JinoOrder.Presentation.Main;
using JinoOrder.Presentation.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace JinoOrder;

public partial class App : Avalonia.Application
{
    private IServiceProvider _services = null!;

    /// <summary>
    /// 플랫폼별 서비스 제공자 (Desktop/iOS/Android에서 설정)
    /// </summary>
    public static IPlatformServiceProvider? PlatformServices { get; set; }

    /// <summary>
    /// Desktop 플랫폼용 MainWindow (파일 다이얼로그 등에 필요)
    /// </summary>
    public static Window? MainWindow { get; private set; }

    /// <summary>
    /// 앱 시작 시 처리할 딥링크 URI (플랫폼에서 설정)
    /// </summary>
    public static string? PendingDeepLink { get; set; }

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

        // 2. 지노오더 서비스 등록
        services.AddJinoOrderServices();

        // 3. Refit API 클라이언트 서비스 등록 (Java Retrofit 스타일)
        services.AddRefitApiServices(options =>
        {
            options.BaseUrl = "https://api.passorder.com";
            options.RefreshTokenEndpoint = "/auth/refresh";
        });

        // 4. ViewModel 등록
        services.AddViewModels();

        // 5. 플랫폼별 서비스 등록 (IPlatformServiceProvider 사용)
        PlatformServices?.ConfigureServices(services);

        // ServiceProvider 생성
        _services = services.BuildServiceProvider();
    }

    /// <summary>
    /// 앱 초기화 및 시작 ViewModel 결정
    /// </summary>
    private void InitializeApp(Action<MainWindowViewModel> setDataContext)
    {
        // MainWindowViewModel 생성 (DI에서 resolve)
        var mainWindowViewModel = _services.GetRequiredService<MainWindowViewModel>();

        // 지노오더를 기본 시작 화면으로 설정
        var initialViewModel = _services.GetRequiredService<JinoOrderMainViewModel>();

        mainWindowViewModel.SetInitialViewModel(initialViewModel);
        setDataContext(mainWindowViewModel);

        // 대기 중인 딥링크가 있으면 처리
        if (!string.IsNullOrEmpty(PendingDeepLink))
        {
            HandleDeepLink(PendingDeepLink);
            PendingDeepLink = null;
        }
    }

    /// <summary>
    /// 딥링크 처리 (외부에서 호출 가능)
    /// </summary>
    /// <param name="uri">딥링크 URI</param>
    /// <returns>처리 성공 여부</returns>
    public static bool HandleDeepLink(string uri)
    {
        if (string.IsNullOrEmpty(uri) || Services == null)
            return false;

        var deepLinkService = Services.GetService<IDeepLinkService>();
        return deepLinkService?.Handle(uri) ?? false;
    }

    /// <summary>
    /// 딥링크 처리 (외부에서 호출 가능)
    /// </summary>
    /// <param name="uri">딥링크 URI</param>
    /// <returns>처리 성공 여부</returns>
    public static bool HandleDeepLink(Uri uri)
    {
        if (uri == null || Services == null)
            return false;

        var deepLinkService = Services.GetService<IDeepLinkService>();
        return deepLinkService?.Handle(uri) ?? false;
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
