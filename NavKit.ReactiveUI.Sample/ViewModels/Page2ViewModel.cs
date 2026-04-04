using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NavigationKit.Avalonia.Abstractions;
using ReactiveUI;
using Splat;

namespace NavKit.ReactiveUI.Sample.ViewModels;

public class Page2ViewModel : ViewModelBase, IDisposable
{
    private readonly CompositeDisposable _subscriptions = new();

    private readonly INavigationService _navigationService;

    public string? UserName
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public string? ErrorMessage
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, string?> GreetCommand { get; }

    public Page2ViewModel(INavigationService? navigation = null)
    {
        _navigationService = navigation ?? AppLocator.Current.GetService<INavigationService>()!;

        GreetCommand = ReactiveCommand.CreateFromTask(OpenNameInputModalAsync,
            outputScheduler: RxSchedulers.MainThreadScheduler);

        _subscriptions.Add(
            GreetCommand.Subscribe(name =>
            {
                ErrorMessage = null;
                UserName = string.IsNullOrWhiteSpace(name) ? "Anonymous" : name;
            }));

        _subscriptions.Add(
            GreetCommand.ThrownExceptions
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(ex => ErrorMessage = ex is OperationCanceledException ? null : ex.Message));
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    private Task<string?> OpenNameInputModalAsync()
    {
        return _navigationService.PushModalForResultAsync<SampleModalViewModel, string?>();
    }
}