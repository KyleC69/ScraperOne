// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ManagerService.csManagerService.cs032420235:08 PM

#pragma warning disable CS0169, CS0649

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Diagnostics;
using DynamicData;
using JetBrains.Annotations;
using KC;
using KC.Models;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using ScraperOne.DataModels;
using ScraperOne.DataModels.Files;
using ScraperOne.Modules.Crawlers;

namespace ScraperOne.Services
{
    [DebuggerDisplay("count={i_counter}")]
    public class ManagerService : ServiceBase
    {
        public delegate void BlogManagerAppStartupFinishedHandler([CanBeNull] object sender, EventArgs e);

        public delegate void BlogManagerFinishedLoadingDatabasesHandler([CanBeNull] object sender, EventArgs e);

        public delegate void BlogManagerFinishedLoadingLibraryHandler([CanBeNull] object sender, EventArgs e);

        public delegate void FinishedCrawlingLastBlogEventHandler(object sender, EventArgs e);

        public static TaskCompletionSource<bool> ManagerServiceStartupComplete;
        private static ILogger s_logger;
        private static readonly CancellationToken ct;
        private readonly int i_counter;
        private readonly ReaderWriterLockSlim i_databasesLock = new();
        private readonly object i_lockObject = new();


        //-----------  SYNCRONIZED BLOG DATA COLLECTIONS  -----------------------
        // Private var to receive the change updates
        private ReadOnlyObservableCollection<IBlog> _derived;


        public ManagerService(ILogger logger)
        {
            i_counter++;
            s_logger = logger;
            s_logger.LogInformation("Manager Services has been started");

            DatabasesLoaded = new();
            LibraryLoaded = new();
            ManagerServiceLoaded = new();
            ManagerServiceStartupComplete = new TaskCompletionSource<bool>();

            CompletedFilesList = new ObservableCollection<IFiles>();

            BlogManagerFinishedLoadingDatabases += OnBlogManagerFinishedLoadingDatabases;
            BlogManagerFinishedLoadingLibrary += OnBlogManagerFinishedLoadingLibrary;
            BlogManagerAppStartupFinished += OnAppStartupFinished;
            Source = new();
        }

        // Property to share and bind to
        public ReadOnlyObservableCollection<IBlog> BlogFiles => _derived;

        // Thread safe source collection to make changes to and 
        [Reactive] public static SourceList<IBlog> Source { get; set; }

        //--------------  /dynamic Data experiment  -----------------
        public static ObservableCollection<IBlog> sBlogFiles { get; set; }

        //---------------- MEDIA ID DATA COLLECTOINS  ------------------
        public static ObservableCollection<IFiles> CompletedFilesList { get; private set; }

        public static TaskCompletionSource<bool> ManagerServiceLoaded { get; set; }

        public static TaskCompletionSource<bool> DatabasesLoaded { get; set; }

        public static TaskCompletionSource<bool> LibraryLoaded { get; set; }

        public IObservable<IChangeSet<IBlog>> IBlogConnection() => Source.Connect();


        private void OnAppStartupFinished(object sender, EventArgs e)
        {
            var derived = IBlogConnection()
                .Bind(out _derived).Subscribe();
        }

        public event FinishedCrawlingLastBlogEventHandler FinishedCrawlingLastBlog;


        public void AddFileDatabase(IFiles database)
        {
            i_databasesLock.EnterWriteLock();
            try
            {
                CompletedFilesList.Add(database);
            }
            finally
            {
                i_databasesLock.ExitWriteLock();
            }
        }


        public void ClearDatabases()
        {
            i_databasesLock.EnterWriteLock();
            try
            {
                CompletedFilesList.Clear();
            }
            finally
            {
                i_databasesLock.ExitWriteLock();
            }
        }


        public async Task<IBlog> AddBlogAsync(string blogUrl)
        {
            IBlog blog = await CheckIfCrawlableBlog(blogUrl);


            lock (i_lockObject)
            {
                //if (CheckIfBlogAlreadyExists(blog)) return blog;

                blog = TransferGlobalSettingsToBlog(blog);
                EnsureUniqueFolder(blog);
                SaveBlog(blog);
            }

            //  await UpdateMetaInformationAsync(blog);
            return blog;
        }


