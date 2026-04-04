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
        Register<Page1Vm, Page1>();

        // Route-based registration
        RegisterRoute<Page2Vm, Page2>("page2");

        // Modal registration
        Register<SampleModalViewModel, Pages.SampleModal>();
    }
}