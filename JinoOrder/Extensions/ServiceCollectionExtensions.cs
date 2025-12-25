using JinoOrder.Application.Common;
using JinoOrder.Application.Customers;
using JinoOrder.Application.Menu;
using JinoOrder.Application.Orders;
using JinoOrder.Application.Statistics;
using JinoOrder.Infrastructure.Logging;
using JinoOrder.Infrastructure.Services;
using JinoOrder.Infrastructure.Services.Mock;
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
using Microsoft.Extensions.Logging;

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
        // 로깅 서비스 등록
        services.AddSerilogLogging();

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
    /// 지노오더 서비스 등록 (Mock 서비스)
    /// </summary>
    public static IServiceCollection AddJinoOrderServices(this IServiceCollection services)
    {
        // 공유 데이터 저장소 (싱글톤)
        services.AddSingleton<MockDataStore>();

        // 각 서비스별 Mock 구현 (분리된 책임)
        services.AddSingleton<IOrderService, MockOrderService>();
        services.AddSingleton<IMenuService, MockMenuService>();
        services.AddSingleton<ICustomerService, MockCustomerService>();
        services.AddSingleton<IStatisticsService, MockStatisticsService>();

        return services;
    }

    /// <summary>
    /// ViewModel 등록 (생성자 주입 지원)
    /// </summary>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // Transient: 매번 새 인스턴스 생성
        services.AddTransient<LoginViewModel>(sp =>
        {
            var authService = sp.GetRequiredService<Application.Auth.IAuthenticationService>();
            var preferencesService = sp.GetRequiredService<PreferencesService>();
            var navigationService = sp.GetRequiredService<NavigationService>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<LoginViewModel>();
            return new LoginViewModel(authService, preferencesService, navigationService, logger);
        });
        services.AddTransient<MainWindowViewModel>();

        // 분리된 자식 ViewModels
        services.AddTransient<OrdersViewModel>(sp =>
        {
            var orderService = sp.GetRequiredService<IOrderService>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<OrdersViewModel>();
            return new OrdersViewModel(orderService, logger);
        });
        services.AddTransient<MenuManagementViewModel>(sp =>
        {
            var menuService = sp.GetRequiredService<IMenuService>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<MenuManagementViewModel>();
            return new MenuManagementViewModel(menuService, logger);
        });
        services.AddTransient<CustomersViewModel>(sp =>
        {
            var customerService = sp.GetRequiredService<ICustomerService>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<CustomersViewModel>();
            return new CustomersViewModel(customerService, logger);
        });
        services.AddTransient<StatisticsViewModel>(sp =>
        {
            var statisticsService = sp.GetRequiredService<IStatisticsService>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<StatisticsViewModel>();
            return new StatisticsViewModel(statisticsService, logger);
        });
        services.AddTransient<StoreStateViewModel>();
        services.AddTransient<SettingsViewModel>(sp =>
        {
            var preferencesService = sp.GetRequiredService<PreferencesService>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<SettingsViewModel>();
            return new SettingsViewModel(preferencesService, logger);
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
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<JinoOrderMainViewModel>();

            return new JinoOrderMainViewModel(
                orders, menu, customers, statistics, storeState, settings, platformInfo, logger);
        });

        return services;
    }
}
