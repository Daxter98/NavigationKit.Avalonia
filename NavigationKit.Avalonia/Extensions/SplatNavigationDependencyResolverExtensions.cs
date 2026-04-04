using System;
using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Activation;
using NavigationKit.Avalonia.Services;
using Splat;

namespace NavigationKit.Avalonia.Extensions;

/// <summary>
/// Provides Splat registration helpers for the navigation library.
/// </summary>
public static class SplatNavigationDependencyResolverExtensions
{
    /// <summary>
    /// Registers the navigation library services and the specified navigation registry in a Splat resolver.
    /// </summary>
    public static IMutableDependencyResolver RegisterAvaloniaNavigation<TNavigationRegistry>(
        this IMutableDependencyResolver dependencyResolver)
        where TNavigationRegistry : class, INavigationViewResolver
    {
        ArgumentNullException.ThrowIfNull(dependencyResolver);

        // Build the registry eagerly so startup does not depend on nested lazy resolution inside Splat.
        var typeActivator = new SplatNavigationTypeActivator(Locator.Current);
        var registryActivator = new ReflectionNavigationTypeActivator(type =>
            type == typeof(TNavigationRegistry)
                ? null
                : Locator.Current.GetService(type, null));
        var viewResolver = registryActivator.Create<TNavigationRegistry>(typeActivator);
        var navigationServiceFactory = new NavigationServiceFactory(viewResolver);

        dependencyResolver.RegisterConstant<INavigationTypeActivator>(typeActivator);
        dependencyResolver.RegisterConstant(viewResolver);
        dependencyResolver.RegisterConstant<INavigationViewResolver>(viewResolver);
        dependencyResolver.RegisterConstant<INavigationServiceFactory>(navigationServiceFactory);

        return dependencyResolver;
    }
}