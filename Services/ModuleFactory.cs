// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ModuleFactory.csModuleFactory.cs032320233:30 AM

#pragma warning disable CS0649,CS0149


using System.Diagnostics;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ScraperOne.Data;
using ScraperOne.DataModels;
using ScraperOne.DataModels.Files;
using ScraperOne.Modules;
using ScraperOne.Modules.Crawlers;
using ScraperOne.Modules.Downloaders;
using ScraperOne.Modules.Parsers;

namespace ScraperOne.Services;

public interface IModuleFactory
{
    List<IFiles> Databases { get; }
    TaskCompletionSource<bool> InitializationComplete { get; set; }

    Task<HttpResponseMessage> SendHttpRequestAsync(HttpRequestMessage request);
}

[DebuggerDisplay("count={counter}")]
public class ModuleFactory
{
    private static readonly ICrawlerService sro_CrawlerService;
    private static ILogger s_logger;


    public ModuleFactory()
    {
        s_logger = DiServiceLoader.LoggerFactory.CreateLogger("ModuleFactory");
        s_logger.LogInformation("ModuleFactory::Loaded:ctor");
    }


    public static NewTumblCrawler GetCrawler(
        IBlog blog,
        [CanBeNull] IProgress<string> progress,
        CancellationToken ct)
    {
        blog.DownloadedItemsNew = 0;
        var postQueue = GetProducerConsumerCollection();
        var files = ManagerService.LoadFilesAsync(blog);
        IWebRequestFactory webRequestFactory = new WebRequestFactory();
        //   IImgurParser imgurParser = GetImgurParser(webRequestFactory, ct);
        //   IGfycatParser gfycatParser = GetGfycatParser(webRequestFactory, ct);
        var jsonNewTumblQueue = GetJsonQueue<Post>();
        return new NewTumblCrawler(sro_CrawlerService, webRequestFactory, ServiceBase.CookieService, postQueue,
             blog, GetApiDownloader(blog, files, null, postQueue, ct) , new Progress<DownloadProgress>(), ct);
    }

    private static NewTumblDownloader GetApiDownloader(IBlog blog, IFiles files, IProgress<DownloadProgress> progress,
        IPostQueue<AbstractPost> postQueue, CancellationToken ct)
    {
var logger = DiServiceLoader.LoggerFactory.CreateLogger("NewDownloader");
var progress2 = new Progress<DownloadProgress>();
        return new NewTumblDownloader(postQueue, blog, files, progress2, logger, ct);
    }


    public static JsonDownloader<T> GetJsonDownloader<T>(
        IPostQueue<CrawlerData<T>> jsonQueue,
        IBlog blog,
        CancellationToken ct)
    {
        return new JsonDownloader<T>(jsonQueue, sro_CrawlerService, blog, ct);
    }


    public static IPostQueue<CrawlerData<T>> GetJsonQueue<T>()
    {
        return new PostQueue<CrawlerData<T>>();
    }


    public static NewTumblDownloader GetNewTumblDownloader(
        IBlog blog,
        IFiles files,
        IProgress<DownloadProgress> progress,
        IPostQueue<AbstractPost> postQueue,
        CancellationToken ct)

    {
        return new NewTumblDownloader(postQueue, blog, files, progress, s_logger, ct);
    }


    public static INewTumblParser GetNewTumblParser()
    {
        return new NewTumblParser();
    }


    public static IPostQueue<AbstractPost> GetProducerConsumerCollection()
    {
        return new PostQueue<AbstractPost>();
    }


    /// <summary>
    ///     Method Sends the reequest and returns the response. Does not retrieve
    ///     the body of the content.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Returns initial response to request. </returns>
    public async Task<HttpResponseMessage> SendHttpRequestAsync(HttpRequestMessage request)
    {
        using HttpClient client = new();
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            s_logger.LogError("ClientSendRequest:Failed.BadResponse");
            return response;
        }

        return response;
    }


    private static HttpClientHandler GetHttpClientHandler()
    {
        HttpClientHandler handler = new()
        {
            AllowAutoRedirect = true,
            CookieContainer = new CookieContainer { Capacity = 100 },
            Credentials = new NetworkCredential("fetishmaster1969@gmail.com", "Angel1031"),
            MaxConnectionsPerServer = 5,
            PreAuthenticate = true,
            UseCookies = true
        };
        return handler;
    }


    /*
            public FileDownloader GetFileDownloader(CancellationToken ct)
            {
                return new FileDownloader(_appSettings, ct);
            }
    */

/*
    private JsonDownloader<T> GetJsonDownloader<T>(
        IPostQueue<CrawlerData<T>> jsonQueue,
        IBlog blog,
        PauseToken pt,
        CancellationToken ct)
    {
        return new JsonDownloader<T>(jsonQueue, _crawlerService, blog, ct);
    }*/


    protected static IFiles GetBlogFiles(IBlog blog)
    {
        s_logger.LogDebug("###########  Completed Files Loader fired #########");
        if (true)
        {
            
            var files = ManagerService.CompletedFilesList.FirstOrDefault(file =>
                file.Name.Equals(blog.Name, StringComparison.Ordinal) && file.BlogType.Equals(blog.OriginalBlogType));
            if (files == null)
            {
                var s = string.Format("Could not load Completed Files db = {0} ({1})", blog.Name, blog.BlogType);
                s_logger.LogError(s);

                App.ShowError(s, "ModuleFactory::GetBlogFiles");
            }

            return files;
        }

        // return Files.Load(blog.ChildId);
    }
}