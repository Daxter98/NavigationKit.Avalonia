using System.Reactive.Disposables.Fluent;
using Avalonia.Controls;
using NavKit.ReactiveUI.Sample.ViewModels;
using ReactiveUI;

namespace NavKit.ReactiveUI.Sample.Pages;

public partial class Page1 : ContentPage, IViewFor<Page1ViewModel>
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
        set => ViewModel = (Page1ViewModel?)value;
    }

    public Page1ViewModel? ViewModel
    {
        get => DataContext as Page1ViewModel;
        set => DataContext = value;
    }
}