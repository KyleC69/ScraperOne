// 
// Program: Scraper One
// Author:  Kyle Crowder
// License : Open Source
// Portions of code taken from TumblrThree
// 
// 052023

#pragma warning disable CS0649, CS0169


using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

using HttpClientLib;

using KC;

using Microsoft.Extensions.Logging;

using ScraperOne.DataModels;
using ScraperOne.DataModels.Files;
using ScraperOne.Properties;
using ScraperOne.Services;


namespace ScraperOne.Modules.Downloaders;



public abstract class AbstractDownloader : IDisposable
{
    #region Setup/Teardown

    protected AbstractDownloader(
        IPostQueue<AbstractPost> postQueue,
        IBlog blog,
        IFiles files,
        IProgress<DownloadProgress> progress,
        CancellationToken ct,
        ILogger logger)
    {
        _Blog = blog;
        _Ct = ct;
        DownloadQue = postQueue;
        s_progress = progress;
        _Files = files;
        _Logger = logger;
        Client = new ApiClient();
        DownloadComplete += OnDownloadComplete;
        //Periodic timer to save files in case of crash???
        i_saveTimer = new Timer(_ => OnSaveTimedEvent(), null, SaveTimespanSecs * 1000, SaveTimespanSecs * 1000);
    }



    public void Dispose()
    {
        DownloadComplete -= OnDownloadComplete;
        _ = _Blog.Save();
        _ = _Files.Save();
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    private const int SaveTimespanSecs = 120;


    protected static ILogger _Logger;
    private static IProgress<DownloadProgress> s_progress;
    protected readonly IBlog _Blog;
    protected readonly IFiles _Files;

    protected readonly object _LockObjectDownload = new();
    protected readonly IPostQueue<AbstractPost> DownloadQue;
    private readonly object i_diskFilesLock = new();
    private readonly ManagerService i_managerService = ServiceBase.ManagerService;
    private readonly Timer i_saveTimer;
    public event EventHandler<DownloadCompleteEventArgs> DownloadComplete;


    private readonly Dictionary<string, StreamWriterWithInfo> i_streamWriters = new();
    private readonly string[] i_suffixes = { ".jpg", ".jpeg", ".png", ".tiff", ".tif", ".heif", ".heic", ".webp" };
    protected CancellationToken _Ct;
    private readonly string i_baseDir = Environment.CurrentDirectory;
    private SemaphoreSlim concurrentConnectionsSemaphore;
    private SemaphoreSlim concurrentVideoConnectionsSemaphore;
    private HashSet<string> i_diskFiles;
    private volatile bool i_disposed;
    protected ApiClient Client { get; set; }


    public string AppendTemplate { get; set; }



    private void OnDownloadComplete(object sender, DownloadCompleteEventArgs e)
    {
        if (e.Ex is null)
        {
            Console.Write("###  complete  ######");
            Console.WriteLine(e.PathToFile);
        }
        else
        {
            Console.Write("Download Failed = ");
            Console.WriteLine(e.PathToFile);
            Console.WriteLine(e.ResponseMessage.ReasonPhrase);
        }
    }



    public static void UpdateProgressQueueInformation(string format, params object[] args)
    {
        DownloadProgress newProgress = new() { SProgress = string.Format(CultureInfo.CurrentCulture, format, args) };
        s_progress.Report(newProgress);
    }



    public static async Task<Stream> ReadFromUrlIntoStreamAsync(string url)
    {
        HttpRequestMessage request = new(HttpMethod.Get, url);
        HttpClient client = new();
        using HttpResponseMessage response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Stream responseStream = await response.Content.ReadAsStreamAsync();

            return responseStream;
        }

        return null;
    }



    public async Task<bool> DownloadBinaryPost(TumblrPost downloaditem)
    {
        return await DownloadBinaryPostAsync(downloaditem);
    }



    public void ChangeCancellationToken(CancellationToken ct)
    {
        _Ct = ct;
    }



    private void RaiseDownloadComplete(DownloadCompleteEventArgs e)
    {
        DownloadComplete?.Invoke(this, new DownloadCompleteEventArgs());
    }