        /// <summary>
        ///     Loads the IBlog and IFile objects from disk
        /// </summary>
        /// <returns>Task</returns>
        public async Task LoadAllDatabasesAsync()
        {
            try
            {
                s_logger.LogDebug("Starting LoadDataBasesAsync**************");
                await LoadBlogNamesAsync();

                await LoadCompletedFilesDatabaseAsync();

                // _ = CheckIfDatabasesComplete();

                _ = await Task.WhenAll(LibraryLoaded.Task, DatabasesLoaded.Task);

                s_logger.LogDebug("Ending LoadDataBasesAsync ********************");
            }
            catch (Exception e)
            {
                s_logger.LogError($"ManagerService.LoadDataBasesAsync: {e.Message}");
                Debug.Write(e.Message);
            }
        }


        /// <summary>
        ///     Loads a list of files already downloaded
        /// </summary>
        /// <returns>Task</returns>
        public Task LoadCompletedFilesDatabaseAsync()
        {
            //TODO: Converting dupe checks to media index dictionary
            s_logger.LogDebug("ManagerController.LoadCompletedFilesListAsync:Start");
            ClearDatabases();
            string path = App.Settings.BlogIndexLocation;
            if (Directory.Exists(path))
            {
                IReadOnlyList<IFiles> completedlist = GetIFiles(path, false);
                foreach (IFiles database in completedlist)
                {
                    AddFileDatabase(database);
                }
            }

            if (CompletedFilesList.Count == 0)
            {
                App.ShowError("Completed files list ws not loaded");
            }

            BlogManagerFinishedLoadingDatabases?.Invoke(this, EventArgs.Empty);
            s_logger.LogDebug("ManagerService::LoadCompletedFilesListAsync::End-----------------------------");

            return Task.CompletedTask;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="blogFiles"></param>
        public static void Enqueue(IEnumerable<IBlog> blogFiles)
        {
            QueueManager.AddItems(blogFiles.Select(x => new QueueListItem(x)));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="iBlog"></param>
        public static void Enqueue(IBlog iBlog)
        {
            QueueListItem blg = new(iBlog);
            if (blg.Blog is null)
            {
                App.ShowError("Error adding item to queuue", "failed to add item");

                return;
            }

            QueueManager.AddItem(blg);
        }


        /// <summary>
        /// 
        /// </summary>
        public static void EnqueueAutoDownload()
        {
            s_logger.LogDebug("Starting Enqueue Auto Download Items");
            //   Enqueue(BlogFiles);

            //  CrawlerService.CrawlCommand.CanExecute(null);
            //    CrawlerService.CrawlCommand.Execute(null);


            //Enqueue(ManagerService.BlogFiles.Where(blog => blog.Online && blog.LastCompleteCrawl == new DateTime(0L, DateTimeKind.Utc)).ToArray());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task ImportFollowedBlogsAsync()
        {
            string token = CookieService.GetCookieByName("LoginToken").Value;
            Guard.IsNotNull(token, nameof(token));

            using ApiController api = new(token, 1616730);

            List<ApiBlog> followed = await api.GetFollowedBlogs(null);
            foreach (ApiBlog nb in followed)
            {
                IBlog blog = await AddBlogAsync(nb.BlogUrl);
                blog.DateAdded = DateTime.Now;
                blog.Description = nb.BlogDescription;
                blog.DwBlogIx = Convert.ToInt32(nb.BlogId);
                blog.DwUserIx = nb.UserId;
                _ = blog.Save();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public static Task<IReadOnlyList<IBlog>> GetIBlogsAsync(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new ApplicationException("Could not locate index directory");
            }

            try
            {
                var bfiles = GetIBlogsCore(directory);
                return Task.FromResult(bfiles);
            }
            catch (Exception e)
            {
                s_logger.LogError($"ManagerService::GetIBlogsAsync::{e.Message}");
                throw new ApplicationException("An error occured while retrieving blog data from disk");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloadItem"></param>
        /// <returns></returns>
        public bool CheckIfFileExistsInDbNew(TumblrPost downloadItem)
        {
            i_databasesLock.EnterReadLock();
            try
            {
                foreach (IFiles database in CompletedFilesList)
                {
                    IEnumerable<FileEntry> any = database.Entries.Where(x => x.PostId == downloadItem.Id);
                    {
                        if (any.Any())
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                i_databasesLock.ExitReadLock();
            }

            return false;
        }


        void UpdateStatus()
        {
        }

        public async Task UpdateMetaInformationAsync()
        {
            ClearBlogStatus();
            foreach (var blog in BlogFiles)
            {
                var progress = new Progress<string>(s => { blog.LastStatus = s; });
                NewTumblCrawler crawler = ModuleFactory.GetCrawler(blog, null, ct);
                try
                {
                    await crawler.UpdateMetaInformationAsync(progress);
                }
                finally
                {
                    crawler?.Dispose();
                }
            }

            await Task.Delay(20);
        }

        private void ClearBlogStatus()
        {
            foreach (var blog in BlogFiles)
            {
                blog.LastStatus = String.Empty;
                blog.Progress = 0;
                blog.TotalCount = 0;
                blog.DuplicatePhotos = 0;
                blog.DuplicateVideos = 0;
                blog.DownloadedVideos = 0;
                blog.DownloadedPhotos = 0;
                blog.Videos = 0;
                blog.Photos = 0;
                blog.DownloadedItemsNew = 0;
            }
        }


        /*
                private void ManagerViewSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        Debugger.Break();
                    }

                    if (sender.Equals(BlogFiles))
                    {
                        if (e.Action != NotifyCollectionChangedAction.Add)
                        {
                            App.ShowInfo("BlogFilesView collection has been decreased in count from init.");
                        }
                    }
                }
        */


        public static event BlogManagerFinishedLoadingLibraryHandler BlogManagerFinishedLoadingLibrary;
        public static event BlogManagerFinishedLoadingDatabasesHandler BlogManagerFinishedLoadingDatabases;
        public static event BlogManagerAppStartupFinishedHandler BlogManagerAppStartupFinished;


        private void OnBlogManagerFinishedLoadingLibrary(object sender, EventArgs e)
        {
            LibraryLoaded.TrySetResult(true);
        }


        private void OnBlogManagerFinishedLoadingDatabases(object sender, EventArgs e)
        {
            DatabasesLoaded.TrySetResult(true);
        }


        private void QueueItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove)
            {
                //QueueItems = QueueManager.Items;
                if (e.Action == NotifyCollectionChangedAction.Remove && QueueManager.Items.Count == 0)
                {
                    FinishedCrawlingLastBlogEventHandler handler = FinishedCrawlingLastBlog;
                    handler?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        public static IBlog GetBlog(string url, string location, string filenameTemplate = "%f")
        {
            return NewTumblBlog.Create(url, location, filenameTemplate);
        }


        public static void EnsureUniqueFolder(IBlog blog)
        {
            _ = blog.Name;
            /*  while (BlogFiles.Any(b => b.DownloadLocation == blog.DownloadLocation + appendix) ||
                     Directory.Exists(blog.DownloadLocation + appendix))
              {
                  // number++;
                  // appendix = $"_{number}";
              }*/
            // blog.FileDownloadLocation = Path.Combine(blog.DownloadLocation, appendix);
            _ = Directory.CreateDirectory(blog.FileDownloadLocation);
        }


        /// <summary>
        /// </summary>
        /// <param name="blog"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static IFiles LoadFilesAsync(IBlog blog)
        {
            IFiles files = CompletedFilesList.FirstOrDefault(file => file.Name.Equals(blog.Name));

            if (files == null)
            {
                string s = string.Format("{0} ({1})", blog.Name, blog.BlogType);
                s_logger.LogDebug($"Completed Files List empty for blog {blog.Name}");
            }

            return Files.Load(blog.ChildId);
        }


        private static IBlog TransferGlobalSettingsToBlog(IBlog blog)
        {
            blog.DownloadAudio = App.Settings.DownloadAudios;
            blog.DownloadPhoto = App.Settings.DownloadPhotos;
            blog.DownloadVideo = App.Settings.DownloadVideos;
            blog.DownloadText = App.Settings.DownloadTexts;
            blog.DownloadAnswer = blog.BlogType == BlogTypes.Newtumbl;
            blog.DownloadQuote = App.Settings.DownloadQuotes;
            blog.DownloadConversation = App.Settings.DownloadConversations;
            blog.DownloadLink = App.Settings.DownloadLinks;
            blog.CreatePhotoMeta = App.Settings.CreateImageMeta;
            blog.CreateVideoMeta = App.Settings.CreateVideoMeta;
            blog.CreateAudioMeta = App.Settings.CreateAudioMeta;
            blog.SkipGif = App.Settings.SkipGif;
            blog.DownloadVideoThumbnail = App.Settings.DownloadVideoThumbnails;
            blog.DownloadRebloggedPosts = App.Settings.DownloadRebloggedPosts;
            blog.ForceSize = App.Settings.ForceSize;
            blog.ForceRescan = App.Settings.ForceRescan;
            blog.CheckDirectoryForFiles = App.Settings.CheckDirectoryForFiles;
            blog.DownloadUrlList = App.Settings.DownloadUrlList;
            blog.DownloadPages = App.Settings.DownloadPages;
            blog.PageSize = App.Settings.PageSize;
            blog.DownloadFrom = App.Settings.DownloadFrom;
            blog.DownloadTo = App.Settings.DownloadTo;
            blog.Tags = App.Settings.Tags;
            //blog.FileDownloadLocation = App.Settings.DownloadLocation;
            blog.FilenameTemplate = App.Settings.FilenameTemplate;
            blog.CollectionId = App.Settings.ActiveCollectionId;

            return blog;
        }


        /// <summary>
        ///     Gets collection of blogs currently being managed by ScraperOno
        ///     Enumerates files in a given path fitting format expected
        ///     Path is set in App.Settings BlogIndexLocation property
        /// </summary>
        /// <param name="directory">Path to location of Index files</param>
        /// <returns>ReadonlyList  </returns>
        private static IReadOnlyList<IBlog> GetIBlogsCore(string directory)
        {
            s_logger.LogDebug("ManagerService::GetIBlogsAsync::GetIBlogsCore::BeginMethod");
            List<IBlog> blogs = new();
            List<string> failedToLoadBlogs = new();
            string[] supportedFileTypes = Enum.GetNames(typeof(BlogTypes)).ToArray();
            foreach (string filename in Directory.GetFiles(directory, "*").Where(
                         fileName => supportedFileTypes.Any() && !fileName.Contains("_files")))
            {
                //TODO: Refactor
                IBlog blog = null;
                try
                {
                    if (filename.EndsWith(BlogTypes.Newtumbl.ToString()))
                    {
                        blog = new NewTumblBlog().Load(filename);
                    }

                    if (blog != null)
                    {
                        /*if (!validCollectionIds.Contains(blog.CollectionId))
           blog.CollectionId = 0;
       */
                        blogs.Add(blog);
                    }
                }
                catch (SerializationException ex)
                {
                    if (blog != null)
                    {
                        blog.LoadError = ex;
                    }

                    failedToLoadBlogs.Add(ex.Data["Filename"]?.ToString());
                }
            }

            if (failedToLoadBlogs.Any())
            {
                string failedBlogNames = failedToLoadBlogs.Aggregate((a, b) => a + ", " + b);
                s_logger.LogDebug("ManagerController:GetIBlogsCore: {name}", failedBlogNames);
            }

            //_logger.LogDebug("ManagerController.GetIBlogsCore End");
            return blogs;
        }


        private void SaveBlog(IBlog blog)
        {
            if (blog.Save())
            {
                AddToManager(blog);
            }
        }


        private void AddToManager(IBlog blog)
        {
            Source.Add(blog);
            AddFileDatabase(Files.Load(blog.ChildId));
        }


        private static void ReportProgress(DownloadProgress item)
        {
            App.ShowInfo($"progress {item.Progress}");
        }

        private static async Task<IBlog> CheckIfCrawlableBlog(string blogUrl)
        {
            //return TumblrBlog.Create(blogUrl, GetIndexFolderPath(), _appSettings.FilenameTemplate, true);
            IBlog newblog = NewTumblBlog.Create(blogUrl, GetIndexFolderPath(), "%f");
            await Task.Delay(20);

            // var factory = new BlogFactory(null);
            // return factory.GetBlog(blogUrl, GetIndexFolderPath(_appSettings.ActiveCollectionId), _appSettings.FilenameTemplate);
            return newblog;
        }


        private static string GetIndexFolderPath()
        {
            return string.IsNullOrEmpty(App.Settings.DownloadLocation)
                ? Path.Combine(Environment.CurrentDirectory, "Index")
                : Path.Combine(App.Settings.DownloadLocation, "Index");
        }


        private IReadOnlyList<IFiles> GetIFiles(string directory, bool isArchive)
        {
            return GetIFilesCoreAsync(directory, isArchive);
        }


        private IReadOnlyList<IFiles> GetIFilesCoreAsync(string directory, bool isArchive)
        {
            s_logger.LogDebug("ManagerController:GetFilesCore Start");
            List<IFiles> databases = new();
            // if (!_appSettings.LoadAllDatabases) return Task.FromResult(databases); // Added line to prevent unnecessary processing.
            List<string> failedToLoadDatabases = new();
            string[] supportedFileTypes = Enum.GetNames(typeof(BlogTypes)).ToArray();
            IEnumerable<string> filenames = Directory.GetFiles(directory, "*").Where(fileName =>
                supportedFileTypes.Any(fileName.Contains) && fileName.Contains("_files"));
            IFiles database;

            if (filenames is null)
            {
                throw new ArgumentException(nameof(filenames));
            }

            foreach (string filename in filenames)
            {
                //TODO: Refactor
                try
                {
                    database = Files.Load(filename, isArchive);
                    if (App.Settings.LoadAllDatabases)
                    {
                        databases.Add(database);
                    }
                }
                catch (Exception ex) when (ex is SerializationException or FileNotFoundException or IOException)
                {
                    failedToLoadDatabases.Add(ex.Data["Filename"].ToString());
                    s_logger.LogDebug("Error in GetFilesCore Method");
                }
            }

            if (failedToLoadDatabases.Any())
            {
                IEnumerable<IBlog> blogs = BlogFiles;
                IEnumerable<IBlog> failedToLoadBlogs =
                    blogs.Where(blog => failedToLoadDatabases.Contains(blog.ChildId)).ToList();
                string failedBlogNames = failedToLoadDatabases.Aggregate((a, b) => a + ", " + b);
                s_logger.LogDebug("ManagerController:GetIFilesCore: {failed}", failedBlogNames);
                foreach (IBlog failedToLoadBlog in failedToLoadBlogs)
                {
                    _ = Source.Remove(failedToLoadBlog);
                }
            }

            s_logger.LogDebug("ManagerController.GetFilesCore End");

            return databases;
        }


        /// <summary>
        ///     Method populates the Sourcelist Property 
        ///     Changes are streamed to subscribers
        /// </summary>
        /// <returns></returns>
        private async Task LoadBlogNamesAsync()
        {
            Source.Clear();

            List<IBlog> blogs = (List<IBlog>)await GetIBlogsAsync(GetIndexFolderPath());
            foreach (var blog in blogs)
            {
                Source.Add(blog);
            }


            s_logger.LogDebug($"Source count={Source.Count}");

            //  Debug.Assert(BlogFiles.Count > 0, "Database did not load properly");
            ;
            BlogManagerFinishedLoadingLibrary.Invoke(this, EventArgs.Empty);

            s_logger.LogDebug("ManagerService::LoadBlogData::Complete-----------------------------");
        }


        public static async void OnAppStartup(object sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            //perform any initialization needed for module
            s_logger.LogInformation("MANAGER.SERVICE.LOADING.DATABASES");
            await ManagerService.LoadAllDatabasesAsync();

            //Wait for application catches up and databases have finished before
            //starting the crawler
            s_logger.LogInformation("MANAGER.SERVICE::INIT::TaskWait");
            _ = await Task.WhenAll(LibraryLoaded.Task, DatabasesLoaded.Task);

            //  s_logger.LogInformation("MANAGER.SERVICE.DATABASES.ARE.LOADED");

            // Pause and wait for other start up methods to finish before
            // Continuing and checking the status of the blogs.
            // _ = await Task.WhenAll(DiServiceLoader.DIServiceAppStartupComplete.Task, CookieService.CookieLoadingComplete.Task);
            //  s_logger.LogInformation("MANAGER.SERVICE::ServiceLoader and Cookies have been loaded");


            //  ManagerServiceStartupComplete.SetResult(true);
            s_logger.LogInformation("MANAGER::SERVICE::APPStartup::Ended");

            BlogManagerAppStartupFinished?.Invoke(null, EventArgs.Empty);
        }
    }
}