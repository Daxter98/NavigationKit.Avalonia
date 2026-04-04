using System.Reactive.Disposables.Fluent;
using Avalonia.Controls;
using NavKit.ReactiveUI.Sample.ViewModels;
using ReactiveUI;

namespace NavKit.ReactiveUI.Sample.Pages;

public partial class Page1 : ContentPage, IViewFor<Page1Vm>
{
    public Page1()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.BindCommand(ViewModel,
                    propertyName: vm => vm.NavigateToPage2Command,
                    controlName: v => v.GoToPage2Button)
                .DisposeWith(disposables);
        });
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (Page1Vm?)value;
    }

    public Page1Vm? ViewModel
    {
        get => DataContext as Page1Vm;
        set => DataContext = value;
    }
}