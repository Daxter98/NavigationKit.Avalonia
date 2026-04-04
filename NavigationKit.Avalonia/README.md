# NavigationKit.Avalonia

`NavigationKit.Avalonia` is a reusable navigation library for Avalonia applications built around a VM-first navigation model.

Instead of navigating to pages directly, callers navigate to view models:

```csharp
await navigationService.PushAsync<LoginPageViewModel>();
await navigationService.ReplaceAsync<AppShellPageViewModel>();
await navigationService.PushAsync<WorkoutDetailPageViewModel>(
    new WorkoutDetailRouteParameters("Morning run"));
```

The library resolves the target page from a registry, creates the view model, binds the page `DataContext`, and drives navigation through an Avalonia `NavigationPage`.

## Project status

`NavigationKit.Avalonia` is currently a work in progress.

It currently depends on Avalonia `12.0.0-rc2`, so the API, behavior, and documentation are still subject to change as Avalonia approaches a stable release and the library itself continues to evolve.

## Features

- VM-first navigation API
- ViewModel-to-Page registration
- Optional route-name navigation
- Parameterized navigation
- Modal and non-modal navigation
- Result-returning modal navigation
- Navigation lifecycle hooks through `INavigationAware`
- State notifications through `INavigationService.StateChanged`
- Manual setup support
- `IServiceProvider` / Microsoft DI integration
- Splat / ReactiveUI integration

## Core concepts

### `INavigationService`

This is the main runtime API used by view models and other application services.

Supported operations include:

- `PushAsync<TViewModel>(parameter)`
- `PushAsync(Type, parameter)`
- `PushAsync(string route, parameter)`
- `PushModalAsync<TViewModel>(parameter)`
- `PushModalForResultAsync<TViewModel, TResult>(parameter)`
- `ReplaceAsync<TViewModel>(parameter)`
- `GoBackAsync()`
- `DismissModalAsync()`
- `DismissAllModalsAsync()`
- `PopToRootAsync()`

The first non-modal navigation initializes the root page in the host `NavigationPage`.

### `NavigationRegistry`

`NavigationRegistry` is the mapping layer between view models, routes, and pages.

It supports:

- registering a view model and page pair
- registering a parameterized target
- registering a named route
- supplying custom view model factories
- supplying custom page factories
- overriding the default page binding step

By default, the created view model is assigned to `page.DataContext`.

### `INavigationAware`

If a page `DataContext` implements `INavigationAware`, the library forwards navigation lifecycle events:

- `OnNavigatingAsync`
- `OnNavigatedFromAsync`
- `OnNavigatedToAsync`

This is useful for loading data, releasing resources, or reacting to route transitions.

### `INavigationServiceFactory`

`INavigationServiceFactory` creates a runtime `INavigationService` from an Avalonia `NavigationPage` host.

```csharp
var navigationService = navigationServiceFactory.Create(navRoot);
```

## Basic usage

### 1. Create a registry

Create an application-specific registry by deriving from `NavigationRegistry`.

```csharp
using NavigationKit.Avalonia;

public sealed class AppPageFactory : NavigationRegistry
{
    public AppPageFactory(INavigationTypeActivator? typeActivator = null)
        : base(typeActivator)
    {
        RegisterRoute<LoginPageViewModel, LoginPageView>("login");
        RegisterRoute<DashboardPageViewModel, DashboardPageView>("dashboard");

        RegisterRoute<WorkoutDetailPageViewModel, WorkoutDetailRouteParameters, WorkoutDetailPageView>(
            "workout-detail",
            static (navigationService, parameter) =>
                new WorkoutDetailPageViewModel(navigationService, parameter.WorkoutName));
    }
}
```

### 2. Create a navigation service factory

```csharp
var viewResolver = new AppPageFactory();
var navigationServiceFactory = new NavigationServiceFactory(viewResolver);
```

### 3. Bind to an Avalonia `NavigationPage`

```csharp
var navigationService = navigationServiceFactory.Create(navRoot);
```

### 4. Navigate from a view model

```csharp
public sealed class MainViewModel
{
    private readonly INavigationService _navigationService;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public Task InitializeAsync()
    {
        return _navigationService.PushAsync<LoginPageViewModel>();
    }
}
```

## Registration patterns

### Register a simple view model/page pair

```csharp
Register<LoginPageViewModel, LoginPageView>();
RegisterRoute<LoginPageViewModel, LoginPageView>("login");
```

### Register a parameterized view model/page pair

```csharp
Register<WorkoutDetailPageViewModel, WorkoutDetailRouteParameters, WorkoutDetailPageView>();

RegisterRoute<WorkoutDetailPageViewModel, WorkoutDetailRouteParameters, WorkoutDetailPageView>(
    "workout-detail");
```

