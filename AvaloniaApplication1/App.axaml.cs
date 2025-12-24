using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;
using AvaloniaApplication1.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Create main window first
            desktop.MainWindow = new MainWindow();

            // Initialize services with DI
            InitializeDesktopServices(desktop.MainWindow);

            // 서비스 초기화
            var authService = new AuthenticationService();
            var preferencesService = new PreferencesService();
            var navigationService = new NavigationService();

            // 자동 로그인 확인
            var savedUser = preferencesService.LoadAutoLogin();
            ViewModelBase initialViewModel;

            if (savedUser != null)
            {
                // 자동 로그인 정보가 있으면 바로 메인으로
                authService.SetCurrentUser(savedUser);
                initialViewModel = new MainViewModel();
            }
            else
            {
                // 로그인 정보가 없으면 로그인 화면으로
                initialViewModel = new LoginViewModel(authService, preferencesService, navigationService);
            }

            desktop.MainWindow.DataContext = new MainWindowViewModel(navigationService, initialViewModel);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Initialize services for mobile platforms
            InitializeMobileServices();

            // 서비스 초기화
            var authService = new AuthenticationService();
            var preferencesService = new PreferencesService();
            var navigationService = new NavigationService();

            // 자동 로그인 확인
            var savedUser = preferencesService.LoadAutoLogin();
            ViewModelBase initialViewModel;

            if (savedUser != null)
            {
                authService.SetCurrentUser(savedUser);
                initialViewModel = new MainViewModel();
            }
            else
            {
                initialViewModel = new LoginViewModel(authService, preferencesService, navigationService);
            }

            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainWindowViewModel(navigationService, initialViewModel)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeDesktopServices(Window mainWindow)
    {
        var services = new ServiceCollection();

        // Register existing services
        services.AddSingleton<AuthenticationService>();
        services.AddSingleton<PreferencesService>();
        services.AddSingleton<NavigationService>();

        // Register platform services using reflection to avoid direct dependency
        // This allows the shared project to remain platform-agnostic
        var desktopServicesType = Type.GetType("AvaloniaApplication1.Desktop.Services.DesktopServiceExtensions, AvaloniaApplication1.Desktop");
        if (desktopServicesType != null)
        {
            var method = desktopServicesType.GetMethod("AddDesktopServices");
            method?.Invoke(null, new object[] { services, mainWindow });
        }

        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.Initialize(serviceProvider);
    }

    private void InitializeMobileServices()
    {
        var services = new ServiceCollection();

        // Register existing services
        services.AddSingleton<AuthenticationService>();
        services.AddSingleton<PreferencesService>();
        services.AddSingleton<NavigationService>();

        // Detect platform and register appropriate services
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")))
        {
            var androidServicesType = Type.GetType("AvaloniaApplication1.Android.Services.AndroidServiceExtensions, AvaloniaApplication1.Android");
            if (androidServicesType != null)
            {
                var method = androidServicesType.GetMethod("AddAndroidServices");
                method?.Invoke(null, new object[] { services });
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS")))
        {
            var iosServicesType = Type.GetType("AvaloniaApplication1.iOS.Services.iOSServiceExtensions, AvaloniaApplication1.iOS");
            if (iosServicesType != null)
            {
                var method = iosServicesType.GetMethod("AddiOSServices");
                method?.Invoke(null, new object[] { services });
            }
        }

        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.Initialize(serviceProvider);
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}