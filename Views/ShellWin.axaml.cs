using System.Collections.Concurrent;
using System.Security;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ScraperOne.DataModels;
using ScraperOne.Logger;
using ScraperOne.Services;
using ScraperOne.ViewModels;



namespace ScraperOne.Views;



public partial class ShellWin : ReactiveWindow<ShellWinViewModel>
{
    private static ColorLogger s_logger;

    private static ShellWin _instance;
    public static ShellWin Instance { get { return _instance; } }
    public ShellWin()
    {
        ViewModel = new ShellWinViewModel();
        s_logger = DiServiceLoader.LoggerFactory.CreateLogger("ShellWin");
        _instance = this;
        AvaloniaXamlLoader.Load(this);

    }
    // Command execution properties for child controls
    // Some dispute over best practices in this area
    // Child User Controls sharing and acting on prop values

    //-------------------------------------------------------------------




    //------------------------------------------------------------------------

   

    //------------------------------------------------------------------------



    private static IObservable<bool> _isBusy;

    public static IObservable<bool> IsBusy => _isBusy;





}