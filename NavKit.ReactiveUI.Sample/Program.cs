using Avalonia;
using System;
using System.Reflection;
using NavigationKit.Avalonia.Extensions;
using NavKit.ReactiveUI.Sample.Navigation;
using ReactiveUI.Avalonia;

namespace NavKit.ReactiveUI.Sample;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI(rxBuilder =>
            {
                rxBuilder
                    .WithAvalonia()
                    .WithViewsFromAssembly(Assembly.GetExecutingAssembly())
                    .WithRegistration(locator =>
                    {
                        locator.RegisterAvaloniaNavigation<AppPageRegistry>();
                        locator.RegisterLazySingleton(static () => new MainViewModel());
                    });
            })
            .RegisterReactiveUIViewsFromEntryAssembly()
            .LogToTrace();
}