using System;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1.Services;

/// <summary>
/// Simple service locator for accessing registered services
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("ServiceLocator has not been initialized. Call ServiceLocator.Initialize() first.");
        }

        return _serviceProvider.GetRequiredService<T>();
    }

    public static T? GetServiceOrNull<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }
}
