using System.Reactive;
using NavigationKit.Avalonia.Abstractions;
using ReactiveUI;
using Splat;

namespace NavKit.ReactiveUI.Sample.ViewModels;

public class Page1Vm : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> NavigateToPage2Command { get; }

    public Page1Vm(INavigationService? navigation = null)
    {
        var navigationService = navigation ?? AppLocator.Current.GetService<INavigationService>()!;

        NavigateToPage2Command = ReactiveCommand.CreateFromTask(async () =>
        {
            await navigationService.PushAsync("page2");
        });
    }
}