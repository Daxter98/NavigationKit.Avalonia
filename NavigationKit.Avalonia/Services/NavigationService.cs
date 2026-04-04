using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Models;

namespace NavigationKit.Avalonia.Services;

/// <summary>
/// Implements serialized navigation operations over an Avalonia <see cref="NavigationPage"/>.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly NavigationPage _navigationPage;
    private readonly INavigationViewResolver _viewResolver;
    private readonly SemaphoreSlim _navigationGate = new(1, 1);
    private readonly Dictionary<Page, IModalResultSource> _pendingModalResults = new();

    /// <summary>
    /// Initializes a new navigation service for the specified host page and resolver.
    /// </summary>
    public NavigationService(NavigationPage navigationPage, INavigationViewResolver viewResolver)
    {
        ArgumentNullException.ThrowIfNull(navigationPage);
        ArgumentNullException.ThrowIfNull(viewResolver);

        _navigationPage = navigationPage;
        _viewResolver = viewResolver;

        _navigationPage.Pushed += (_, e) => PublishState($"Pushed {e.Page?.Header}");
        _navigationPage.Popped += (_, e) => PublishState($"Popped {e.Page?.Header}");
        _navigationPage.PoppedToRoot += (_, _) => PublishState("Popped to root");
        _navigationPage.ModalPushed += (_, e) => PublishState($"Presented modal {e.Modal?.Header}");
        _navigationPage.ModalPopped += (_, e) =>
        {
            // A modal result must be canceled if the user or host dismisses it outside the awaited result path.
            CancelPendingModalResult(e.Modal);
            PublishState($"Dismissed modal {e.Modal?.Header}");
        };
    }

    /// <inheritdoc />
    public event EventHandler<NavigationStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public bool CanGoBack => _navigationPage.CanGoBack;

    /// <inheritdoc />
    public bool HasModal => _navigationPage.ModalStack.Count > 0;

    /// <inheritdoc />
    public int StackDepth => _navigationPage.NavigationStack.Count;

    /// <inheritdoc />
    public int ModalDepth => _navigationPage.ModalStack.Count;

    /// <inheritdoc />
    public Task PushAsync<TViewModel>(object? parameter = null) where TViewModel : class
    {
        return PushAsync(typeof(TViewModel), parameter);
    }

    /// <inheritdoc />
    public async Task PushAsync(Type viewModelType, object? parameter = null)
    {
        await _navigationGate.WaitAsync();

        try
        {
            var page = PreparePage(_viewResolver.CreatePage(viewModelType, this, parameter));

            // The first non-modal navigation initializes the host page instead of pushing onto an empty stack.
            if (IsRootNavigationRequired())
            {
                await _navigationPage.ReplaceAsync(page);
                PublishState($"Initialized {page.Header}");
                return;
            }

            await _navigationPage.PushAsync(page);
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task PushAsync(string route, object? parameter = null)
    {
        await _navigationGate.WaitAsync();

        try
        {
            var page = PreparePage(_viewResolver.CreatePage(route, this, parameter));

            if (IsRootNavigationRequired())
            {
                await _navigationPage.ReplaceAsync(page);
                PublishState($"Initialized {page.Header}");
                return;
            }

            await _navigationPage.PushAsync(page);
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public Task PushModalAsync<TViewModel>(object? parameter = null) where TViewModel : class
    {
        return PushModalAsync(typeof(TViewModel), parameter);
    }

    /// <inheritdoc />
    public async Task PushModalAsync(Type viewModelType, object? parameter = null)
    {
        await _navigationGate.WaitAsync();

        try
        {
            var page = PreparePage(_viewResolver.CreatePage(viewModelType, this, parameter));
            await _navigationPage.PushModalAsync(page);
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task PushModalAsync(string route, object? parameter = null)
    {
        await _navigationGate.WaitAsync();

        try
        {
            var page = PreparePage(_viewResolver.CreatePage(route, this, parameter));
            await _navigationPage.PushModalAsync(page);
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public Task<TResult> PushModalForResultAsync<TViewModel, TResult>(object? parameter = null)
        where TViewModel : class
    {
        return PushModalForResultAsync<TResult>(typeof(TViewModel), parameter);
    }

    /// <inheritdoc />
    public async Task<TResult> PushModalForResultAsync<TResult>(Type viewModelType, object? parameter = null)
    {
        var page = await PushModalForResultCoreAsync(viewModelType, parameter);
        return await AwaitModalResultAsync<TResult>(page);
    }

    /// <inheritdoc />
    public async Task<TResult> PushModalForResultAsync<TResult>(string route, object? parameter = null)
    {
        var page = await PushModalForResultCoreAsync(route, parameter);
        return await AwaitModalResultAsync<TResult>(page);
    }

    /// <inheritdoc />
    public Task ReplaceAsync<TViewModel>(object? parameter = null) where TViewModel : class
    {
        return ReplaceAsync(typeof(TViewModel), parameter);
    }

    /// <inheritdoc />
    public async Task ReplaceAsync(Type viewModelType, object? parameter = null)
    {
        await _navigationGate.WaitAsync();

        try
        {
            var page = PreparePage(_viewResolver.CreatePage(viewModelType, this, parameter));
            await _navigationPage.ReplaceAsync(page);
            PublishState($"Replaced with {page.Header}");
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task ReplaceAsync(string route, object? parameter = null)
    {
        await _navigationGate.WaitAsync();

        try
        {
            var page = PreparePage(_viewResolver.CreatePage(route, this, parameter));
            await _navigationPage.ReplaceAsync(page);
            PublishState($"Replaced with {page.Header}");
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task GoBackAsync()
    {
        await _navigationGate.WaitAsync();

        try
        {
            if (_navigationPage.NavigationStack.Count <= 1)
            {
                PublishState("Already at root");
                return;
            }

            await _navigationPage.PopAsync();
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task DismissModalAsync()
    {
        await _navigationGate.WaitAsync();

        try
        {
            if (_navigationPage.ModalStack.Count == 0)
            {
                PublishState("No modal to dismiss");
                return;
            }

            CancelPendingModalResult(_navigationPage.ModalStack[_navigationPage.ModalStack.Count - 1]);
            await _navigationPage.PopModalAsync();
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task DismissAllModalsAsync()
    {
        await _navigationGate.WaitAsync();

        try
        {
            if (_navigationPage.ModalStack.Count == 0)
            {
                PublishState("No modals to dismiss");
                return;
            }

            foreach (var page in _navigationPage.ModalStack)
                CancelPendingModalResult(page);

            await _navigationPage.PopAllModalsAsync();
            PublishState("Dismissed all modals");
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task PopToRootAsync()
    {
        await _navigationGate.WaitAsync();

        try
        {
            if (_navigationPage.NavigationStack.Count <= 1)
            {
                PublishState("Already at root");
                return;
            }

            await _navigationPage.PopToRootAsync();
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <summary>
    /// Attaches navigation lifecycle forwarding to a page before it is displayed.
    /// </summary>
    private Page PreparePage(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (page.DataContext is INavigationAware navigationAware)
        {
            page.Navigating += args => navigationAware.OnNavigatingAsync(args).AsTask();
            page.NavigatedFrom += (_, args) => navigationAware.OnNavigatedFromAsync(args);
            page.NavigatedTo += (_, args) => navigationAware.OnNavigatedToAsync(args);
        }

        return page;
    }

    /// <summary>
    /// Pushes a modal that is expected to publish a result.
    /// </summary>
    private async Task<Page> PushModalForResultCoreAsync(Type viewModelType, object? parameter)
    {
        await _navigationGate.WaitAsync();

        try
        {
            var page = PreparePage(_viewResolver.CreatePage(viewModelType, this, parameter));
            EnsureModalResultSource(page);
            await _navigationPage.PushModalAsync(page);
            return page;
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <summary>
    /// Pushes a modal route that is expected to publish a result.
    /// </summary>
    private async Task<Page> PushModalForResultCoreAsync(string route, object? parameter)
    {
        await _navigationGate.WaitAsync();

        try
        {
            var page = PreparePage(_viewResolver.CreatePage(route, this, parameter));
            EnsureModalResultSource(page);
            await _navigationPage.PushModalAsync(page);
            return page;
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <summary>
    /// Waits for a modal result and dismisses the modal if it is still present when the result completes.
    /// </summary>
    private async Task<TResult> AwaitModalResultAsync<TResult>(Page page)
    {
        var resultSource = GetTrackedModalResultSource<TResult>(page);

        try
        {
            return await resultSource.ResultTask;
        }
        finally
        {
            await DismissModalPageIfPresentAsync(page);
            ClearPendingModalResult(page);
        }
    }

    /// <summary>
    /// Verifies that the page data context supports modal-result completion and tracks it by page.
    /// </summary>
    private void EnsureModalResultSource(Page page)
    {
        if (page.DataContext is not IModalResultSource resultSource)
        {
            throw new InvalidOperationException(
                $"Modal page '{page.GetType().Name}' requires a DataContext that implements IModalResultSource<TResult> when using PushModalForResultAsync.");
        }

        _pendingModalResults[page] = resultSource;
    }

    /// <summary>
    /// Retrieves the typed modal result source from the tracked page mapping, falling back to the current data context.
    /// </summary>
    private IModalResultSource<TResult> GetTrackedModalResultSource<TResult>(Page page)
    {
        if (_pendingModalResults.TryGetValue(page, out var resultSource) &&
            resultSource is IModalResultSource<TResult> trackedResultSource)
        {
            return trackedResultSource;
        }

        if (page.DataContext is IModalResultSource<TResult> fallbackResultSource)
        {
            return fallbackResultSource;
        }

        throw new InvalidOperationException(
            $"Modal page '{page.GetType().Name}' DataContext must implement IModalResultSource<{typeof(TResult).Name}>.");
    }

    /// <summary>
    /// Dismisses a modal page if it is still present in the modal stack.
    /// </summary>
    private async Task DismissModalPageIfPresentAsync(Page page)
    {
        await _navigationGate.WaitAsync();

        try
        {
            if (!IsModalPagePresent(page))
                return;

            ClearPendingModalResult(page);
            await _navigationPage.PopModalAsync();
        }
        finally
        {
            _navigationGate.Release();
        }
    }

    /// <summary>
    /// Cancels and removes a pending modal result associated with the specified page.
    /// </summary>
    private void CancelPendingModalResult(Page? page)
    {
        if (page is null)
            return;

        if (_pendingModalResults.Remove(page, out var resultSource))
            resultSource.CancelResult();
    }

    /// <summary>
    /// Removes any tracked modal result source for the specified page without canceling it.
    /// </summary>
    private void ClearPendingModalResult(Page page)
    {
        _pendingModalResults.Remove(page);
    }

    /// <summary>
    /// Determines whether the specified page is still present in the modal stack.
    /// </summary>
    private bool IsModalPagePresent(Page page)
    {
        foreach (var modalPage in _navigationPage.ModalStack)
        {
            if (ReferenceEquals(modalPage, page))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the next non-modal navigation should initialize the root page.
    /// </summary>
    private bool IsRootNavigationRequired()
    {
        return _navigationPage.CurrentPage is null && _navigationPage.NavigationStack.Count == 0;
    }

    /// <summary>
    /// Publishes the current navigation state to observers.
    /// </summary>
    private void PublishState(string action)
    {
        var header = _navigationPage.CurrentPage?.Header?.ToString() ?? "(none)";
        StateChanged?.Invoke(this, new NavigationStateChangedEventArgs(
            header,
            _navigationPage.NavigationStack.Count,
            _navigationPage.ModalStack.Count,
            action));
    }
}