// 
// Program: Scraper One
// Author:  Kyle Crowder
// License : Open Source
// Portions of code taken from TumblrThree
// 
// 052023


using Avalonia;
using Avalonia.ReactiveUI;

namespace ScraperOne;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

/*

    public static void Mainzzzzz(string[] args)
    {

       // RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
        TaskScheduler.UnobservedTaskException += UnHandledException;

        try
        {

            Locator.CurrentMutable.RegisterConstant(new CookieService());
            Locator.CurrentMutable.RegisterConstant(new ColorLoggerProvider());
            RegisterServices();
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToTrace()
                .SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime());



            }
            catch (Exception uhe)
            {

                System.Console.WriteLine(uhe.Message);
            }

        }

    */


    private static void UnHandledException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        System.Console.WriteLine(e.Exception.Message);
    }
    /*
    public static T UseGtk<T>(this T builder) where T : AppBuilderBase<T>, new()
    {
        builder.WindowingSubsystem = Avalonia.Gtk.GtkPlatform.Initialize;
        builder.WindowingSubsystemInitializer = Gtk.GtkPlatform.Initialize;
        builder.WindowingSubsystemName = "Gtk";
        return builder;
    }
*/
}