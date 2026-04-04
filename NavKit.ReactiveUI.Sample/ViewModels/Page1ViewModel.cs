using System.Reactive;
using System.Threading.Tasks;
using NavigationKit.Avalonia.Abstractions;
using ReactiveUI;
using Splat;

namespace NavKit.ReactiveUI.Sample.ViewModels;

public class Page1ViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> NavigateToPage2Command { get; }

    public Page1ViewModel(INavigationService navigation) : base(navigation)
    {
        NavigateToPage2Command =
            ReactiveCommand.CreateFromTask(NavigateToPage2, outputScheduler: RxSchedulers.MainThreadScheduler);
    }

    private Task NavigateToPage2()
    {
        return Navigation.PushAsync("page2");
    }
}