using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NavigationKit.Avalonia.Abstractions;
using NavigationKit.Avalonia.Services;
using Splat;

namespace NavKit.ReactiveUI.Sample;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(CreateNavigationServiceFactory());
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static INavigationServiceFactory CreateNavigationServiceFactory()
    {
        return new NavigationServiceFactory(ResolveRequired<INavigationViewResolver>());
    }

    private static T ResolveRequired<T>() where T : class
    {
        return AppLocator.Current.GetService<T>()
               ?? throw new InvalidOperationException($"Splat registration missing for '{typeof(T).Name}'.");
    }
}