using System.Threading.Tasks;
using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Models;
using NavKit.ReactiveUI.Sample.ViewModels;
using ReactiveUI;

namespace NavKit.ReactiveUI.Sample;

public class MainViewModel : ReactiveObject
{
    private INavigationService? _navigationService;

    public string LastAction
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;
    
    public int NavigationDepth
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public void AttachNavigationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        // Attach to the navigation state
        _navigationService.StateChanged += OnNavigationStateChanged;
    }

    public Task InitializeAsync()
    {
        if (_navigationService is null)
        {
            return Task.CompletedTask;
        }

        return _navigationService.PushAsync<Page1Vm>();
    }
    
    private void OnNavigationStateChanged(object? sender, NavigationStateChangedEventArgs args)
    {
        LastAction = args.LastAction;
        NavigationDepth = args.NavigationDepth;
    }
}