using Avalonia.Controls;

namespace NavigationKit.Avalonia.Abstractions;

/// <summary>
/// Creates runtime navigation services for a specific <see cref="NavigationPage"/> host.
/// </summary>
public interface INavigationServiceFactory
{
    /// <summary>
    /// Creates a navigation service bound to the specified navigation host.
    /// </summary>
    INavigationService Create(NavigationPage navigationPage);
}