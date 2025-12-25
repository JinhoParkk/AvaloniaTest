using JinoOrder.Application.Customers;
using JinoOrder.Application.Menu;
using JinoOrder.Application.Orders;
using JinoOrder.Application.Statistics;
using JinoOrder.Infrastructure.Services;
using JinoOrder.Infrastructure.Storage;
using JinoOrder.Presentation.Auth;
using JinoOrder.Presentation.Main;
using JinoOrder.Presentation.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace JinoOrder.Extensions;

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
    /// 지노오더 서비스 등록
    /// </summary>
    public static IServiceCollection AddJinoOrderServices(this IServiceCollection services)
    {
        // Mock 서비스 (나중에 실제 API 서비스로 교체 가능)
        // 하나의 인스턴스가 모든 인터페이스를 구현
        services.AddSingleton<MockJinoOrderService>();
        services.AddSingleton<IOrderService>(sp => sp.GetRequiredService<MockJinoOrderService>());
        services.AddSingleton<IMenuService>(sp => sp.GetRequiredService<MockJinoOrderService>());
        services.AddSingleton<ICustomerService>(sp => sp.GetRequiredService<MockJinoOrderService>());
        services.AddSingleton<IStatisticsService>(sp => sp.GetRequiredService<MockJinoOrderService>());

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

        // 지노오더 ViewModels
        services.AddTransient<JinoOrderMainViewModel>();

        return services;
    }
}
