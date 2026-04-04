using System.Reactive.Disposables.Fluent;
using Avalonia.Controls;
using NavKit.ReactiveUI.Sample.ViewModels;
using ReactiveUI;

namespace NavKit.ReactiveUI.Sample.Pages;

public partial class Page2 : ContentPage, IViewFor<Page2Vm>
{
    public Page2()
    {
        InitializeComponent();

        this.WhenActivated(block: disposables =>
        {
            this.OneWayBind(viewModel: ViewModel,
                    vmProperty: vm => vm.UserName,
                    viewProperty: v => v.ResultTextBlock.Text,
                    selector: value => $"Hello, {value}!")
                .DisposeWith(compositeDisposable: disposables);

            this.OneWayBind(viewModel: ViewModel,
                    vmProperty: vm => vm.ErrorMessage,
                    viewProperty: v => v.ExceptionsTextBlock.Text)
                .DisposeWith(compositeDisposable: disposables);

            this.BindCommand(viewModel: ViewModel,
                    propertyName: vm => vm.GreetCommand,
                    controlName: v => v.ModalButton)
                .DisposeWith(compositeDisposable: disposables);
        });
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (Page2Vm?)value;
    }

    public Page2Vm? ViewModel
    {
        get => DataContext as Page2Vm;
        set => DataContext = value;
    }
}