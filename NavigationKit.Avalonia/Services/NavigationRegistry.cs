using System;
using System.Collections.Generic;
using Avalonia.Controls;
using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Activation;

namespace NavigationKit.Avalonia.Services;

/// <summary>
/// Stores ViewModel-to-Page registrations and resolves pages for navigation requests.
/// </summary>
public class NavigationRegistry : INavigationViewResolver
{
    private readonly INavigationTypeActivator _typeActivator;
    private readonly Dictionary<Type, NavigationRegistration> _viewModelRegistrations = new();
    private readonly Dictionary<string, NavigationRegistration> _routeRegistrations =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new registry.
    /// </summary>
    public NavigationRegistry(INavigationTypeActivator? typeActivator = null)
    {
        _typeActivator = typeActivator ?? new ReflectionNavigationTypeActivator();
    }

    /// <summary>
    /// Registers a view model/page pair that can be navigated to by view model type.
    /// </summary>
    public NavigationRegistry Register<TViewModel, TPage>(Action<TPage, TViewModel>? bind = null)
        where TViewModel : class
        where TPage : Page
    {
        Action<Page, TViewModel>? pageBind = bind is null ? null : (page, viewModel) => bind((TPage)page, viewModel);

        return Register(
            static (navigationService, activator, _) => activator.Create<TViewModel>(navigationService),
            static (navigationService, activator, viewModel) => activator.Create<TPage>(viewModel),
            pageBind);
    }

    /// <summary>
    /// Registers a parameterized view model/page pair that can be navigated to by view model type.
    /// </summary>
    public NavigationRegistry Register<TViewModel, TParameter, TPage>(Action<TPage, TViewModel>? bind = null)
        where TViewModel : class
        where TPage : Page
    {
        Action<Page, TViewModel>? pageBind = bind is null ? null : (page, viewModel) => bind((TPage)page, viewModel);

        return Register(
            static (navigationService, activator, parameter) =>
                activator.Create<TViewModel>(navigationService, ResolveParameter<TParameter>(typeof(TViewModel).Name, parameter)),
            static (navigationService, activator, viewModel) => activator.Create<TPage>(viewModel),
            pageBind);
    }

    /// <summary>
    /// Registers a view model/page pair using a custom view model factory.
    /// </summary>
    public NavigationRegistry Register<TViewModel, TPage>(
        Func<INavigationService, TViewModel> createViewModel,
        Action<TPage, TViewModel>? bind = null)
        where TViewModel : class
        where TPage : Page
    {
        Action<Page, TViewModel>? pageBind = bind is null ? null : (page, viewModel) => bind((TPage)page, viewModel);

        return Register(
            (navigationService, _, _) => createViewModel(navigationService),
            static (navigationService, activator, viewModel) => activator.Create<TPage>(viewModel),
            pageBind);
    }

    /// <summary>
    /// Registers a parameterized view model/page pair using a custom view model factory.
    /// </summary>
    public NavigationRegistry Register<TViewModel, TParameter, TPage>(
        Func<INavigationService, TParameter, TViewModel> createViewModel,
        Action<TPage, TViewModel>? bind = null)
        where TViewModel : class
        where TPage : Page
    {
        Action<Page, TViewModel>? pageBind = bind is null ? null : (page, viewModel) => bind((TPage)page, viewModel);

        return Register(
            (navigationService, _, parameter) =>
                createViewModel(navigationService, ResolveParameter<TParameter>(typeof(TViewModel).Name, parameter)),
            static (navigationService, activator, viewModel) => activator.Create<TPage>(viewModel),
            pageBind);
    }

    /// <summary>
    /// Registers a target using custom factories for both the view model and page.
    /// </summary>
    public NavigationRegistry Register<TViewModel>(
        Func<INavigationService, TViewModel> createViewModel,
        Func<INavigationService, TViewModel, Page> createPage)
        where TViewModel : class
    {
        return Register(
            (navigationService, _, _) => createViewModel(navigationService),
            (navigationService, _, viewModel) => createPage(navigationService, viewModel));
    }