In this form, the view model and page are created through the current `INavigationTypeActivator`.

### Register with a custom view model factory

```csharp
RegisterRoute<WorkoutDetailPageViewModel, WorkoutDetailRouteParameters, WorkoutDetailPageView>(
    "workout-detail",
    static (navigationService, parameter) =>
        new WorkoutDetailPageViewModel(navigationService, parameter.WorkoutName));
```

Use this when the constructor shape is app-specific or when you need to transform the navigation parameter before constructing the view model.

### Register with a custom page factory

```csharp
RegisterRoute<AppShellPageViewModel>(
    "app-shell",
    static navigationService => new AppShellPageViewModel(navigationService),
    static (_, viewModel) => new AppShellPage(viewModel));
```

Use this when the page needs custom construction instead of simple `new TPage(viewModel)` or `new TPage()` activation.

### Override the default page binding step

By default, the created view model is assigned to `page.DataContext`.

If you need additional binding behavior, provide a bind callback:

```csharp
Register<LoginPageViewModel, LoginPageView>(
    static (page, viewModel) =>
    {
        page.DataContext = viewModel;
        page.Header = viewModel.Title;
    });
```

## Parameter passing

Any navigation call can include a parameter object:

```csharp
await navigationService.PushAsync<WorkoutDetailPageViewModel>(
    new WorkoutDetailRouteParameters("Morning run"));
```

The parameter type must match the type declared in the registry registration.

If the type does not match, the library throws an `InvalidOperationException` with a clear message.

## Result-returning modal navigation

For modal workflows such as pickers, confirmation dialogs, or create/edit forms, the library can await a result from the modal view model.

The modal view model must implement `IModalResultSource<TResult>`.

The easiest way to do that is to wrap a `ModalNavigationResult<TResult>` field.

```csharp
using System.Threading.Tasks;
using NavigationKit.Avalonia;

public sealed class WorkoutPickerViewModel : IModalResultSource<string?>
{
    private readonly ModalNavigationResult<string?> _result = new();

    public Task<string?> ResultTask => _result.ResultTask;

    public void CancelResult()
    {
        _result.Cancel();
    }

    public void SelectWorkout(string workoutName)
    {
        _result.SetResult(workoutName);
    }

    public void Cancel()
    {
        _result.Cancel();
    }
}
```

Caller side:

```csharp
var selectedWorkout = await navigationService
    .PushModalForResultAsync<WorkoutPickerViewModel, string?>();
```

If the modal is dismissed without producing a result, the pending modal task is canceled.

Supported overloads:

- `PushModalForResultAsync<TViewModel, TResult>(parameter)`
- `PushModalForResultAsync<TResult>(Type, parameter)`
- `PushModalForResultAsync<TResult>(string route, parameter)`

If the page `DataContext` does not implement `IModalResultSource<TResult>`, the library throws an `InvalidOperationException`.

## Route-name navigation

You can navigate by route string when you want to decouple the caller from the concrete view model type.

```csharp
await navigationService.PushAsync("login");
await navigationService.ReplaceAsync("app-shell");
await navigationService.PushAsync(
    "workout-detail",
    new WorkoutDetailRouteParameters("Intervals"));
```

Route registration is case-insensitive.

Result-returning modal navigation also works with route names:

```csharp
var selectedWorkout = await navigationService.PushModalForResultAsync<string?>("workout-picker");
```

## Lifecycle integration

Implement `INavigationAware` on the view model assigned to the page `DataContext`.

```csharp
using System.Threading.Tasks;
using Avalonia.Controls;
using NavigationKit.Avalonia;

public sealed class LoginPageViewModel : INavigationAware
{
    public ValueTask OnNavigatingAsync(NavigatingFromEventArgs args)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnNavigatedFromAsync(NavigatedFromEventArgs args)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnNavigatedToAsync(NavigatedToEventArgs args)
    {
        return ValueTask.CompletedTask;
    }
}
```

## Navigation state notifications

`INavigationService` exposes a `StateChanged` event with:

- current page header
- navigation depth
- modal depth
- last action

```csharp
navigationService.StateChanged += (_, args) =>
{
    Console.WriteLine($"Page: {args.CurrentPageHeader}");
    Console.WriteLine($"Stack: {args.NavigationDepth}");
    Console.WriteLine($"Modals: {args.ModalDepth}");
    Console.WriteLine($"Action: {args.LastAction}");
};
```

## Modal result cancellation semantics

- If a modal completes its `ResultTask`, the navigation service dismisses that modal automatically.
- If a pending result modal is dismissed through `DismissModalAsync()` or `DismissAllModalsAsync()`, the result task is canceled.
- If the modal is popped through the `NavigationPage` dismissal path, the service also cancels the pending result task.

