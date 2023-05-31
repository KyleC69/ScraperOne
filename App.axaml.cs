// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// App.axaml.csApp.axaml.cs032420231:30 AM


using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Notification;
using ScraperOne.DependencyInjection;
using ScraperOne.Properties;
using ScraperOne.Services;
using ScraperOne.ViewModels;
using ScraperOne.Views;
using Splat;
using Splat.ModeDetection;

namespace ScraperOne;

public class App : Application
{
    /// <summary>
    ///     Creates an instance of the <see cref="T:Avalonia.Application" /> class.
    /// </summary>
    public App()
    {
        Splat.ModeDetector.OverrideModeDetector(Mode.Run);
        // Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
    }

    public static IClassicDesktopStyleApplicationLifetime Desktop { get; set; }
    public static AppSettings Settings { get; } = new AppSettings();


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        AppBootStrapper.RegisterServices(Locator.CurrentMutable, Locator.Current);


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Startup += ManagerService.OnAppStartup;
            desktop.Startup += DiServiceLoader.OnApplicationStartup;
            desktop.Exit += DiServiceLoader.OnApplicationExiting;

            desktop.MainWindow = new ShellWin()
            {
                DataContext = new ShellWinViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    #region "UI Alerts Section"

    public static INotificationMessageManager Manager { get; } = new NotificationMessageManager();


    public static void ShowInfo(string msg, string header = "Info")
    {
        //  Alerts.ShowInfo(msg, header);
    }

    public static void ShowWarning(string msg, string header = "Warning")
    {
        //_alerts.ShowWarning(msg,header);
    }

    public static void ShowError(string msg, string header = "Error")
    {
        //  Alerts.ShowError(msg, header);
    }

    #endregion
}