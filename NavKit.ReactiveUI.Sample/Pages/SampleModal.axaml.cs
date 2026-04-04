using System.Reactive.Disposables.Fluent;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NavKit.ReactiveUI.Sample.ViewModels;
using ReactiveUI;

namespace NavKit.ReactiveUI.Sample.Pages;

public partial class SampleModal : ContentPage, IViewFor<SampleModalViewModel>
{
    public SampleModal()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                    vmProperty: vm => vm.UserName,
                    viewProperty: v => v.NameTextBox.Text)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel,
                    propertyName: vm => vm.ConfirmCommand,
                    controlName: v => v.ConfirmButton)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel,
                    propertyName: vm => vm.CancelCommand,
                    controlName: v => v.CancelButton)
                .DisposeWith(disposables);
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.FindControl<TextBox>("NameTextBox")?.Focus();
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (SampleModalViewModel?)value;
    }

    public SampleModalViewModel? ViewModel
    {
        get => DataContext as SampleModalViewModel;
        set => DataContext = value;
    }
}