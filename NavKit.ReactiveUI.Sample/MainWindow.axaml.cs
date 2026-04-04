using System.Reactive.Disposables.Fluent;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NavigationKit.Avalonia.Abstractions;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;

namespace NavKit.ReactiveUI.Sample;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    private readonly INavigationServiceFactory? _navigationServiceFactory;
    private bool _initialized;

    // We keep the empty constructor to avoid avalonia runtime loader warning
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(INavigationServiceFactory navigationServiceFactory) : this()
    {
        _navigationServiceFactory = navigationServiceFactory;

        ViewModel = AppLocator.Current.GetService<MainViewModel>() ?? new MainViewModel();

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel,
                    vmProperty: vm => vm.LastAction,
                    viewProperty: v => v.LastActionTextBlock.Text)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    vmProperty: vm => vm.NavigationDepth,
                    viewProperty: v => v.NavigationDepthTextBlock.Text,
                    selector: depth => $"Stack {depth}")
                .DisposeWith(disposables);
        });
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_initialized || Design.IsDesignMode)
        {
            return;
        }

        if (this.FindControl<NavigationPage>("NavigationRoot") is not { } navigationPage)
        {
            return;
        }

        _initialized = true;

        var navigationService = _navigationServiceFactory?.Create(navigationPage)!;

        ViewModel?.AttachNavigationService(navigationService);

        await ViewModel?.InitializeAsync()!;
    }
}