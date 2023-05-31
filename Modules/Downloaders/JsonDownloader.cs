// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// JsonDownloader.csJsonDownloader.cs032320233:30 AM


using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;
using ScraperOne.Data;
using ScraperOne.DataModels;
using ScraperOne.Services;

namespace ScraperOne.Modules.Downloaders;

public class JsonDownloader<T> : ICrawlerDataDownloader
{
    private readonly IBlog i_blog;
    private readonly ICrawlerService i_crawlerService;
    private readonly IPostQueue<CrawlerData<T>> i_jsonQueue;
    private CancellationToken i_ct;


    public JsonDownloader(
        IPostQueue<CrawlerData<T>> jsonQueue,
        ICrawlerService crawlerService,
        IBlog blog,
        CancellationToken ct)
    {
        i_crawlerService = crawlerService;
        i_blog = blog;
        i_ct = ct;
        i_jsonQueue = jsonQueue;
    }


    /// <summary>
    ///     What are we downloading???
    /// </summary>
    /// <returns></returns>
    public virtual async Task DownloadCrawlerDataAsync()
    {
        List<Task> trackedTasks = new();
        _ = i_blog.CreateDataFolder();
        try
        {
            while (await i_jsonQueue.OutputAvailableAsync(i_ct))
            {
                var downloadItem = await i_jsonQueue.ReceiveAsync();
                if (i_ct.IsCancellationRequested) break;
                trackedTasks.Add(DownloadPostAsync(downloadItem));
            }
        }
        catch (OperationCanceledException e)
        {
            Debug.WriteLine(e.ToString());
        }

        await Task.WhenAll(trackedTasks);
    }


    public void ChangeCancellationToken(CancellationToken ct)
    {
        i_ct = ct;
    }


    private async Task DownloadPostAsync(CrawlerData<T> downloadItem)
    {
        try
        {
            await DownloadTextPostAsync(downloadItem);
        }
        catch
        {
        }
    }


    private async Task DownloadTextPostAsync(CrawlerData<T> crawlerData)
    {
        var blogDownloadLocation = i_blog.DownloadLocation;
        var fileLocation = FileLocation(blogDownloadLocation, crawlerData.Filename);
        await AppendToTextFileAsync(fileLocation, crawlerData.Data);
    }


    private static async Task AppendToTextFileAsync(string fileLocation, T data)
    {
        try
        {
            if (typeof(T) == typeof(Post))
            {
                JsonSerializer serializer = new();
                using StreamWriter sw = new(fileLocation, false);
                using JsonWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented };
                serializer.Serialize(writer, data);
            }
            else
            {
                using FileStream stream = new(fileLocation, FileMode.Create, FileAccess.Write);
                using var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "  ");
                DataContractJsonSerializer serializer = new(data.GetType());
                serializer.WriteObject(writer, data);
                writer.Flush();
            }

            await Task.CompletedTask;
        }
        catch (IOException)
        {
            //_logger.LogError("TumblrJsonDownloader:AppendToTextFile: {0}", ex);
        }
        catch
        {
        }
    }


    private static string FileLocation(string blogDownloadLocation, string fileName)
    {
        return Path.Combine(blogDownloadLocation, fileName);
    }
}