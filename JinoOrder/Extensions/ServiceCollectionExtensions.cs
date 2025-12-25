using JinoOrder.Application.Common;
using JinoOrder.Application.Customers;
using JinoOrder.Application.Menu;
using JinoOrder.Application.Orders;
using JinoOrder.Application.Statistics;
using JinoOrder.Infrastructure.Services;
using JinoOrder.Infrastructure.Storage;
using JinoOrder.Presentation.Auth;
using JinoOrder.Presentation.Customers;
using JinoOrder.Presentation.Main;
using JinoOrder.Presentation.Menu;
using JinoOrder.Presentation.Orders;
using JinoOrder.Presentation.Settings;
using JinoOrder.Presentation.Shell;
using JinoOrder.Presentation.Statistics;
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
        // ViewModelFactory (NavigationService에서 사용)
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();

        // 네비게이션 및 환경설정
        services.AddSingleton<NavigationService>();
        services.AddSingleton<PreferencesService>();

        // 딥링크 서비스
        services.AddSingleton<DeepLinkService>();
        services.AddSingleton<IDeepLinkService>(sp => sp.GetRequiredService<DeepLinkService>());

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
        services.AddTransient<MainWindowViewModel>();

        // 분리된 자식 ViewModels
        services.AddTransient<OrdersViewModel>(sp =>
        {
            var orderService = sp.GetRequiredService<IOrderService>();
            // StoreState에서 MinPickupTime을 가져오기 위한 콜백
            // JinoOrderMainViewModel이 조합할 때 연결됨
            return new OrdersViewModel(orderService);
        });
        services.AddTransient<MenuManagementViewModel>();
        services.AddTransient<CustomersViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<StoreStateViewModel>();
        services.AddTransient<SettingsViewModel>(sp =>
        {
            var preferencesService = sp.GetRequiredService<PreferencesService>();
            return new SettingsViewModel(preferencesService);
        });

        // 지노오더 메인 ViewModel (자식 VM들을 조합)
        services.AddTransient<JinoOrderMainViewModel>(sp =>
        {
            var orders = sp.GetRequiredService<OrdersViewModel>();
            var menu = sp.GetRequiredService<MenuManagementViewModel>();
            var customers = sp.GetRequiredService<CustomersViewModel>();
            var statistics = sp.GetRequiredService<StatisticsViewModel>();
            var storeState = sp.GetRequiredService<StoreStateViewModel>();
            var settings = sp.GetRequiredService<SettingsViewModel>();
            var platformInfo = sp.GetService<IPlatformInfo>();

            return new JinoOrderMainViewModel(
                orders, menu, customers, statistics, storeState, settings, platformInfo);
        });

        return services;
    }
}