    /// <summary>
    /// Registers a parameterized target using custom factories for both the view model and page.
    /// </summary>
    public NavigationRegistry Register<TViewModel, TParameter>(
        Func<INavigationService, TParameter, TViewModel> createViewModel,
        Func<INavigationService, TViewModel, Page> createPage)
        where TViewModel : class
    {
        return Register(
            (navigationService, _, parameter) =>
                createViewModel(navigationService, ResolveParameter<TParameter>(typeof(TViewModel).Name, parameter)),
            (navigationService, _, viewModel) => createPage(navigationService, viewModel));
    }

    /// <summary>
    /// Registers a route name for a view model/page pair.
    /// </summary>
    public NavigationRegistry RegisterRoute<TViewModel, TPage>(string route, Action<TPage, TViewModel>? bind = null)
        where TViewModel : class
        where TPage : Page
    {
        Action<Page, TViewModel>? pageBind = bind is null ? null : (page, viewModel) => bind((TPage)page, viewModel);

        return RegisterRoute(
            route,
            static (navigationService, activator, _) => activator.Create<TViewModel>(navigationService),
            static (navigationService, activator, viewModel) => activator.Create<TPage>(viewModel),
            pageBind);
    }

    /// <summary>
    /// Registers a route name for a parameterized view model/page pair.
    /// </summary>
    public NavigationRegistry RegisterRoute<TViewModel, TParameter, TPage>(string route, Action<TPage, TViewModel>? bind = null)
        where TViewModel : class
        where TPage : Page
    {
        Action<Page, TViewModel>? pageBind = bind is null ? null : (page, viewModel) => bind((TPage)page, viewModel);

        return RegisterRoute(
            route,
            static (navigationService, activator, parameter) =>
                activator.Create<TViewModel>(navigationService, ResolveParameter<TParameter>(typeof(TViewModel).Name, parameter)),
            static (navigationService, activator, viewModel) => activator.Create<TPage>(viewModel),
            pageBind);
    }

    /// <summary>
    /// Registers a route name using a custom view model factory.
    /// </summary>
    public NavigationRegistry RegisterRoute<TViewModel, TPage>(
        string route,
        Func<INavigationService, TViewModel> createViewModel,
        Action<TPage, TViewModel>? bind = null)
        where TViewModel : class
        where TPage : Page
    {
        Action<Page, TViewModel>? pageBind = bind is null ? null : (page, viewModel) => bind((TPage)page, viewModel);

        return RegisterRoute(
            route,
            (navigationService, _, _) => createViewModel(navigationService),
            static (navigationService, activator, viewModel) => activator.Create<TPage>(viewModel),
            pageBind);
    }

    /// <summary>
    /// Registers a route name for a parameterized target using a custom view model factory.
    /// </summary>
    public NavigationRegistry RegisterRoute<TViewModel, TParameter, TPage>(
        string route,
        Func<INavigationService, TParameter, TViewModel> createViewModel,
        Action<TPage, TViewModel>? bind = null)
        where TViewModel : class
        where TPage : Page
    {
        Action<Page, TViewModel>? pageBind = bind is null ? null : (page, viewModel) => bind((TPage)page, viewModel);

        return RegisterRoute(
            route,
            (navigationService, _, parameter) =>
                createViewModel(navigationService, ResolveParameter<TParameter>(route, parameter)),
            static (navigationService, activator, viewModel) => activator.Create<TPage>(viewModel),
            pageBind);
    }

    /// <summary>
    /// Registers a route name using custom factories for both the view model and page.
    /// </summary>
    public NavigationRegistry RegisterRoute<TViewModel>(
        string route,
        Func<INavigationService, TViewModel> createViewModel,
        Func<INavigationService, TViewModel, Page> createPage)
        where TViewModel : class
    {
        return RegisterRoute(
            route,
            (navigationService, _, _) => createViewModel(navigationService),
            (navigationService, _, viewModel) => createPage(navigationService, viewModel));
    }

