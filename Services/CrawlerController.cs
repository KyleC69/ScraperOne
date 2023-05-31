// 
// Program: Scraper One
// Author:  Kyle Crowder
// License : Open Source
// Portions of code taken from TumblrThree
// 
// 052023

#pragma warning disable CS0649, CS0149


using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PuppeteerSharp.Helpers;
using ScraperOne.DataModels;
using ScraperOne.Logger;
using ScraperOne.Modules;
using ScraperOne.Modules.Crawlers;

namespace ScraperOne.Services;

[DebuggerDisplay("count={i_counter}")]
public class CrawlerController : ServiceBase
{
    public static CancellationTokenSource _Cts = new();

    private static ColorLogger s_logger;
    private static CancellationToken s_ct;
    private readonly TaskCompletionSource<bool> i_archiveLoaded;

    private readonly int i_counter;
    private readonly TaskCompletionSource<bool> i_databasesLoaded;
    private readonly TaskCompletionSource<bool> i_libraryLoaded;


    private readonly object i_lockObject;
    private readonly List<Task> i_runningTasks;

    #region Setup/Teardown

    public CrawlerController(ColorLogger createLogger)
    {
        s_logger = createLogger;
        s_logger.LogInformation("Crawler Controller Service loaded");
        i_counter++;
        ActiveItems = new ObservableCollection<QueueListItem>();
        ActiveItems.CollectionChanged += ActiveItemsCollectionChanged;

        i_libraryLoaded = new TaskCompletionSource<bool>();
        i_databasesLoaded = new TaskCompletionSource<bool>();
        i_archiveLoaded = new TaskCompletionSource<bool>();
        InitializationComplete = new TaskCompletionSource<bool>();
        i_lockObject = new object();
        s_ct = _Cts.Token;
        i_runningTasks = new List<Task>();
    }

    #endregion


    public TaskCompletionSource<bool> InitializationComplete { get; set; }
    public ObservableCollection<QueueListItem> ActiveItems { get; private set; }


