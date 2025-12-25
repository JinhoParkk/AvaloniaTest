using AvaloniaApplication1.Services;
using AvaloniaApplication1.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.Extensions;

/// <summary>
/// 서비스 등록 확장 메서드 (모듈화된 DI 구성)
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 핵심 앱 서비스 등록
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // 네비게이션 및 환경설정
        services.AddSingleton<NavigationService>();
        services.AddSingleton<PreferencesService>();

        return services;
    }

    /// <summary>
    /// ViewModel 등록 (생성자 주입 지원)
    /// </summary>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // Transient: 매번 새 인스턴스 생성
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PlaygroundViewModel>();

        return services;
    }
}
