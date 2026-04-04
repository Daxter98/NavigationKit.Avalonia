using System.Reactive.Disposables.Fluent;
using Avalonia.Controls;
using NavKit.ReactiveUI.Sample.ViewModels;
using ReactiveUI;

namespace NavKit.ReactiveUI.Sample.Pages;

public partial class Page2 : ContentPage, IViewFor<Page2ViewModel>
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
        set => ViewModel = (Page2ViewModel?)value;
    }

    public Page2ViewModel? ViewModel
    {
        get => DataContext as Page2ViewModel;
        set => DataContext = value;
    }
}