using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Notification;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using ScraperOne.DataModels;
using ScraperOne.Modules.Crawlers;
using ScraperOne.Services;
using ScraperOne.Views;
using Splat;

namespace ScraperOne.ViewModels;

public partial class ManagerButtonsViewModel : ViewModelBase
{
    private ObservableAsPropertyHelper<bool> _isAdding;


    private ObservableAsPropertyHelper<bool> _isCrawling;
    private ObservableAsPropertyHelper<bool> _isEnqueue;
    private ObservableAsPropertyHelper<bool> _isLoading;
    private ObservableAsPropertyHelper<bool> _isRefreshing;

    private ObservableAsPropertyHelper<bool> _isUpdating;
   


    public ManagerButtonsViewModel()
    {
        SetupCommands();
    }

    private ManagerService ManagerService { get; } = Locator.Current.GetService<ManagerService>();
    private CrawlerController CrawlerController { get; } = Locator.Current.GetService<CrawlerController>();

    private INotificationMessageManager Manager { get; } = new NotificationMessageManager();
    public ReactiveCommand<Unit, Unit> AddBlogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> EnqueueBlogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> UpdateBlogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AuthenticateCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
    public ReactiveCommand<Unit, Unit> EnqueBlogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> BlogSettingsCommand { get; set; }
    public ReactiveCommand<Unit, Unit> GarbageCollectCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SendTextCommand { get; set; }
    public ReactiveCommand<Unit, Unit> StartCrawlerCommand { get; set; }

    public bool IsAdding => _isAdding.Value;

    public bool IsUpdating => _isUpdating.Value;

    public bool IsCrawling => _isCrawling.Value;
    public bool IsRefreshing => _isRefreshing.Value;
    public bool IsLoading => _isLoading.Value;
    public bool IsEnqueue => _isEnqueue.Value;



    public IObservable<bool> CanCrawl = Observable.Return(SelectedItem != null);
    public IObservable<bool> CanEnqueue = Observable.Return(SelectedItem != null);

    public IObservable<Unit> AddImpl()
    {
        return Observable.Start(() => throw new NotImplementedException());
    }


    public IObservable<Unit> EnqueueBlogImpl()
    {
        return Observable.Start(() => EnqueueBlog());
    }

    private Unit EnqueueBlog()
    {
        throw new NotImplementedException();
    }

    public IObservable<Unit> UpdateMetaImpl()
    {
        return Observable.Start(UpdateBlogMeta);
    }


    public IObservable<Unit> RefreshCommandImpl()
    {
        return Observable.Start(RefreshBlogGrid);
    }

    private void RefreshBlogGrid()
    {
        ManagerService.Source.Clear();
        ManagerService.LoadAllDatabasesAsync().Wait();
    }

    public IObservable<Unit> CrawlerImpl()
    {
        return Observable.Start(StartCrawler);
    }

    private async void StartCrawler()
    {
        ManagerService.Enqueue(SelectedItem);
        await CrawlerController.SetupCrawlAsync();
    }

    public IObservable<Unit> LoadDBImpl()
    {
        return Observable.Start(() => LoadDB());
    }

    private void LoadDB()
    {
        ManagerService.LoadAllDatabasesAsync().Wait();
    }


