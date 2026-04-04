using System;

namespace NavigationKit.Avalonia.Abstractions;

/// <summary>
/// Creates view models, pages, and supporting navigation types.
/// </summary>
public interface INavigationTypeActivator
{
    /// <summary>
    /// Creates an instance of the requested type.
    /// </summary>
    object Create(Type type, params object?[] explicitArguments);

    /// <summary>
    /// Creates an instance of the requested type.
    /// </summary>
    T Create<T>(params object?[] explicitArguments);
}