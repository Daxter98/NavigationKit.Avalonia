using System;
using Avalonia.Controls;
using NavigationKit.Avalonia.Abstractions;

namespace NavigationKit.Avalonia.Services;

/// <summary>
/// Creates <see cref="NavigationService"/> instances bound to a specific navigation host.
/// </summary>
public sealed class NavigationServiceFactory : INavigationServiceFactory
{
    private readonly INavigationViewResolver _viewResolver;

    /// <summary>
    /// Initializes a new factory.
    /// </summary>
    public NavigationServiceFactory(INavigationViewResolver viewResolver)
    {
        ArgumentNullException.ThrowIfNull(viewResolver);
        _viewResolver = viewResolver;
    }

    /// <inheritdoc />
    public INavigationService Create(NavigationPage navigationPage)
    {
        return new NavigationService(navigationPage, _viewResolver);
    }
}