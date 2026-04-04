using NavigationKit.Avalonia.Abstractions;
using ReactiveUI;

namespace NavKit.ReactiveUI.Sample.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    protected ViewModelBase(INavigationService navigationService)
    {
        Navigation = navigationService;
    }

    protected INavigationService Navigation { get; }
}