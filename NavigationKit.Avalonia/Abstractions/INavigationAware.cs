using System.Threading.Tasks;
using Avalonia.Controls;

namespace NavigationKit.Avalonia.Abstractions;

/// <summary>
/// Defines navigation lifecycle hooks for a page data context.
/// </summary>
public interface INavigationAware
{
    /// <summary>
    /// Called before the current page is navigated away from.
    /// </summary>
    /// <param name="args">Navigation event data.</param>
    ValueTask OnNavigatingAsync(NavigatingFromEventArgs args);

    /// <summary>
    /// Called after the page has been navigated away from.
    /// </summary>
    /// <param name="args">Navigation event data.</param>
    ValueTask OnNavigatedFromAsync(NavigatedFromEventArgs args);

    /// <summary>
    /// Called after the page has been navigated to.
    /// </summary>
    /// <param name="args">Navigation event data.</param>
    ValueTask OnNavigatedToAsync(NavigatedToEventArgs args);
}