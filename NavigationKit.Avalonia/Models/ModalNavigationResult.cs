using System;
using System.Threading.Tasks;
using NavigationKit.Avalonia.Abstractions;

namespace NavigationKit.Avalonia.Models;

/// <summary>
/// Provides a reusable implementation of <see cref="IModalResultSource{TResult}"/> backed by a task completion source.
/// </summary>
/// <typeparam name="TResult">The result type returned by the modal.</typeparam>
public sealed class ModalNavigationResult<TResult> : IModalResultSource<TResult>
{
    private readonly TaskCompletionSource<TResult> _resultSource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <inheritdoc />
    public Task<TResult> ResultTask => _resultSource.Task;

    /// <summary>
    /// Completes the modal result successfully.
    /// </summary>
    public bool SetResult(TResult result)
    {
        return _resultSource.TrySetResult(result);
    }

    /// <summary>
    /// Cancels the modal result.
    /// </summary>
    public bool Cancel()
    {
        return _resultSource.TrySetCanceled();
    }

    /// <summary>
    /// Completes the modal result with an exception.
    /// </summary>
    public bool SetException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return _resultSource.TrySetException(exception);
    }

    /// <inheritdoc />
    void IModalResultSource.CancelResult()
    {
        _resultSource.TrySetCanceled();
    }
}