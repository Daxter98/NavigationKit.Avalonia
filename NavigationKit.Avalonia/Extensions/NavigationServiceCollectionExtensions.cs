using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Activation;
using NavigationKit.Avalonia.Services;

namespace NavigationKit.Avalonia.Extensions;

/// <summary>
/// Provides Microsoft DI registration helpers for the navigation library.
/// </summary>
public static class NavigationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the navigation library services and the specified navigation registry in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddAvaloniaNavigation<TNavigationRegistry>(this IServiceCollection services)
        where TNavigationRegistry : class, INavigationViewResolver
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<INavigationTypeActivator>(static serviceProvider =>
            new ServiceProviderNavigationTypeActivator(serviceProvider));

        services.TryAddSingleton<TNavigationRegistry>(static serviceProvider =>
        {
            var typeActivator = serviceProvider.GetRequiredService<INavigationTypeActivator>();
            return typeActivator.Create<TNavigationRegistry>(typeActivator);
        });

        services.TryAddSingleton<INavigationViewResolver>(static serviceProvider =>
            serviceProvider.GetRequiredService<TNavigationRegistry>());

        services.TryAddSingleton<INavigationServiceFactory>(static serviceProvider =>
            new NavigationServiceFactory(serviceProvider.GetRequiredService<INavigationViewResolver>()));

        return services;
    }
}