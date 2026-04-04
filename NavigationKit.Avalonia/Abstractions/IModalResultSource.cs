using System.Threading.Tasks;

namespace NavigationKit.Avalonia.Abstractions;

/// <summary>
/// Defines the non-generic contract used by the navigation service to cancel a pending modal result.
/// </summary>
public interface IModalResultSource
{
    /// <summary>
    /// Cancels the pending modal result.
    /// </summary>
    void CancelResult();
}

/// <summary>
/// Exposes the result task produced by a modal workflow.
/// </summary>
/// <typeparam name="TResult">The result type returned by the modal.</typeparam>
public interface IModalResultSource<TResult> : IModalResultSource
{
    /// <summary>
    /// Gets the task that completes when the modal publishes its result.
    /// </summary>
    Task<TResult> ResultTask { get; }
}