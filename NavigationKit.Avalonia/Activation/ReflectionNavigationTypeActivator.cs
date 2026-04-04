using System;
using System.Collections.Generic;
using System.Linq;
using NavigationKit.Avalonia.Abstractions;

namespace NavigationKit.Avalonia.Activation;

/// <summary>
/// Creates navigation types by matching explicit arguments first, then optional service resolution,
/// and finally recursive concrete-type construction.
/// </summary>
public class ReflectionNavigationTypeActivator : INavigationTypeActivator
{
    private readonly Func<Type, object?>? _serviceResolver;

    /// <summary>
    /// Initializes a new activator.
    /// </summary>
    public ReflectionNavigationTypeActivator(Func<Type, object?>? serviceResolver = null)
    {
        _serviceResolver = serviceResolver;
    }

    /// <inheritdoc />
    public object Create(Type type, params object?[] explicitArguments)
    {
        ArgumentNullException.ThrowIfNull(type);
        return Create(type, explicitArguments, new HashSet<Type>(), allowDirectServiceResolution: explicitArguments.Length == 0);
    }

    /// <inheritdoc />
    public T Create<T>(params object?[] explicitArguments)
    {
        return (T)Create(typeof(T), explicitArguments);
    }

    /// <summary>
    /// Walks the constructor graph while guarding against circular activation.
    /// </summary>
    private object Create(
        Type type,
        object?[] explicitArguments,
        ISet<Type> activationPath,
        bool allowDirectServiceResolution)
    {
        if (allowDirectServiceResolution && TryResolveService(type, out var service))
            return service;

        if (type.IsAbstract || type.IsInterface)
            throw new InvalidOperationException($"Unable to create abstract navigation type '{type.Name}'.");

        if (!activationPath.Add(type))
            throw new InvalidOperationException(
                $"Detected a circular navigation activation dependency for '{type.Name}'.");

        try
        {
            var constructors = type
                .GetConstructors()
                .OrderByDescending(static ctor => ctor.GetParameters().Length)
                .ToArray();

            // Prefer the most specific constructor that can be satisfied by explicit arguments,
            // the configured service resolver, or recursive concrete activation.
            foreach (var constructor in constructors)
            {
                if (TryBuildArguments(constructor.GetParameters(), explicitArguments, activationPath,
                        out var arguments))
                    return constructor.Invoke(arguments);
            }

            throw new InvalidOperationException(
                $"Unable to create navigation type '{type.Name}'. No suitable constructor was found.");
        }
        finally
        {
            activationPath.Remove(type);
        }
    }

    /// <summary>
    /// Attempts to build a constructor argument list from explicit values, registered services,
    /// default values, and recursive concrete activation.
    /// </summary>
    private bool TryBuildArguments(
        System.Reflection.ParameterInfo[] parameters,
        object?[] explicitArguments,
        ISet<Type> activationPath,
        out object?[] arguments)
    {
        arguments = new object?[parameters.Length];
        var explicitArgumentUsage = new bool[explicitArguments.Length];

        for (var index = 0; index < parameters.Length; index++)
        {
            var parameter = parameters[index];

            if (TryTakeExplicitArgument(parameter.ParameterType, explicitArguments, explicitArgumentUsage,
                    out var explicitArgument))
            {
                arguments[index] = explicitArgument;
                continue;
            }

            if (TryResolveService(parameter.ParameterType, out var service))
            {
                arguments[index] = service;
                continue;
            }

            if (parameter.HasDefaultValue)
            {
                arguments[index] = parameter.DefaultValue;
                continue;
            }

            if (!parameter.ParameterType.IsAbstract && !parameter.ParameterType.IsInterface)
            {
                arguments[index] = Create(
                    parameter.ParameterType,
                    Array.Empty<object?>(),
                    activationPath,
                    allowDirectServiceResolution: true);
                continue;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Consumes a compatible explicit argument if one is available.
    /// </summary>
    private static bool TryTakeExplicitArgument(
        Type targetType,
        object?[] explicitArguments,
        bool[] usage,
        out object? argument)
    {
        for (var index = 0; index < explicitArguments.Length; index++)
        {
            if (usage[index])
                continue;

            var candidate = explicitArguments[index];
            if (!IsCompatible(targetType, candidate))
                continue;

            usage[index] = true;
            argument = candidate;
            return true;
        }

        argument = null;
        return false;
    }

    /// <summary>
    /// Attempts to resolve a service from the configured service resolver.
    /// </summary>
    private bool TryResolveService(Type type, out object service)
    {
        service = null!;

        if (_serviceResolver is null)
        {
            return false;
        }

        var resolved = _serviceResolver(type);

        if (resolved is null)
        {
            return false;
        }

        if (!IsCompatible(type, resolved))
        {
            return false;
        }

        service = resolved;
        
        return true;
    }

    /// <summary>
    /// Determines whether a value can be assigned to the target type.
    /// </summary>
    private static bool IsCompatible(Type targetType, object? value)
    {
        if (value is null)
            return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null;

        return targetType.IsInstanceOfType(value);
    }
}