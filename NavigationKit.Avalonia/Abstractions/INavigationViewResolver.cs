using System;
using Avalonia.Controls;

namespace NavigationKit.Avalonia.Abstractions;

/// <summary>
/// Resolves a page instance from a view model type or route.
/// </summary>
public interface INavigationViewResolver
{
    /// <summary>
    /// Creates the page registered for the specified view model type.
    /// </summary>
    Page CreatePage(Type viewModelType, INavigationService navigationService, object? parameter = null);

    /// <summary>
    /// Creates the page registered for the specified route.
    /// </summary>
    Page CreatePage(string route, INavigationService navigationService, object? parameter = null);
}