## Activation models

The library uses `INavigationTypeActivator` to construct view models and pages.

Available activators:

- `ReflectionNavigationTypeActivator`
- `ServiceProviderNavigationTypeActivator`
- `SplatNavigationTypeActivator`

### Reflection activation

This is the default if no activator is supplied.

It:

- matches explicit constructor arguments first
- resolves remaining dependencies from the configured service resolver if one exists
- falls back to constructing concrete types recursively

### Microsoft DI / `IServiceProvider`

Use the provided registration helper:

```csharp
services.AddAvaloniaNavigation<AppPageFactory>();
```

This registers:

- `INavigationTypeActivator`
- your registry type
- `INavigationViewResolver`
- `INavigationServiceFactory`

Example:

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddAvaloniaNavigation<AppPageFactory>();
services.AddSingleton<MainViewModel>();

var provider = services.BuildServiceProvider();

var navigationServiceFactory = new NavigationServiceFactory(
    provider.GetRequiredService<INavigationViewResolver>());
```

### Splat / ReactiveUI

Use the provided Splat helper:

```csharp
Locator.CurrentMutable.RegisterAvaloniaNavigation<AppPageFactory>();
```

This eagerly registers:

- `INavigationTypeActivator`
- your registry type
- `INavigationViewResolver`
- `INavigationServiceFactory`

Example:

```csharp
using Splat;

Locator.CurrentMutable.RegisterAvaloniaNavigation<AppPageFactory>();
Locator.CurrentMutable.RegisterLazySingleton(() => new MainViewModel());

var navigationServiceFactory = new NavigationServiceFactory(
    Locator.Current.GetService<INavigationViewResolver>()
        ?? throw new InvalidOperationException("Navigation view resolver registration is missing."));
```

## Manual setup without a container

If you do not want to use a DI container, create the registry and factory directly:

```csharp
var registry = new AppPageFactory();
var navigationServiceFactory = new NavigationServiceFactory(registry);
```

You can also supply a custom activator explicitly:

```csharp
var activator = new ReflectionNavigationTypeActivator();
var registry = new AppPageFactory(activator);
var navigationServiceFactory = new NavigationServiceFactory(registry);
```

## Typical Avalonia host setup

In a root view containing a `NavigationPage`:

```csharp
private async void OnLoaded(object? sender, RoutedEventArgs e)
{
    if (this.FindControl<NavigationPage>("NavRoot") is not { } navRoot)
        return;

    var navigationService = navigationServiceFactory.Create(navRoot);
    mainViewModel.AttachNavigation(navigationService);

    await mainViewModel.InitializeAsync();
}
```

## Notes and constraints

- Navigation is VM-first, not page-first.
- The library assumes your application hosts routed content inside an Avalonia `NavigationPage`.
- Non-modal startup navigation initializes the first page as the root page.
- Route registration is optional. You can navigate entirely by view model type if preferred.
- Parameter type validation happens at runtime against the registry declaration.

## Example flow

```csharp
public sealed class LoginPageViewModel
{
    private readonly INavigationService _navigationService;

    public LoginPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public Task SignInAsync()
    {
        return _navigationService.ReplaceAsync<AppShellPageViewModel>();
    }
}
```

```csharp
public sealed class ConfirmDeleteViewModel : IModalResultSource<bool>
{
    private readonly ModalNavigationResult<bool> _result = new();

    public Task<bool> ResultTask => _result.ResultTask;

    public void CancelResult()
    {
        _result.Cancel();
    }

    public void Confirm()
    {
        _result.SetResult(true);
    }

    public void Cancel()
    {
        _result.Cancel();
    }
}
```

```csharp
var confirmed = await navigationService.PushModalForResultAsync<ConfirmDeleteViewModel, bool>();

if (confirmed)
{
    await DeleteAsync();
}
```

```csharp
public sealed class DashboardPageViewModel
{
    private readonly INavigationService _navigationService;

    public DashboardPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public Task OpenWorkoutAsync()
    {
        return _navigationService.PushAsync<WorkoutDetailPageViewModel>(
            new WorkoutDetailRouteParameters("Morning run"));
    }
}
```

## TODO

- Add a fuller Microsoft DI example covering registration and app startup wiring
- Do a general code review across the library and sample app
- Expand the docs with more end-to-end usage examples
- Review naming and API consistency before a stable release

## Summary

`NavigationKit.Avalonia` is designed for applications that want:

- a reusable Avalonia navigation layer
- strongly typed VM-first navigation
- optional route names
- simple parameter passing
- result-returning modal workflows
- lifecycle callbacks
- compatibility with manual wiring, Microsoft DI, and Splat / ReactiveUI
