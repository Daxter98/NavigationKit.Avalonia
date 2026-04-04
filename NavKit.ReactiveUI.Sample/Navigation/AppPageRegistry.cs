using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Services;
using NavKit.ReactiveUI.Sample.Pages;
using NavKit.ReactiveUI.Sample.ViewModels;

namespace NavKit.ReactiveUI.Sample.Navigation;

public class AppPageRegistry : NavigationRegistry
{
    public AppPageRegistry(INavigationTypeActivator? activator = null) : base(activator)
    {
        // Vm-only registration
        Register<Page1ViewModel, Page1>();

        // Route-based registration
        RegisterRoute<Page2ViewModel, Page2>("page2");

        // Modal registration
        Register<SampleModalViewModel, SampleModal>();
    }
}