    private void ActiveItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove)
        {
            RaisePropertyChanged(nameof(ActiveItems));
        }
    }


    public static async Task CrawlSingleBlog(IBlog blog)
    {
        _ = blog.Load(App.Settings.DownloadLocation);

        NewTumblCrawler crawler = ModuleFactory.GetCrawler(blog, new Progress<string>(), s_ct);
        string document = await crawler.GetApiPageAsync(1);
        _ = JsonConvert.DeserializeObject(document);
        _ = JsonConvert.SerializeObject(document);


        // document.FixHtml();


        await Task.CompletedTask;
    }


    public void ClearItems()
    {
        ActiveItems.Clear();
    }


    public void Shutdown()
    {
        foreach (IBlog blog in ManagerService.BlogFiles)
        {
            if (blog.Dirty)
            {
                _ = blog.Save();
            }
        }

        try
        {
            s_logger.LogInformation($"Running Task count at shutdown {i_runningTasks.Count}");
            _ = Task.WhenAll(i_runningTasks.ToArray()).WithTimeout(60000);
        }
        catch (AggregateException ae)
        {
            s_logger.LogError(ae, "Shutdown contained errors");
        }
    }


    public async Task SetupCrawlAsync()
    {
        s_logger.LogDebug("----------------  Crawl Setup Started---------");


        for (int i = 0; i < App.Settings.ConcurrentBlogs; i++)
        {
            i_runningTasks.Add(RunCrawlerTasksAsync(s_ct));
        }

        s_logger.LogDebug("Crawling Task Starting");
        await CrawlAsync();
        s_logger.LogCritical("***************   Crawler Task has been completed  *************8");
    }


    /// <summary>
    ///     Custom crawling start
    /// </summary>
    /// <returns></returns>
    public async void StartCrawler()
    {
        await RunCrawlerTasksAsync(s_ct);
    }


    private async Task CrawlAsync()
    {
        try
        {
            await Task.WhenAll(i_runningTasks.ToArray());
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
        finally
        {
            i_runningTasks.Clear();
        }
    }


    private async Task RunCrawlerTasksAsync(CancellationToken ct)
    {
        while (true)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            {
                try
                {
                    if ((ActiveItems.Count < QueueManager.Items.Count) && QueueManager.Items.Any())
                    {
                        QueueListItem nextQueueItem;
                        try
                        {
                            nextQueueItem = QueueManager.Items.Except(ActiveItems).First();
                        }
                        catch (InvalidOperationException)
                        {
                            continue;
                        }

                        IBlog blog = nextQueueItem.Blog;
                        ICrawler crawler = ModuleFactory.GetCrawler(blog, new Progress<string>(), ct);
                        try
                        {
                            if (!blog.Online)
                            {
                                QueueManager.RemoveItem(nextQueueItem);

                                continue;
                            }

                            bool t = crawler.IsBlogOnlineAsync().Wait(4000, ct);
                        }
                        catch (AggregateException ex)
                        {
                            s_logger.LogError(999, ex, "CrawlerController::RunCrawlerTasksAsync::ISBLOGONLINE::Error");
                        }
                        finally
                        {
                            crawler.Dispose();
                        }

                        if (ActiveItems.Any(item =>
                                item.Blog.Name.Equals(nextQueueItem.Blog.Name, StringComparison.Ordinal)))
                        {
                            QueueManager.RemoveItem(nextQueueItem);

                            continue;
                        }

                        AddActiveItems(nextQueueItem);

                        await StartSiteSpecificDownloaderAsync(nextQueueItem, ct);
                    }
                    else
                    {
                        await Task.Delay(4000, ct);
                        Console.WriteLine("QueueManager.IsEmpty");
                    }
                }
                catch (Exception ex)
                {
                    s_logger.LogError(ex.Message);
                    if (!ct.IsCancellationRequested)
                    {
                        s_logger.LogError("CrawlerController.RunCrawlerTasksAsync:CatchAllException");
                    }
                }
            }
        }
    }


    private void ThreadProc(object state)
    {
        GCMemoryInfo info = GC.GetGCMemoryInfo();
        System.Console.Write("Frag Bytes");
        System.Console.WriteLine(info.FragmentedBytes);

        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForFullGCComplete();
        Console.WriteLine("Garbage Cleanup is complete;");
    }


    private void AddActiveItems(QueueListItem nextQueueItem)
    {
        ActiveItems.Add(nextQueueItem);
    }


    /// <summary>
    /// </summary>
    /// <param name="queueListItem"></param>
    /// <param name="ct"></param>
    private async Task StartSiteSpecificDownloaderAsync(QueueListItem queueListItem, CancellationToken ct)
    {
        IBlog blog = queueListItem.Blog;
        blog.Dirty = true;
        ProgressThrottler<DownloadProgress> progress = SetupThrottledQueueListProgress(queueListItem);
        ICrawler crawler = null;
        try
        {
            crawler = ModuleFactory.GetCrawler(blog, null, ct);
            queueListItem.InterruptionRequested += crawler.InterruptionRequestedEventHandler;
            await crawler.CrawlAsync();
            blog.UpdateProgress(false);
        }
        catch (Exception ex)
        {
            s_logger.LogError("CrawlerController.StartSiteSpecificDownloaderAsync: ");
            s_logger.LogError(ex.Message);
        }
        finally
        {
            if (crawler != null)
            {
                queueListItem.InterruptionRequested -= crawler.InterruptionRequestedEventHandler;
            }

            crawler?.Dispose();
        }

        _ = ActiveItems.Remove(queueListItem);

        if (!ct.IsCancellationRequested)
        {
            QueueManager.RemoveItem(queueListItem);
        }
    }


    private static ProgressThrottler<DownloadProgress> SetupThrottledQueueListProgress(QueueListItem queueListItem)
    {
        Progress<DownloadProgress> progressHandler = new(value => queueListItem.Progress = value.SProgress);

        return new ProgressThrottler<DownloadProgress>(progressHandler, App.Settings.ProgressUpdateInterval);
    }
}