    /// <summary>
    ///     Method Creates the task to download each post in the blog
    /// </summary>
    /// <returns></returns>
    public virtual async Task<bool> DownloadBlogAsync()
    {
        Debug.WriteLine("Starting DownloadBlogAsync");


        List<Task> trackedTasks = new();
        bool completeDownload = true;
        _ = _Blog.CreateDataFolder();

        await Task.CompletedTask;

        try
        {
            while (await DownloadQue.OutputAvailableAsync(_Ct))
            {
                TumblrPost downloadItem = (TumblrPost)await DownloadQue.ReceiveAsync();

                trackedTasks.Add(DownloadPostAsync(downloadItem));
            }

            _Logger.LogCritical("Download task has completed #########");
        }
        catch (OperationCanceledException)
        {
            Debug.Print("AbstractDownloader::Exceptoin thrown");
            _Logger.LogError("AbstractDownload::DownloadBlogAsync::Error");
            completeDownload = false;
        }
        finally
        {
        }

        try
        {
            await Task.WhenAll(trackedTasks);
        }
        catch
        {
            completeDownload = false;
            Console.WriteLine("DownloadBlogAsync thrown exception");
        }

        Console.WriteLine("Download Tasks finished");
        _Blog.LastDownloadedPhoto = null;
        _Blog.LastDownloadedVideo = null;

        _ = _Files.Save();

        return completeDownload;
    }



    public virtual async Task<string> DownloadPageAsync(string url)
    {
        using Stream s = await ReadFromUrlIntoStreamAsync(url);
        using StreamReader sr = new(s);
        string content = sr.ReadToEnd();

        return content;
    }



    public bool CheckIfFileExistsInDb(string filenameUrl)
    {
        return _Files.CheckIfFileExistsInDb(filenameUrl, false);
    }



    public virtual bool CheckIfPostedUrlIsDownloaded(string url)
    {
        string filenameUrl = url.Split('/').Last();

        return _Files.CheckIfFileExistsInDb(filenameUrl, true);
    }



    /// <summary>
    /// Task wrapper for download core task
    /// Provides exception and Semaphore handling (connection throttling)
    /// </summary>
    /// <param name="downloadItem"></param>
    /// <returns>Task</returns>
    private async Task DownloadPostAsync(TumblrPost downloadItem)
    {
        //standard connections should be double video connections. Video takes one of each sema
        concurrentConnectionsSemaphore = new SemaphoreSlim(App.Settings.ConcurrentConnections);
        concurrentVideoConnectionsSemaphore = new SemaphoreSlim(App.Settings.ConcurrentVideoConnections);

        try
        {
            await concurrentConnectionsSemaphore.WaitAsync();
            if (downloadItem.GetType() == typeof(VideoPost))
            {
                await concurrentVideoConnectionsSemaphore.WaitAsync();
            }

            await DownloadPostCoreAsync(downloadItem, _Ct);
        }
        catch (Exception e)
        {
            _Logger.LogError("AbstractDownloader.DownloadPostAsync: {0}", e);
        }
        finally
        {
            _ = concurrentConnectionsSemaphore.Release();
            if (downloadItem.GetType() == typeof(VideoPost))
            {
                _ = concurrentVideoConnectionsSemaphore.Release();
            }
        }
    }



    /// <summary>
    /// Method employs mechanisms to handle failed downloads by resuming
    /// </summary>
    /// <param name="downloadItem"></param>
    /// <param name="killToken"></param>
    /// <returns></returns>
    private async Task DownloadPostCoreAsync(TumblrPost downloadItem, CancellationToken killToken)
    {
        // TODO: Refactor, should be polymorphism
        DownloadCompleteEventArgs args = new();
        HttpResponseMessage resp;
        string url = downloadItem.Url;
        string dest = Path.Combine(_Blog.FileDownloadLocation, Path.GetFileName(url));
        args.PathToFile = dest;

        bool resuming = File.Exists(dest);
        long? expectedLen;

        HttpRequestMessage req = new(HttpMethod.Get, url);
        if (resuming)
        {
            req.Headers.Range = new RangeHeaderValue(new FileInfo(dest).Length, null);
        }

        using FileStream stream = new(dest, resuming ? FileMode.Append : FileMode.Create);
        try
        {
            resp = await Client.GetAsync(url, HttpCompletionOption.ResponseContentRead, killToken);
            _ = resp.EnsureSuccessStatusCode();
            expectedLen = resp.Content.Headers.ContentLength;
            await resp.Content.CopyToAsync(stream, killToken);

            args.IsComplete = !(new FileInfo(dest).Length < expectedLen);
            args.ResponseMessage = resp;
        }
        catch (Exception ex)
        {
            args.Ex = ex;
            //TODO: catch and retry
            _Logger.LogError("DownloadPostCore threw");
        }
        finally
        {
            RaiseDownloadComplete(args);
        }
    }



