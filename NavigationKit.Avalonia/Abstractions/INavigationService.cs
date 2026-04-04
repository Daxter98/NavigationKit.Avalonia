using System;
using System.Threading.Tasks;
using NavigationKit.Avalonia.Models;

namespace NavigationKit.Avalonia.Abstractions;

/// <summary>
/// Defines the VM-first navigation API used by application code.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Occurs when the navigation state changes.
    /// </summary>
    event EventHandler<NavigationStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Gets a value indicating whether the navigation stack can go back.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets a value indicating whether a modal page is currently displayed.
    /// </summary>
    bool HasModal { get; }

    /// <summary>
    /// Gets the current navigation stack depth.
    /// </summary>
    int StackDepth { get; }

    /// <summary>
    /// Gets the current modal stack depth.
    /// </summary>
    int ModalDepth { get; }

    /// <summary>
    /// Pushes the page registered for the specified view model type onto the navigation stack.
    /// </summary>
    Task PushAsync<TViewModel>(object? parameter = null) where TViewModel : class;

    /// <summary>
    /// Pushes the page registered for the specified view model type onto the navigation stack.
    /// </summary>
    Task PushAsync(Type viewModelType, object? parameter = null);

    /// <summary>
    /// Pushes the page registered for the specified route onto the navigation stack.
    /// </summary>
    Task PushAsync(string route, object? parameter = null);

    /// <summary>
    /// Presents the page registered for the specified view model type as a modal.
    /// </summary>
    Task PushModalAsync<TViewModel>(object? parameter = null) where TViewModel : class;

    /// <summary>
    /// Presents the page registered for the specified view model type as a modal.
    /// </summary>
    Task PushModalAsync(Type viewModelType, object? parameter = null);

    /// <summary>
    /// Presents the page registered for the specified route as a modal.
    /// </summary>
    Task PushModalAsync(string route, object? parameter = null);

    /// <summary>
    /// Presents a modal and awaits a typed result from its data context.
    /// </summary>
    Task<TResult> PushModalForResultAsync<TViewModel, TResult>(object? parameter = null)
        where TViewModel : class;

    /// <summary>
    /// Presents a modal and awaits a typed result from its data context.
    /// </summary>
    Task<TResult> PushModalForResultAsync<TResult>(Type viewModelType, object? parameter = null);

    /// <summary>
    /// Presents a modal by route and awaits a typed result from its data context.
    /// </summary>
    Task<TResult> PushModalForResultAsync<TResult>(string route, object? parameter = null);

    /// <summary>
    /// Replaces the current non-modal page with the page registered for the specified view model type.
    /// </summary>
    Task ReplaceAsync<TViewModel>(object? parameter = null) where TViewModel : class;

    /// <summary>
    /// Replaces the current non-modal page with the page registered for the specified view model type.
    /// </summary>
    Task ReplaceAsync(Type viewModelType, object? parameter = null);

    /// <summary>
    /// Replaces the current non-modal page with the page registered for the specified route.
    /// </summary>
    Task ReplaceAsync(string route, object? parameter = null);

    /// <summary>
    /// Navigates back one page in the non-modal stack.
    /// </summary>
    Task GoBackAsync();

    /// <summary>
    /// Dismisses the top-most modal page.
    /// </summary>
    Task DismissModalAsync();

    /// <summary>
    /// Dismisses all modal pages.
    /// </summary>
    Task DismissAllModalsAsync();

    /// <summary>
    /// Pops the non-modal stack back to its root page.
    /// </summary>
    Task PopToRootAsync();
}