using System;
using NavigationKit.Avalonia.Abstractions;

namespace NavigationKit.Avalonia.Activation;

/// <summary>
/// Adapts <see cref="IServiceProvider"/> to <see cref="INavigationTypeActivator"/>.
/// </summary>
public sealed class ServiceProviderNavigationTypeActivator : ReflectionNavigationTypeActivator
{
    /// <summary>
    /// Initializes a new activator backed by the specified service provider.
    /// </summary>
    public ServiceProviderNavigationTypeActivator(IServiceProvider serviceProvider)
        : base(serviceProvider.GetService)
    {
    }
}