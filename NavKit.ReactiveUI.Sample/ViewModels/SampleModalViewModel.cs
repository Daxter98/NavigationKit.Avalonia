using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Models;
using ReactiveUI;
using Splat;

namespace NavKit.ReactiveUI.Sample.ViewModels;

public class SampleModalViewModel : ViewModelBase, IModalResultSource<string?>
{
    private readonly ModalNavigationResult<string?> _result = new();
    private INavigationService? _navigationService;

    public void CancelResult()
    {
        _result.Cancel();
    }

    public string? UserName
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public SampleModalViewModel(INavigationService? navigationService = null)
    {
        _navigationService = navigationService ?? AppLocator.Current.GetService<INavigationService>()!;
        Debug.WriteLine($"NavigationService Modal stack on ctor call: {_navigationService?.ModalDepth}");
        ConfirmCommand = ReactiveCommand.Create(() => SetResult(UserName));
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void SetResult(string? result)
    {
        _result.SetResult(result);
        
        Debug.WriteLine($"NavigationService Modal stack: {_navigationService?.ModalDepth}");
    }

    private void Cancel()
    {
        _result.Cancel();
    }

    public Task<string?> ResultTask => _result.ResultTask;
}