using NavigationKit.Avalonia.Abstractions;
using Splat;

namespace NavigationKit.Avalonia.Activation;

/// <summary>
/// Adapts Splat's dependency resolver to <see cref="INavigationTypeActivator"/>.
/// </summary>
public sealed class SplatNavigationTypeActivator : ReflectionNavigationTypeActivator
{
    /// <summary>
    /// Initializes a new activator backed by the specified Splat resolver.
    /// </summary>
    public SplatNavigationTypeActivator(IReadonlyDependencyResolver dependencyResolver)
        : base(type => dependencyResolver.GetService(type, null))
    {
    }
}