    private void SetupCommands()
    {
        AddBlogCommand = ReactiveCommand.CreateFromObservable(AddImpl);
        AddBlogCommand.IsExecuting.ToProperty(this, x => x.IsAdding, out _isAdding);
        AddBlogCommand.ThrownExceptions.Subscribe(ex =>
            this.Log().Error("Something Went wwrong Adding Blog", ex));


        EnqueBlogCommand = ReactiveCommand.CreateFromObservable(EnqueueBlogImpl, CanEnqueue);
        EnqueBlogCommand.IsExecuting.ToProperty(this, x => x.IsEnqueue, out _isEnqueue);
        EnqueBlogCommand.ThrownExceptions.Subscribe(ex =>
            this.Log().Error("Something Went wwrong Adding Blog", ex));


        UpdateBlogCommand = ReactiveCommand.CreateFromObservable(UpdateMetaImpl, CanEnqueue);
        UpdateBlogCommand.IsExecuting.ToProperty(this, x => x.IsUpdating, out _isUpdating);
        UpdateBlogCommand.ThrownExceptions.Subscribe(ex => this.Log().Error("Error Updating Metadata", ex));


        StartCrawlerCommand = ReactiveCommand.CreateFromObservable(CrawlerImpl, CanCrawl);
        StartCrawlerCommand.IsExecuting.ToProperty(this, x => x.IsCrawling, out _isCrawling);
        StartCrawlerCommand.ThrownExceptions.Subscribe(ex => this.Log().Error("Error during crawl", ex));

        SendTextCommand = ReactiveCommand.CreateFromObservable(LoadDBImpl);
        SendTextCommand.IsExecuting.ToProperty(this, x => x.IsLoading, out _isLoading);
        SendTextCommand.ThrownExceptions.Subscribe(ex => this.Log().Error("Error during crawl", ex));

        RefreshCommand = ReactiveCommand.CreateFromObservable(RefreshCommandImpl);
        RefreshCommand.IsExecuting.ToProperty(this, x => x.IsRefreshing, out _isRefreshing);
        RefreshCommand.ThrownExceptions.Subscribe(ex => Console.WriteLine(ex.Message));
    }


    private async void UpdateBlogMeta()
    {
        try
        {
            await ManagerService.UpdateMetaInformationAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("An error occured trying to update Meta Info");
        }
    }

    private void ReportProgress(DownloadProgress obj)
    {
        obj.Blog.Progress = obj.Progress;
        obj.Blog.Save();
    }

    private void SendTextMessage()
    {
        App.ShowError("Error test message");
    }

    private void StartGarbageCollect()
    {
        App.ShowError("Actions not implemented", "NOT Implemented");
    }


    private void DoAuthentication()
    {
        try
        {
            Dispatcher.UIThread.Post(async () => await Authenticate(), DispatcherPriority.Background);
        }
        catch (Exception e)
        {
            App.ShowError(e.Message, "Exception Thrown");
            ;
        }
    }


    private async Task Authenticate()
    {
        try
        {
            App.ShowError("Request for authentication recd.");
            //  AuthenticationManager am = new();
            //  App.ShowError("AM obj created Beginning Authentication...");
            var ls = new LoginService();
            //  App.ShowError("Login Service obj created");
            var success = await ls.AuthenticateNewtumblBrowserAsync();

            if (success) Console.WriteLine("authentication was successful");
            App.ShowError("We have succesfully logged in..");
        }
        catch (Exception)
        {
            App.ShowError("Login failed...");
        }
    }


    private async void DeleteBlog()
    {
        var progress = new Progress<DownloadProgress>(UpdateActivityGridProgress);
        var blog = ManagerService.BlogFiles.LastOrDefault();
        {
            App.ShowError($"updating meta for blog== {blog.Name}");
            var crawler = ModuleFactory.GetCrawler(blog, null, CancellationToken.None);
            var a = await crawler.GetBasicBlogInfo(blog.Url);
            var b = await AbstractCrawler.GetBasicBlogInfoCoreAsync(blog.Url, 1);
            App.ShowError("Button Trip counter is ");
        }
    }


    private void SendText_OnClick()
    {
        Manager
            .CreateMessage()
            .HasHeader("Lost connection to server")
            .HasMessage("Reconnecting...")
            .WithOverlay(new ProgressBar
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 3,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                Background = Brushes.Transparent,
                IsIndeterminate = true,
                IsHitTestVisible = false
            }).Queue();
    }


    private void UpdateActivityGridProgress(DownloadProgress obj)
    {
        Console.WriteLine(obj.Progress);
    }


    private Task EnqueueBlog(IBlog blogName)
    {
        IBlog blog = blogName;
        if (blog is null)
        {
            App.ShowError("Select item from list before adding to que", "Failed to Que");
            return Task.CompletedTask;
        }


        ManagerService.Enqueue(blog);
        CrawlerController.StartCrawler();
        return Task.CompletedTask;
    }

    private void AddBlog()
    {
        App.ShowError("Actions not implemented", "NOT Implemented");
    }
}