    /// <summary>
    /// Registers a parameterized route name using custom factories for both the view model and page.
    /// </summary>
    public NavigationRegistry RegisterRoute<TViewModel, TParameter>(
        string route,
        Func<INavigationService, TParameter, TViewModel> createViewModel,
        Func<INavigationService, TViewModel, Page> createPage)
        where TViewModel : class
    {
        return RegisterRoute(
            route,
            (navigationService, _, parameter) =>
                createViewModel(navigationService, ResolveParameter<TParameter>(route, parameter)),
            (navigationService, _, viewModel) => createPage(navigationService, viewModel));
    }

    /// <inheritdoc />
    public Page CreatePage(Type viewModelType, INavigationService navigationService, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        ArgumentNullException.ThrowIfNull(navigationService);

        if (!_viewModelRegistrations.TryGetValue(viewModelType, out var registration))
            throw new InvalidOperationException($"View model type '{viewModelType.Name}' is not registered.");

        return registration.CreatePage(navigationService, _typeActivator, parameter);
    }

    /// <inheritdoc />
    public Page CreatePage(string route, INavigationService navigationService, object? parameter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentNullException.ThrowIfNull(navigationService);

        if (!_routeRegistrations.TryGetValue(route, out var registration))
            throw new InvalidOperationException($"Route '{route}' is not registered.");

        return registration.CreatePage(navigationService, _typeActivator, parameter);
    }

    /// <summary>
    /// Validates and extracts a strongly typed navigation parameter.
    /// </summary>
    private static TParameter ResolveParameter<TParameter>(string routeName, object? parameter)
    {
        if (parameter is TParameter typedParameter)
            return typedParameter;

        throw new InvalidOperationException(
            $"Navigation target '{routeName}' requires parameter type {typeof(TParameter).Name}.");
    }

    /// <summary>
    /// Stores the resolved page factory for a view model registration.
    /// </summary>
    private NavigationRegistry Register<TViewModel>(
        Func<INavigationService, INavigationTypeActivator, object?, TViewModel> createViewModel,
        Func<INavigationService, INavigationTypeActivator, TViewModel, Page> createPage,
        Action<Page, TViewModel>? bind = null)
        where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(createViewModel);
        ArgumentNullException.ThrowIfNull(createPage);

        _viewModelRegistrations[typeof(TViewModel)] = new NavigationRegistration(
            (navigationService, activator, parameter) =>
            {
                var viewModel = createViewModel(navigationService, activator, parameter);
                var page = createPage(navigationService, activator, viewModel);
                (bind ?? DefaultBind).Invoke(page, viewModel);
                return page;
            });

        return this;
    }

    /// <summary>
    /// Binds a route name to an existing view model registration.
    /// </summary>
    private NavigationRegistry RegisterRoute<TViewModel>(
        string route,
        Func<INavigationService, INavigationTypeActivator, object?, TViewModel> createViewModel,
        Func<INavigationService, INavigationTypeActivator, TViewModel, Page> createPage,
        Action<Page, TViewModel>? bind = null)
        where TViewModel : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        Register(createViewModel, createPage, bind);
        _routeRegistrations[route] = _viewModelRegistrations[typeof(TViewModel)];
        return this;
    }

    /// <summary>
    /// Applies the default page binding by assigning the view model as the page data context.
    /// </summary>
    private static void DefaultBind<TViewModel>(Page page, TViewModel viewModel)
        where TViewModel : class
    {
        page.DataContext = viewModel;
    }

    /// <summary>
    /// Wraps the page-creation delegate stored for each registration.
    /// </summary>
    private sealed class NavigationRegistration
    {
        public NavigationRegistration(Func<INavigationService, INavigationTypeActivator, object?, Page> createPage)
        {
            CreatePage = createPage;
        }

        /// <summary>
        /// Gets the page factory for the registration.
        /// </summary>
        public Func<INavigationService, INavigationTypeActivator, object?, Page> CreatePage { get; }
    }
}