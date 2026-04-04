using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Models;
using ReactiveUI;
using Splat;

namespace NavKit.ReactiveUI.Sample.ViewModels;

public class SampleModalViewModel : ReactiveObject, IModalResultSource<string?>
{
    private readonly ModalNavigationResult<string?> _result = new();

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

    public SampleModalViewModel()
    {
        ConfirmCommand = ReactiveCommand.Create(() => SetResult(UserName));
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void SetResult(string? result)
    {
        _result.SetResult(result);
    }

    private void Cancel()
    {
        _result.Cancel();
    }

    public Task<string?> ResultTask => _result.ResultTask;
}