    private static async Task<bool> DownloadFileWithResumeAsync(string url, string fileLocation)
    {
        try
        {
            HttpRequestMessage req = new(HttpMethod.Get, url);
            HttpClient client = new();
            using FileStream fstream = new(fileLocation, FileMode.Create, FileAccess.ReadWrite);
            HttpResponseMessage resp = await client.SendAsync(req).ConfigureAwait(false);
            if (resp.IsSuccessStatusCode)
            {
                using Stream stream = await resp.Content.ReadAsStreamAsync();
                await stream.CopyToAsync(fstream);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }



    /*
    private StreamWriterWithInfo GetTextAppenderStreamWriter(string key, bool isJson)
    {
        if (i_streamWriters.TryGetValue(key, out StreamWriterWithInfo value))
        {
            return value;
        }

        StreamWriterWithInfo sw = new(key, true, isJson);
        i_streamWriters.Add(key, sw);
        return sw;
    }

    private void UpdateLinkIfNeededNew(TumblrPost downloadItem)
    {
        _Files.UpdatePostId(downloadItem);
    }


    private void UpdateLinkIfNeeded(bool found, string filename, string filenameOrgUrl)
    {
        if (found && filenameOrgUrl != null)
        {
            // filenameOrgUrl is not equal filename and not found, but filename is found, so update file entry
            _Files.UpdateOriginalLink(filename, filenameOrgUrl);
        }
    }
*/



    private static string PostId(TumblrPost downloadItem)
    {
        return downloadItem.Id;
    }



    protected bool CheckIfLinkRestored(TumblrPost downloadItem)
    {
        if (!_Blog.ForceRescan || _Blog.FilenameTemplate != "%f")
        {
            return false;
        }

        lock (i_diskFilesLock)
        {
            if (i_diskFiles == null)
            {
                i_diskFiles = new HashSet<string>();
                foreach (string item in Directory.EnumerateFiles(_Blog.DownloadLocation, "*",
                             SearchOption.TopDirectoryOnly))
                {
                    if (!string.Equals(Path.GetExtension(item), ".json", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = i_diskFiles.Add(Path.GetFileName(item).ToLower());
                    }
                }
            }

            string filename = downloadItem.Url.Split('/').Last().ToLower();

            return i_diskFiles.Contains(filename);
        }
    }



    /// <summary>
    ///     Method verifies download item and updates tracking files and stats.
    /// </summary>
    /// <param name="downloadItem"></param>
    /// <returns>bool</returns>
    protected virtual async Task<bool> DownloadBinaryPostAsync(TumblrPost downloadItem)
    {
        ApiDownloader.DownloadComplete += OnDownloadComplete;

        string blogDownloadLocation = _Blog.DownloadLocation;
        string fileName = AddFileToDb(downloadItem);
        string fileLocation = Path.Combine(blogDownloadLocation, _Blog.Name, fileName);
        string fileLocationUrlList = FileLocationLocalized(blogDownloadLocation, downloadItem.TextFileLocation);
        DateTime postDate = PostDate(downloadItem);

        try
        {
            if (!await DownloadBinaryFileAsync(fileLocationUrlList, Url(downloadItem)))
            {
                return false;
            }

            // await ApiDownloader.DownloadRemoteFileAsync(downloadItem.Url, fileLocation);
            UpdateProgressQueueInformation(Resources.ProgressDownloadImage, fileName);
            _Blog.Progress++;
        }
        catch (Exception)
        {
            Debug.Print("Exception thrown during download");
            App.ShowError("Download fail");

            return false;
        }
        finally
        {
            UpdateBlogDb(downloadItem.DbType);
        }

        return true;
    }



    protected virtual async Task<bool> DownloadBinaryFileAsync(string fileLocation, string url)
    {
        try
        {
            return await DownloadFileWithResumeAsync(url, fileLocation).ConfigureAwait(false);
        }
        catch (IOException)
        {
            // Disk Full, HRESULT: ‭-2147024784‬ == 0xFFFFFFFF80070070
            _Logger.LogError("AbstractDownloader:DownloadBinaryFile: Disk Full");

            return false;
        }
        catch (WebException webException) when (webException.Response != null)
        {
            int webRespStatusCode = (int)((HttpWebResponse)webException.Response).StatusCode;
            if (webRespStatusCode is >= 400 and < 600) // removes inaccessible files: http status codes 400 to 599
            {
                try
                {
                    File.Delete(fileLocation);
                } // could be open again in a different thread
                catch
                {
                }
            }

            return false;
        }
        catch (TimeoutException)
        {
            _Logger.LogError("AbstractDownloader:DownloadBinaryFile timeout exception");

            //throw new TaskCanceledException();
            return false;
        }
        finally
        {
            _ = concurrentConnectionsSemaphore.Release();
            _ = concurrentVideoConnectionsSemaphore.Release();
        }
    }



/*
        /// <summary>
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <param name="text"></param>
        /// <param name="isJson"></param>
        /// <returns></returns>
        protected virtual bool AppendToTextFile(string fileLocation, string text, bool isJson)
        {
            try
            {
                lock (_LockObjectDownload)
                {
                    StreamWriterWithInfo sw = GetTextAppenderStreamWriter(fileLocation, isJson);
                    sw.WriteLine(text);
                }

                return true;
            }
            catch (IOException)
            {
                _Logger.LogError("Downloader:AppendToTextFile: IO Exceptoni");
                return false;
            }
            catch
            {
                return false;
            }
        }

*/
    protected string AddFileToDb(TumblrPost downloadItem)
    {
        _ = _Files.AddFileToDb(FileNameUrl(downloadItem), FileNameOriginalUrl(downloadItem), downloadItem.Filename,
            PostId(downloadItem));
        _ = _Files.Save();

        return downloadItem.Filename;
    }



/*
        protected bool CheckIfFileExistsInDb(TumblrPost downloadItem)
        {
            bool found;
            string filenameOrgUrl = string.IsNullOrEmpty(downloadItem.PostedUrl) ? null : FileNameOriginalUrl(downloadItem);
            if (string.IsNullOrEmpty(downloadItem.PostedUrl))
            {
                filenameOrgUrl = FileNameOriginalUrl(downloadItem);
            }

            string filename = FileNameUrl(downloadItem);


            if (App.Settings.LoadAllDatabases)
            {
                found = i_managerService.CheckIfFileExistsInDbNew(downloadItem);
                UpdateLinkIfNeededNew(downloadItem);
                return found;
            }


            found = _Files.CheckIfFileExistsInDb(filename, false);
            UpdateLinkIfNeeded(found, filename, filenameOrgUrl);
            return found;
        }
*/



    /// <summary>
    ///     Increments the counts and progress bar
    /// </summary>
    /// <param name="postType"></param>
    protected void UpdateBlogDb(string postType)
    {
        _Blog.UpdatePostCount(postType);
        _Blog.UpdateProgress(true);
    }



    protected void SetFileDate(string fileLocation, DateTime postDate)
    {
        if (_Blog.DownloadUrlList)
        {
            return;
        }
        //File.SetLastWriteTime(fileLocation, postDate);
    }



    protected static string Url(TumblrPost downloadItem)
    {
        return downloadItem.Url;
    }



    protected virtual string FileNameUrl(TumblrPost downloadItem)
    {
        return downloadItem.Url?.Split('/').Last();
    }



    protected virtual string FileNameOriginalUrl(TumblrPost downloadItem)
    {
        return downloadItem.PostedUrl?.Split('/').Last() ?? downloadItem.Url;
    }



    protected virtual string FileName(TumblrPost downloadItem)
    {
        string filename = downloadItem.Url.Split('/').Last();
        if (Path.GetExtension(filename).ToLower() == ".gifv")
        {
            filename = Path.GetFileNameWithoutExtension(filename) + ".gif";
        }

        if (Path.GetExtension(filename).ToLower() == ".pnj")
        {
            filename += ".png";
        }

        return filename;
    }



    protected static string FileNameNew(TumblrPost downloadItem)
    {
        return downloadItem.Filename;
    }



    protected static string FileLocation(string blogDownloadLocation, string fileName)
    {
        return Path.Combine(blogDownloadLocation, fileName);
    }



    protected static string FileLocationLocalized(string blogDownloadLocation, string fileName)
    {
        return Path.Combine(blogDownloadLocation, string.Format(CultureInfo.CurrentCulture, fileName));
    }



    protected static DateTime PostDate(TumblrPost downloadItem)
    {
        if (string.IsNullOrEmpty(downloadItem.Date))
        {
            return DateTime.Now;
        }

        DateTime epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        DateTime postDate = epoch.AddSeconds(Convert.ToDouble(downloadItem.Date)).ToLocalTime();

        return postDate;
    }



    protected bool CheckIfShouldStop()
    {
        return _Ct.IsCancellationRequested;
    }



    protected void OnSaveTimedEvent()
    {
        if (i_disposed)
        {
            return;
        }

        try
        {
            _ = i_saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if (_Files != null && _Files.IsDirty)
            {
                _ = _Files.Save();
            }
        }
        catch (Exception)
        {
            // progress.Report(e);
        }
        finally
        {
            if (!i_disposed)
            {
                _ = i_saveTimer.Change(SaveTimespanSecs * 1000, SaveTimespanSecs * 1000);
            }
        }
    }



    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            i_disposed = true;
            _ = i_saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            i_saveTimer.Dispose();
            concurrentConnectionsSemaphore?.Dispose();
            concurrentVideoConnectionsSemaphore?.Dispose();
            foreach (StreamWriterWithInfo sw in i_streamWriters.Values)
            {
                sw.Dispose();
            }
        }
    }
}