using System;
using NavigationKit.Avalonia.Abstractions;

namespace NavigationKit.Avalonia.Models;

/// <summary>
/// Provides navigation state information when <see cref="INavigationService.StateChanged"/> is raised.
/// </summary>
public sealed class NavigationStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationStateChangedEventArgs"/> class.
    /// </summary>
    public NavigationStateChangedEventArgs(string currentPageHeader, int navigationDepth, int modalDepth, string lastAction)
    {
        CurrentPageHeader = currentPageHeader;
        NavigationDepth = navigationDepth;
        ModalDepth = modalDepth;
        LastAction = lastAction;
    }

    /// <summary>
    /// Gets the current page header.
    /// </summary>
    public string CurrentPageHeader { get; }

    /// <summary>
    /// Gets the current navigation stack depth.
    /// </summary>
    public int NavigationDepth { get; }

    /// <summary>
    /// Gets the current modal stack depth.
    /// </summary>
    public int ModalDepth { get; }

    /// <summary>
    /// Gets the last navigation action description.
    /// </summary>
    public string LastAction { get; }
}