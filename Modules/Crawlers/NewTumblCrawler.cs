using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HttpClientLib;
using KC;
using Newtonsoft.Json;
using ScraperOne.Data;
using ScraperOne.DataModels;
using ScraperOne.DataModels.NewTumbl;
using ScraperOne.Logger;
using ScraperOne.Modules.Downloaders;
using ScraperOne.Properties;
using ScraperOne.Services;
using Splat;
using static KC.ApiController;


namespace ScraperOne.Modules.Crawlers
{
    public class NewTumblCrawler : AbstractCrawler, ICrawler
    {
        private static readonly ColorLogger sro_Logger =
            (ColorLogger)ServiceBase.LoggerFactory.CreateLogger("NewTumblCrawler");


        private readonly string[] cookieHosts = { "https://newtumbl.com/" };

        private readonly string[] i_aExt = { "", "html", "jpg", "png", "gif", "mp3", "mp4", "mov" };

        private readonly string[] i_cookieHosts = { "https://newtumbl.com/" };

        //private readonly ICrawlerDataDownloader i_crawlerDataDownloader;
        private readonly NewTumblDownloader i_downloader;
        private readonly IList<string> i_existingCrawlerData = new List<string>();
        private readonly object i_existingCrawlerDataLock = new();
        private readonly SettingsProvider i_provider;
        private long blogIx;
        private string dtSearchField;
        private bool GrabComplete;

        private ulong highestId;
        private bool incompleteCrawl;
        private int pageNo;

        private SemaphoreSlim semaphoreSlim;
        private int totalPosts;

        private List<Task> trackedTasks;


        #region Setup/Teardown

        public NewTumblCrawler(
            ICrawlerService crawlerService,
            IWebRequestFactory webRequestFactory,
            CookieService cookieService,
            IPostQueue<AbstractPost> downloadQue,
            IBlog blog,
            NewTumblDownloader downloader,
            // ICrawlerDataDownloader crawlerDataDownloader,
            //  INewTumblParser newTumblParser,
            IProgress<DownloadProgress> progress,
            CancellationToken ct) : base(crawlerService, webRequestFactory, cookieService, downloadQue, blog,
            downloader,
            sro_Logger, progress, ct)
        {
            Ct = ct;
            i_downloader = downloader;
            // this.downloader.ChangeCancellationToken(Ct);
            //  i_jsonQueue = jsonQueue;
            //   i_crawlerDataDownloader = crawlerDataDownloader;
            //   i_crawlerDataDownloader.ChangeCancellationToken(Ct);
            //   i_newTumblParser = newTumblParser;
            i_provider = new SettingsProvider();
            LoginToken = CookieService.GetLoginToken;
            ApiDownloader.DownloadComplete += (sender, args) => App.ShowInfo($"Download State is {args.IsComplete}");
        }

        #endregion

        private IBlogService _blogService => Locator.Current.GetService<BlogService>();

        public IBlogService BlogService => _blogService;


        public async Task CrawlAsync()
        {
            try
            {
                await CrawlAsync2();
            }
            catch (Exception ex)
            {
                Console.WriteLine("catchall");
                Console.WriteLine(ex.Message);
            }
        }


        public async Task<string> GetApiPageAsync(int mode)
        {
            switch (mode)
            {
                case 0:
                    return string.Empty;

                case 1:
                    if (Blog.DownloadFrom is not null)
                    {
                        dtSearchField = DateTime
                            .ParseExact(Blog.DownloadFrom, "yyyyMMdd", CultureInfo.InvariantCulture,
                                DateTimeStyles.None)
                            .ToString("yyyy-MM-ddTHH:mm:ss");
                    }

                    IEnumerable<Cookie> cookies = CookieService.GetAllCookies();
                    Cookie cookie = cookies.FirstOrDefault(c => c.Name == "Affinity");
                    string affinity = cookie?.Value ?? "";
                    cookie = cookies.FirstOrDefault(c => c.Name == "LoginToken");
                    string token = cookie?.Value ?? "";
                    cookie = cookies.FirstOrDefault(c => c.Name == "ActiveBlog");
                    long activeBlog = long.TryParse(cookie?.Value, out long result) ? result : 0;


                    ApiController api = new(LoginToken, (int)activeBlog);
                    //    string parms = api.BuildParams(ApiCall.SearchResults, Blog.DwBlogIx,dtSearchField, pageNum: pageNo);
                    ApiHttpRequest req = api.BuildHttpRequestMessage(EnumApiEndPoints.search_Blog_Posts,
                        blogIx.ToString(), Blog.Url);
                    //_ = req.SetContentHeader(parms);

                    return await api.SendApiPostRequestAsync(req);
                default:
                    break;
                //var results =  await PostDataAsync(url, Blog.Url, d, cookieHosts);
                // return results;
            }

            return string.Empty;
        }


        public async Task<string> GetBasicBlogInfo(string url)
        {
            return await GetBasicBlogInfoCoreAsync(url);
        }


        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            GC.Collect(0, GCCollectionMode.Default, true, true);
        }


        Task<Root> ICrawler.DoApiGetRequest()
        {
            throw new NotImplementedException();
        }


        Task<List<ARow>> ICrawler.GetFollowedBlogsAsync()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Method starts retrieving posts using API Requests.
        /// </summary>
        public async Task CrawlAsync2()
        {
            sro_Logger.LogDebug("NewTumblCrawler.Crawl:Start");
            sro_Logger.LogDebug($"Starting blog-- {Blog.Name}");

            Task<bool> grabber = GetUrlsAsync();

            Task<bool> download = i_downloader.DownloadBlogAsync();

            bool errorsOccurred = await grabber;


            //UpdateProgressQueueInformation(Resources.ProgressUniqueDownloads);
            if (!errorsOccurred && (Blog.ForceRescan || Blog.TotalCount == 0))
            {
                Blog.Posts = totalPosts;
            }

            Blog.DuplicatePhotos = DetermineDuplicates<PhotoPost>();
            Blog.DuplicateVideos = DetermineDuplicates<VideoPost>();
            Blog.TotalCount = Blog.TotalCount - Blog.DuplicatePhotos - Blog.DuplicateVideos;

            ClearCollectedBlogStatistics();

            bool finishedDownloading = await download;

            if (finishedDownloading && !errorsOccurred)
            {
                Blog.LastId = highestId;
            }

            _ = Blog.Save();
            // UpdateProgressQueueInformation();
        }


        public async Task<Root> DoApiGetRequest()
        {
            Root obj = null;
            for (int retries = 0; retries < 2; retries++)
            {
                try
                {
                    string document = await BasicInfoGetRequest(Blog.Url);
                    string json = sro_ExtractJsonFromPage.Match(document).Groups[1].Value + "}";
                    obj = ConvertJsonToClassNew<Root>(json);

                    return obj;
                }
                catch (APIException ex)
                {
                    if (retries == 0)
                    {
                        sro_Logger.LogError($"GetApiPageAsync, retrying: {ex.Message}");
                        await Task.Delay(10000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (obj?.aResultSet[2].aRow.Count == 0)
            {
                App.ShowError("API reports not found??");
            }

            if (obj?.aResultSet[3].aRow[0].bLoggedIn == 0)
            {
                App.ShowError("API::bLoggedIn reports zero");
            }

            blogIx = blogIx > 0 ? blogIx : BlogInfo(obj, 2)?.dwBlogIx ?? 0;

            return obj;
        }


        public static Task GetBlogSettings()
        {
            if (!Blog.Online)
            {
                return Task.CompletedTask;
            }

            ApiController api = new(LoginToken);
            //string par = api.BuildParams(ApiCall.GetBlogSettings, Blog.DwBlogIx);
            //  string settings = await api.GetBlogSettings(par);
            return Task.CompletedTask;
        }


        public static async Task<string> GetBlogMarquee()
        {
            if (!Blog.Online)
            {
                return "";
            }

            ApiController api = new();

            ApiHttpRequest req = api.BuildHttpRequestMessage(EnumApiEndPoints.get_Blog_Marquee);
            //_ = req.SetContent(api.BuildParams(ApiCall.GetMarquee, 0));
            req.Headers.Referrer = new Uri(Blog.Url);

            string result = await api.DoApiCall(req);
            Root obj = JsonConvert.DeserializeObject<Root>(result);

            int? blogIx = obj.aResultSet[2].aRow[0].dwBlogIx;
            int user = obj.aResultSet[2].aRow[0].dwUserIx;

            return result;
        }


        private async Task<string> OldApiCallsTesting()
        {
            IEnumerable<Cookie> cookies = CookieService.GetAllCookies();
            Cookie cookie = cookies.FirstOrDefault(c => c.Name == "Affinity");
            string affinity = cookie?.Value ?? "";
            cookie = cookies.FirstOrDefault(c => c.Name == "LoginToken");
            string token = cookie?.Value ?? "";
            cookie = cookies.FirstOrDefault(c => c.Name == "ActiveBlog");
            long activeBlog = long.TryParse(cookie?.Value, out long result) ? result : 0;

            string url = "https://api-ro.newtumbl.com/sp/NewTumbl/" + "search_Blog_Posts" + $"?affinity={affinity}";

            Dictionary<string, string> d = new Dictionary<string, string>
            {
                {
                    "json",
                    "{\"Params\":[\"[{IPADDRESS}]\",\"" + token + "\",null,\"0\",\"0\"," + activeBlog + ",null," +
                    (dtSearchField == null ? "null" : $"\"{dtSearchField}\"") + "," + pageNo +
                    ",50,0,null,0,\"\",0,0,0,0,0,null,null]}"
                }
            };

            return await PostDataAsync(url, Blog.Url, d, cookieHosts);
        }


        public async Task<string> GetApiPageAsync2(int mode)
        {
            sro_Logger.LogDebug("GetAPIPage::Starting::");

            switch (mode)
            {
                case 0:
                    sro_Logger.LogDebug("GetAPIPage::Starting::MODE=0");
                    string json = null;
                    Root obj = null;
                    for (int numberOfTrials = 0; numberOfTrials < 2; numberOfTrials++)
                    {
                        try
                        {
                            string document = await RequestDataAsync(Blog.Url, null, i_cookieHosts);
                            json = sro_ExtractJsonFromPage.Match(document).Groups[1].Value + "}";
                            obj = ConvertJsonToClassNew<Root>(json);
                            CheckError(obj);

                            break;
                        }
                        catch (APIException ex)
                        {
                            if (numberOfTrials == 0)
                            {
                                sro_Logger.LogError($"GetApiPageAsync, retrying: {ex.Message}");
                                await Task.Delay(10000);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    if (obj?.aResultSet[2]?.aRow.Count == 0)
                    {
                        Console.WriteLine("API reports not found??");

                        throw CreateWebException(HttpStatusCode.NotFound);
                    }

                    if (obj?.aResultSet[3].aRow[0].bLoggedIn == 0)
                    {
                        Console.WriteLine("API::bLoggedIn reports zero");
                    }

                    //blogIx = blogIx > 0 ? blogIx : BlogInfo(obj, i_isLike ? 2 : 8)?.DwBlogIx ?? 0;
                    return json;
                case 1:
                    sro_Logger.LogDebug("GetAPIPage::Starting::MODE=1");
                    //
                    //
                    // 
                    //  API Post Message with Data
                    // DateTime.ParseExact(Blog.DownloadFrom, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None)
                    dtSearchField = DateTime
                        .ParseExact(Blog.DownloadFrom, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None)
                        .ToString("yyyy-MM-ddTHH:mm:ss");
                    IEnumerable<Cookie> cookies = CookieService.GetAllCookies();
                    Cookie cookie = cookies.FirstOrDefault(c => c.Name == "Affinity");
                    string affinity = cookie?.Value ?? "";
                    cookie = cookies.FirstOrDefault(c => c.Name == "LoginToken");
                    string token = cookie?.Value ?? "";
                    cookie = cookies.FirstOrDefault(c => c.Name == "ActiveBlog");
                    long activeBlog = long.TryParse(cookie?.Value, out long result) ? result : 0;
                    string url = "https://api-ro.newtumbl.com/sp/NewTumbl/" + "search_Blog_Posts" +
                                 $"?affinity={affinity}";
                    Dictionary<string, string> d = new Dictionary<string, string>();

                    /*d.Add("json",
                        "{\"Params\":[\"[{IPADDRESS}]\",\"" + token + "\",null,\"0\",\"0\"," + activeBlog + ",null," +
                        (dtSearchField == null ? "null" : $"\"{dtSearchField}\"") + "," + i_pageNo +
                        ",50,0,null,0,\"\",0,0,0,0,0," + gIx.ToString()) + ",null]}");*/
                    return await PostDataAsync(url, Blog.Url, d, i_cookieHosts);
                default:
                    throw new NotImplementedException();
            }
        }


        public async Task IsBlogOnlineAsync22()
        {
            //TODO


            try
            {
                Root text = await DoApiGetRequest();
                Blog.Online = true;
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.RequestCanceled)
                {
                    return;
                }

                if (HandleUnauthorizedWebException(webException))
                {
                    Blog.Online = false;
                }
                else if (HandleLimitExceededWebException(webException))
                {
                    Blog.Online = false;
                }
                else if (HandleNotFoundWebException(webException))
                {
                    Blog.Online = false;
                }
                else
                {
                    sro_Logger.LogError($"NewTumblCrawler:IsBlogOnlineAsync: {Blog.Name}, {webException.Message}");
                    //Settings.ShowError(webException, "{0}, {1}", Blog.Name, webException.Message);
                    Blog.Online = false;
                }
            }
            catch (TimeoutException timeoutException)
            {
                HandleTimeoutException(timeoutException, Resources.OnlineChecking);
                Blog.Online = false;
            } //CATCH IS TOO SPECIFIC HERE NO FAILSAFE TO ENSURE PROPER RESULTS FROM METHOD TODO:REFACTOR
            catch (Exception ex) when (ex.Message == "Acceptance of privacy consent needed!")
            {
                Blog.Online = false;
            }
            finally
            {
                _ = Blog.Save();
            }
        }


        public async Task UpdateMetaInformationAsync(IProgress<string> progress)
        {
            if (await UpdateMetaInformationCoreAsync())
            {
                progress.Report("Success");
            }
            else
            {
                progress.Report("Failure");
            }
        }


        public static async Task IsBlogInline()
        {
            try
            {
                string ix = await GetBasicBlogInfoCoreAsync(Blog.Url);
                if (string.IsNullOrEmpty(ix))
                {
                    Blog.Online = false;
                    _ = Blog.Save();
                }


                // await UpdateMetaInformationCoreAsync();
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.RequestCanceled)
                {
                    return;
                }

                _ = HandleLimitExceededWebException(webException);
            }
        }


        public Task<List<ARow>> GetFollowedBlogsAsync()
        {
            throw new NotImplementedException();
        }


        public static async Task<bool> UpdateMetaInformationCoreAsync()
        {
            try
            {
                var json = await GetBasicBlogInfoCoreAsync(Blog.Url);
                var cook = ApiClient.GetCookieJar().GetAllCookies();
                //                string resp = await client.GetStringAsync(Blog.Url);
                //resp.EnsureSuccessStatusCode();

                var info = BlogInfo(json, 2);

                //Blog.DwBlogIx = obj.aResultSet[2].aRow[0].DwBlogIx;
                //Blog.DwUserIx = obj.aResultSet[2].aRow[0].dwUserIx;
                if (!String.IsNullOrEmpty(json))
                {
                    Blog.Progress = 0;
                    Blog.Online = true;
                    Blog.Save();
                }
            }
            catch (Exception)
            {
                sro_Logger.LogError($"An Error occured updating Meta info  {Blog.Name}");
            }

            return true;
        }


        private static async Task<bool> UpdateMetaInformationCoreAsync3()
        {
            //if (!Blog.Online) { return; }
            //string pagetext = await BrowserControl.LoadTargetPageGetContentAsync(Blog.Url);
            string page = String.Empty;
            try
            {
                page = await ApiHttpClient.GetStringAsync(Blog.Url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            try
            {
                string json = sro_ExtractJsonFromPage.Match(page).Groups[1].Value + "}";
                //  json = sro_ExtractJsonFromPage.Match(document).Groups[1].Value + "}";
                Root obj = JsonConvert.DeserializeObject<Root>(json);
                if (obj?.nResult == "0")
                {
                    ARow tgt = obj?.aResultSet?[2]?.aRow?.FirstOrDefault();
                    Blog.DwBlogIx = tgt?.dwBlogIx;

                    if (Blog.DwUserIx == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            /*  Blog.LatestPost = DateTime.MinValue;
              Blog.LastId = 0;

              Blog.LastCompleteCrawl = DateTime.MinValue;
              Blog.Online = true;
              Blog.Progress = 100;
            */
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        private async Task<bool> GetUrlsAsync()
        {
            semaphoreSlim = new SemaphoreSlim(App.Settings.ConcurrentScans);
            trackedTasks = new List<Task>();


            await semaphoreSlim.WaitAsync();
            trackedTasks.Add(CrawlPageAsync2());
            await Task.WhenAll(trackedTasks);

            // Is this working as intended?
            // Is more that one Task ever initiated?

            //BUG: Async method completeAdding is incorrectly labled
            await DownloadQue.CompleteAdding();
            //i_jsonQueue.CompleteAdding().Wait();

            UpdateBlogStats(GetLastPostId() != 0);

            // Check if variable is not assigned to
            return incompleteCrawl;
        }


        /// <summary>
        /// Sets up the parameters and data to post to the api
        /// </summary>
        /// <param name="ct"></param>
        /// <returns >string</returns>
        private async Task<string> MakeApiCallAsync(CancellationToken ct)
        {
            dtSearchField = "null";
            if (ct.IsCancellationRequested)
            {
                return string.Empty;
            }

            IEnumerable<Cookie> cookies = CookieService.GetAllCookies();
            ApiHttpClient.SetRequestCookies(CookieService.CookieJar.GetAllCookies());
            Cookie cookie = cookies.FirstOrDefault(c => c.Name == "Affinity");
            string affinity = cookie?.Value ?? "";
            cookie = cookies.FirstOrDefault(c => c.Name == "LoginToken");
            string token = cookie?.Value ?? "";
            string dtFrom = string.Empty;
            using ApiController api = new(token);


            if (!string.IsNullOrEmpty(Blog.DownloadFrom))
            {
                dtFrom = DateTime.ParseExact(Blog.DownloadFrom, "yyyyMMdd", null, DateTimeStyles.None)
                    .ToString("yyyy-MM-ddTHH:mm:ss");
            }

            ApiHttpRequest req = api.BuildHttpRequestMessage(EnumApiEndPoints.search_Blog_Posts,
                Blog.DwBlogIx.ToString(), Blog.Url);


            var par2 =
                $$"""json={"Params":["[{IPADDRESS}]","{{LoginToken}}",null,0,0,{{ActiveIndex}},null, null, {{pageNo}},50,0,null,0,"",0,0,0,0,0,{{Blog.DwBlogIx}},null]}""";


            var par = api.BuildParams(Blog.DwBlogIx, null, pageNo);
            req.SetContent(par2);


            HttpResponseMessage resp = await ApiHttpClient.SendAsync(req, ct);

            _ = resp.EnsureSuccessStatusCode();

            var cook = ApiHttpClient.GetAllCookies();

            string json = await resp.Content.ReadAsStringAsync(ct);

            if (CheckResultsForErrors(json))
            {
                //Error in results stop further attempts
                Blog.LastStatus = "ApiErr";
                Blog.Dirty = true;
                GrabComplete = true;
                return string.Empty;
            }

            return json;
        }


        private async Task CrawlPageAsync2()
        {
            try
            {
                while (true)
                {
                    if (GrabComplete) //Should break loop when we have grabbed all the new posts within timespan - If set
                    {
                        //TODO: not yet stable api param issue
                        break;
                    }

                    string results = string.Empty;
                    Root obj = null;
                    List<Post> posts = new();
                    for (int i = 0; i < 2; i++) //retry loop
                    {
                        try
                        {
                            //  string temp = await GetApiPageAsync(1);
                            results = await MakeApiCallAsync(Ct);
                            obj = ConvertJsonToClassNew<Root>(results);
                            posts = GetPosts(obj);
                            // break after successful grab-process-rinse & repeat
                            break;
                        }
                        catch (APIException)
                        {
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    pageNo++;
                    totalPosts += posts.Count;

                    dtSearchField ??= obj.aResultSet[7].aRow[0].dtSearch.Value.ToString("yyyy-MM-ddTHH:mm:ss");

                    if (highestId == 0)
                    {
                        highestId = (ulong)posts[0].QwPostIx.Value;
                    }

                    Blog.LatestPost = (DateTime)posts.Max(dt => dt.DtActive);

                    //If api returns correctly filtered results this should not be needed to
                    // stop paged results
                    GrabComplete = CheckPostAge(posts);

                    await AddUrlsToDownloadListAsync(posts);

                    //   Console.WriteLine($"number of pages crawled (x/50)= {numberOfPagesCrawled}");
                    Console.WriteLine($"total posts = {totalPosts}");
                    //     Dispatcher.UIThread.Post(() => UpdateProgressQueueInformation(posts.Count, numberOfPagesCrawled));
                }


                Blog.TotalCount = totalPosts;
            } //  Spread out exception handing for debugging only
            catch (TimeoutException ex)
            {
                incompleteCrawl = true;
                Console.WriteLine(ex.Message);
            }
            catch (ApplicationException ae)
            {
                Console.WriteLine(ae.Message);
                incompleteCrawl = true;
            }
            catch (APIException ae2)
            {
                Console.WriteLine(ae2.Message);
                incompleteCrawl = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                incompleteCrawl = true;
            }
            finally
            {
                _ = Blog.Save();
            }
        }


        /// <summary>
        ///     Checks the reponse string from API calls for an error condition
        ///     Returns true if errors are found.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>boolean</returns>
        private static bool CheckResultsForErrors(string response)
        {
            try
            {
                Root json = JsonConvert.DeserializeObject<Root>(response);
                if (json.nResult != "0")
                {
                    string msg = $"Api returned an error {json.nResult} reason: {json.sAPIErrorMessage}";
                    App.ShowError(msg);
                    sro_Logger.LogError(msg);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return true;
            }
        }


        private static bool CheckPostAge(List<Post> posts)
        {
            Post post = posts.FirstOrDefault();

            if (post == null)
            {
                return false;
            }

            var max = posts.Max(ix => ix.QwPostIx.Value);

            ulong highestPostId = (ulong)post.QwPostIx.Value;

            return highestPostId >= GetLastPostId();
        }


        private static bool PostWithinTimeSpan(Post post)
        {
            if (string.IsNullOrEmpty(Blog.DownloadFrom) && string.IsNullOrEmpty(Blog.DownloadTo))
            {
                return true;
            }

            DateTime downloadFrom = DateTime.MinValue;
            DateTime downloadTo = DateTime.Now.AddDays(1);

            if (!string.IsNullOrEmpty(Blog.DownloadFrom))
            {
                downloadFrom = DateTime.ParseExact(Blog.DownloadFrom, "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None);
            }

            if (!string.IsNullOrEmpty(Blog.DownloadTo))
            {
                downloadTo = DateTime
                    .ParseExact(Blog.DownloadTo, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None)
                    .AddDays(1);
            }

            int span = DateTime.Compare(downloadFrom, post.DtActive.Value);
            int span2 = DateTime.Compare(post.DtActive.Value, downloadTo);

            //  var iswithinspan= downloadFrom <= post.DtActive.Value && post.DtActive.Value < downloadTo;
            return Equals(span, span2);
            //return iswithinspan;
        }


        private static bool CheckIfSkipGif(string imageUrl)
        {
            return Blog.SkipGif && (imageUrl.EndsWith(".gif") || imageUrl.EndsWith(".gifv"));
        }


        private static bool CheckIfDownloadRebloggedPosts(Post post)
        {
            return Blog.DownloadRebloggedPosts || post.QwPostIx.Value == post.QwPostIxFrom.Value;
        }


        private bool CheckIfContainsTaggedPost(Post post)
        {
            return !Tags.Any() || post.Tags.Any(x => Tags.Contains(x.SzTagId, StringComparer.OrdinalIgnoreCase));
        }


        private async Task GetAlreadyExistingCrawlerDataFilesAsync()
        {
            foreach (string filepath in Directory.GetFiles(Blog.DownloadLocation, "*.json"))
            {
                i_existingCrawlerData.Add(Path.GetFileName(filepath));
            }

            await Task.CompletedTask;
        }


        private static List<string> GetTags(Post post)
        {
            return post.Tags == null ? new List<string>() : post.Tags.Select(t => t.SzTagId).ToList();
        }


        private static string BuildFileName(string url, Post post, string type, int index)
        {
            bool reblogged = !post.DwBlogIx.Equals(post.DwBlogIxFrom);
            string userId = post.DwBlogIx.ToString();
            string reblogName = "";
            string reblogId = "";
            if (reblogged)
            {
                reblogName = post.DwBlogIxFrom.ToString();
                reblogId = post.DwBlogIxFrom.ToString();
            }

            List<string> tags = GetTags(post);

            return BuildFileNameCore(url, Blog.Name, post.DtActive.Value, UnixTimestamp(post), index, type,
                GetPostId(post), tags, "", GetTitle(post), reblogName, reblogId);
        }


        private static string GetTitle(Post post)
        {
            string title = "";
            if (post.BPostTypeIx.Equals(PostType.Photo) || post.BPostTypeIx.Equals(PostType.Video) ||
                post.BPostTypeIx.Equals(PostType.Audio))
            {
                title = post.Parts.FirstOrDefault(p => p.BPartTypeIx == PostType.Comment)?.Medias?[0]?.SzBody ?? "";
                //         title = RemoveHtmlFromString(title);
            }

            return title;
        }


        private static string Sanitize(string filename)
        {
            char[] invalids = Path.GetInvalidFileNameChars();

            return string.Join("-", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }


        private static string ReplaceCi(string input, string search, string replacement)
        {
            string result = Regex.Replace(input, Regex.Escape(search), replacement.Replace("$", "$$"),
                RegexOptions.IgnoreCase);

            return result;
        }


        private static bool ContainsCi(string input, string search)
        {
            return input.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }


        [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
        private static string BuildFileNameCore(
            string url,
            string blogName,
            DateTime date,
            int timestamp,
            int index,
            string type,
            string id,
            List<string> tags,
            string slug,
            string title,
            string rebloggedFromName,
            string reblogKey)
        {
            /*
             * Replaced are:
             *  %f  original filename (default)
                %b  blog name
                %d  post date (yyyyMMdd)
                %e  post date and time (yyyyMMddHHmmss)
                %g  post date in GMT (yyyyMMdd)
                %h  post date and time in GMT (yyyyMMddHHmmss)
                %u  post timestamp (number)
                %o  blog name of reblog origin
                %p  post title (shorted if needed…)
                %i  post id
                %n  image index (of photo sets)
                %t  for all tags (cute+cats,big+dogs)
                %r  for reblog ("" / "reblog")
                %s  slug (last part of a post's url)
                %k  reblog-key
               Tokens to make filenames unique:
                %x  "_{number}" ({number}: 2..n)
                %y  " ({number})" ({number}: 2..n)
             */
            string filename = Blog.FilenameTemplate;
            filename += Path.GetExtension(FileName(url));
            if (ContainsCi(filename, "%f"))
            {
                filename = ReplaceCi(filename, "%f", Path.GetFileNameWithoutExtension(FileName(url)));
            }

            if (ContainsCi(filename, "%d"))
            {
                filename = ReplaceCi(filename, "%d", date.ToString("yyyyMMdd"));
            }

            if (ContainsCi(filename, "%e"))
            {
                filename = ReplaceCi(filename, "%e", date.ToString("yyyyMMddHHmmss"));
            }

            if (ContainsCi(filename, "%g"))
            {
                filename = ReplaceCi(filename, "%g", date.ToUniversalTime().ToString("yyyyMMdd"));
            }

            if (ContainsCi(filename, "%h"))
            {
                filename = ReplaceCi(filename, "%h", date.ToUniversalTime().ToString("yyyyMMddHHmmss"));
            }

            if (ContainsCi(filename, "%u"))
            {
                filename = ReplaceCi(filename, "%u", timestamp.ToString());
            }

            if (ContainsCi(filename, "%b"))
            {
                filename = ReplaceCi(filename, "%b", blogName);
            }

            if (ContainsCi(filename, "%i"))
            {
                if (type == "photo" && Blog.GroupPhotoSets && index != -1)
                {
                    id = $"{id}_{index}";
                }

                filename = ReplaceCi(filename, "%i", id);
            }
            else if (type == "photo" && Blog.GroupPhotoSets && index != -1)
            {
                filename = $"{id}_{index}_{filename}";
            }

            if (ContainsCi(filename, "%n"))
            {
                if (type != "photo" || index == -1)
                {
                    string charBefore = "";
                    string charAfter = "";
                    if (filename.IndexOf("%n", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        charBefore = filename.Substring(filename.IndexOf("%n", StringComparison.OrdinalIgnoreCase) - 1,
                            1);
                    }

                    if (filename.IndexOf("%n", StringComparison.OrdinalIgnoreCase) < filename.Length - 2)
                    {
                        charAfter = filename.Substring(filename.IndexOf("%n", StringComparison.OrdinalIgnoreCase) + 2,
                            1);
                    }

                    if (charBefore == charAfter)
                    {
                        filename = filename.Remove(filename.IndexOf("%n", StringComparison.OrdinalIgnoreCase) - 1, 1);
                    }

                    filename = ReplaceCi(filename, "%n", "");
                }
                else
                {
                    filename = ReplaceCi(filename, "%n", index.ToString());
                }
            }

            if (ContainsCi(filename, "%t"))
            {
                filename = ReplaceCi(filename, "%t", string.Join(",", tags).Replace(" ", "+"));
            }

            if (ContainsCi(filename, "%r"))
            {
                if (rebloggedFromName.Length == 0 && filename.IndexOf("%r", StringComparison.OrdinalIgnoreCase) > 0 &&
                    filename.IndexOf("%r", StringComparison.OrdinalIgnoreCase) < filename.Length - 2 &&
                    filename.Substring(filename.IndexOf("%r", StringComparison.OrdinalIgnoreCase) - 1, 1) ==
                    filename.Substring(filename.IndexOf("%r", StringComparison.OrdinalIgnoreCase) + 2, 1))
                {
                    filename = filename.Remove(filename.IndexOf("%r", StringComparison.OrdinalIgnoreCase), 3);
                }

                filename = ReplaceCi(filename, "%r", rebloggedFromName.Length == 0 ? "" : "reblog");
            }

            if (ContainsCi(filename, "%o"))
            {
                filename = ReplaceCi(filename, "%o", rebloggedFromName);
            }

            if (ContainsCi(filename, "%s"))
            {
                filename = ReplaceCi(filename, "%s", slug);
            }

            if (ContainsCi(filename, "%k"))
            {
                filename = ReplaceCi(filename, "%k", reblogKey);
            }

            int neededChars = 0;
            if (ContainsCi(filename, "%x"))
            {
                neededChars = 6;
                //Downloader.AppendTemplate = "_<0>";
                filename = ReplaceCi(filename, "%x", "");
            }

            if (ContainsCi(filename, "%y"))
            {
                neededChars = 8;
                //Downloader.AppendTemplate = " (<0>)";
                filename = ReplaceCi(filename, "%y", "");
            }

            if (ContainsCi(filename, "%p"))
            {
                string atitle = title;
                if (!AppSettings.IsLongPathSupported)
                {
                    string filepath = Path.Combine(Blog.DownloadLocation, filename);
                    // 260 (max path minus NULL) - current filename length + 2 chars (%p) - chars for numbering
                    int charactersLeft = 259 - filepath.Length + 2 - neededChars;

                    if (charactersLeft < 0)
                    {
                        throw new PathTooLongException($"{Blog.Name}: filename for post id {id} is too long");
                    }

                    if (charactersLeft < atitle.Length)
                    {
                        atitle = atitle[..(charactersLeft - 1)] + "…";
                    }
                }

                filename = ReplaceCi(filename, "%p", atitle);
            }
            else if (!AppSettings.IsLongPathSupported)
            {
                string filepath = Path.Combine(Blog.DownloadLocation, filename);
                // 260 (max path minus NULL) - current filename length - chars for numbering
                int charactersLeft = 259 - filepath.Length - neededChars;

                if (charactersLeft < 0)
                {
                    throw new PathTooLongException($"{Blog.Name}: filename for post id {id} is too long");
                }
            }

            return Sanitize(filename);
        }


        private void AddToJsonQueue(CrawlerData<Post> addToList)
        {
            if (!Blog.DumpCrawlerData)
            {
                return;
            }

            lock (i_existingCrawlerDataLock)
            {
                if (Blog.ForceRescan || !i_existingCrawlerData.Contains(addToList.Filename))
                {
                    //i_jsonQueue.Add(addToList);
                    i_existingCrawlerData.Add(addToList.Filename);
                }
            }
        }


        /// <summary>
        /// Method filters posts based on overall app settings or individual blog settings
        /// </summary>
        /// <param name="posts"></param>
        /// <returns>Task</returns>
        internal Task AddUrlsToDownloadListAsync(List<Post> posts)
        {
            ulong lastPostId = GetLastPostId();
            foreach (Post post in posts)
            {
                try
                {
                    if (post.Parts[0].Medias[0].QwMediaIx > 0)
                    {
                        // ??
                    }

                    if (lastPostId > 0 && (ulong)post.QwPostIx.Value < lastPostId)
                    {
                        continue;
                    }

                    if (!PostWithinTimeSpan(post))
                    {
                        continue;
                    }

                    if (!CheckIfContainsTaggedPost(post))
                    {
                        continue;
                    }

                    if (!CheckIfDownloadRebloggedPosts(post))
                    {
                        continue;
                    }


                    if (Blog.DownloadPhoto && post.BPostTypeIx.Equals(PostType.Photo))
                    {
                        Console.WriteLine($"Adding photo url {post.SzUrl}");
                        AddPhotoUrl(post);
                    }

                    if (Blog.DownloadVideo && post.BPostTypeIx.Equals(PostType.Video))
                    {
                        Console.WriteLine($"Adding video Url {post.SzUrl}");
                        AddVideoUrl(post);
                    }
                }
                catch (Exception)
                {
                    // Catch bad apple, log and continue
                    App.ShowError("Error adding url to list");
                    sro_Logger.LogDebug("NewTumblCrawler.AddUrlsToDownloadListAsync: ");
                    sro_Logger.LogInformation($"Failed to process a post in blog {Blog.Name} Continuing on...");

                    continue;
                }
            }


            return Task.CompletedTask;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="post"></param>
        private void AddPhotoUrl(Post post)
        {
            sro_Logger.LogDebug("starting AddPhotoUrl");

            int photoCount = post.Parts.Count(c => c.BPartTypeIx == PostType.Photo);
            int counter = 1;
            foreach (Part part in post.Parts)
            {
                //Double check post type
                if (!part.BPartTypeIx.Equals(PostType.Photo))
                {
                    continue;
                }

                Media media = part.Medias[0];
                if (BlogService.IsMediaIndexOnFile(media.QwMediaIx.ToString()))
                {
                    continue;
                }

                if (i_downloader.CheckIfMediaExists(media.QwMediaIx.ToString()))
                {
                    continue;
                }

                string imageUrl = GetMediaUrl(blogIx, post.QwPostIx, part.NPartIz, part.QwPartIx, media.BMediaTypeIx,
                    media.NWidth, media.NHeight, 0);

                // skipping photos so just add to the stats collection
                if (!Blog.DownloadPhoto && post.BPostTypeIx.Equals(PostType.Photo))
                {
                    StatisticsBag.Add(new PhotoPost(imageUrl, null, null, null));

                    continue;
                }

                int index = photoCount > 1 ? counter++ : -1;

                // Build Filename and Url from port parts
                string filename = BuildFileName(imageUrl, post, "photo", index);
                AddToDownloadList(new PhotoPost(imageUrl, null, GetPostId(part), UnixTimestamp(post).ToString(),
                    filename));
                i_downloader.AddMediaToDb(media.QwMediaIx.ToString());
            }
        }


        private static string InlineSearch(Post post)
        {
            string s = "";
            foreach (Part part in post.Parts)
            {
                foreach (Media media in part.Medias)
                {
                    s += $"{media.SzSub} {media.SzBody} ";
                }
            }

            return s;
        }


        private static int UnixTimestamp(Post post)
        {
            long postTime = ((DateTimeOffset)post.DtActive).ToUnixTimeSeconds();

            return (int)postTime;
        }


        /// <summary>
        ///     Extracts the media URL from the post parts
        ///     creates a new videopost object and adds to download list.
        /// </summary>
        /// <param name="post"></param>
        private void AddVideoUrl(Post post)
        {
            foreach (Part part in post.Parts)
            {
                if (!post.BPostTypeIx.Equals(PostType.Video) || !part.BPartTypeIx.Equals(PostType.Video))
                {
                    continue;
                }

                Media media = part.Medias[0];

                if (i_downloader.CheckIfMediaExists(media.QwMediaIx.ToString()))
                {
                    continue;
                }

                string videoUrl = GetMediaUrl((long)Blog.DwBlogIx, post.QwPostIx, part.NPartIz, part.QwPartIx,
                    media.BMediaTypeIx, media.NWidth, media.NHeight, 0);

                VideoPost v = new VideoPost(videoUrl, GetPostId(part), UnixTimestamp(post).ToString(),
                    Path.GetFileName(videoUrl));
                AddToDownloadList(v);


                /*
                            await ApiDownloader.DownloadRemoteFileAsync(videoUrl,
                                Path.Combine(Blog.DownloadLocation, Path.GetFileName(videoUrl)));
                                */
            }
        }
        /*

                    if (Blog.DownloadVideo)
                    {
                        if (firstVideoUrl == null) firstVideoUrl = videoUrl;
                        AddToDownloadList(new VideoPost(videoUrl, GetPostId(part), UnixTimestamp(post).ToString(),
                            BuildFileName(videoUrl, post, "video", -1)));
                    }
                    else
                    {
                        StatisticsBag.Add(new VideoPost(videoUrl, null, null));
                    }

                    var imageUrl = GetMediaUrl(blogIx, post.QwPostIx, part.NPartIz, part.QwPartIx, media.BMediaTypeIx,
                        media.NWidth, media.NHeight, 300);
                    ApiDownloader.DownloadRemoteFileAsync(imageUrl,
                        Path.Combine(Blog.DownloadLocation, Path.GetFileName(imageUrl))).Wait();
                    if (Blog.DownloadVideoThumbnail)
                    {
                        var filename = FileName(imageUrl);
                        filename = BuildFileName(filename, post, "photo", -1);
                        AddToDownloadList(new PhotoPost(imageUrl, null, GetPostId(part, true), UnixTimestamp(post).ToString(),
                            filename));
                        if (!Blog.DownloadVideo)
                            AddToJsonQueue(new CrawlerData<Post>(Path.ChangeExtension(imageUrl.Split('/').Last(), ".json"),
                                post));
                    }
                    else
                    {
                        StatisticsBag.Add(new PhotoPost(imageUrl, null, null, null));
                    }
                }

                if (firstVideoUrl != null)
                    AddToJsonQueue(new CrawlerData<Post>(Path.ChangeExtension(firstVideoUrl.Split('/').Last(), ".json"), post));
            }
        */


        private static void HandleAuthenticationError(IBlog blog)
        {
            sro_Logger.LogInformation("Attempting to Set authorization cookies....");
            WaitHandle handle = new AutoResetEvent(false);

            QueueListItem item = new(Blog);
            ServiceBase.QueueManager.AddItem(item);

            _ = ThreadPool.QueueUserWorkItem(CookieService.SetAuthCookie, handle);
            _ = handle.WaitOne();

            sro_Logger.LogInformation("Returning current blog to process again.");
        }


        private static void CheckError(Root obj)
        {
            if (obj.nResult == "-1")
            {
                //  App.ShowError(                $"Newtumbl API has returned an error code of {obj.nResult}. Further processing of results will not be possible");
                sro_Logger.LogError(
                    $"server returned: {obj.aResultSet[0].aRow[0].szError} ({obj.aResultSet[0].aRow[0].dwError})");

                throw new APIException(obj.aResultSet[0].aRow[0].szError);
            }

            if (obj.nResult == "-9999")
            {
                //  App.ShowError(        $"Newtumbl API has returned an error code of {obj.nResult}. Further processing of results will not be possible");
                sro_Logger.LogError(string.Format(Resources.ErrorDownloadingBlog, Blog.Name,
                    $"{obj.sError}({obj.sAPIErrorCode}): {obj.sAPIErrorMessage}", "My Feed"));

                throw new APIException(obj.aResultSet[0].aRow[0].szError);
            }
            /*
            else if (obj.nResult != "0")
            {
                //App.ShowError(                $"Newtumbl API has returned an unknown error code of {obj.nResult}. Further processing of results will not be possible");
                sro_Logger.LogCritical($"Newtumbl API has returned an unexpected error code of {obj.nResult}");
                throw new ApplicationException("Results from api results check has failed server returned an error");
            }
    */
        }


        private static List<Post> GetPosts(Root obj)
        {
            List<ARow> posts = PostInfo(obj, 3);
            //  var tags = PostInfo(obj, 5);
            List<ARow> parts = PostInfo(obj, 4);
            List<ARow> medias = PostInfo(obj, 1);
            List<Post> list = new List<Post>();
            foreach (ARow post in posts.OrderByDescending(o => o.qwPostIx))
            {
                Post item = Post.Create(post);
                //      item.Tags = tags.Where(w => w.QwPostIx == item.QwPostIx).OrderBy(o => o.BOrder).Select(s => Tag.Create(s)).ToList();
                item.Parts = parts.Where(w => w.qwPostIx == item.QwPostIx).OrderBy(o => o.bOrder)
                    .Select(Part.Create).ToList();
                foreach (Part part in item.Parts)
                {
                    part.Medias = medias.Where(w => w.qwPartIx == part.QwPartIx).OrderBy(o => o.bOrder)
                        .Select(Media.Create).ToList();
                }

                list.Add(item);
            }

            return list;
        }


        private static string GetPostId(Post post)
        {
            return post.QwPostIx.ToString();
        }


        private static string GetPostId(Part part, bool isThumb = false)
        {
            // url filenames are unique and can't identify duplicates, so use mediaIx for now
            return part.Medias[0].QwMediaIx + (isThumb ? "T" : "");
        }


        private static ARow BlogInfo(string json, int type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            Root obj = ConvertJsonToClassNew<Root>(json);
            CheckError(obj);

            return BlogInfo(obj, type);
        }


        private static List<ARow> PostInfoold(string json, int type)
        {
            Root obj = ConvertJsonToClassNew<Root>(json);
            CheckError(obj);

            return PostInfo(obj, type);
        }


        private static List<ARow> PostInfo(Root obj, int type)
        {
            /*
             * 1.x - post part medias
             * 2.x - blog info
             * 3.x - posts
             * 4.x - post parts
             * 5.x - post tags
             * 7.0 - search time
             */
            switch (type)
            {
                case 1:
                    return obj.aResultSet[type].aRow;
                case 2:
                    return obj.aResultSet[type].aRow;
                case 3:
                    return obj.aResultSet[type].aRow;
                case 4:
                    return obj.aResultSet[type].aRow;
                case 5:
                    return obj.aResultSet[type].aRow;
                case 7:
                    return obj.aResultSet[type].aRow;
                default:
                    break;
            }

            return null;
        }


        private static byte[] GetSha256(string msg)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(msg);
            byte[] bytes = SHA256.HashData(buffer);

            return bytes;
        }


        private static string GetBase32(byte[] input)
        {
            const string map = "abcdefghijknpqrstuvxyz0123456789";
            string output = "";
            int i = -1;
            int b = 0;
            int c = 0;
            int d;
            while (i < input.Length || b > 0)
            {
                if (b < 5)
                {
                    if (++i < input.Length)
                    {
                        c = (c << 8) + input[i];
                        b += 8;
                    }
                }

                d = c % 32;
                c >>= 5;
                b -= 5;
                output += map[d];
            }

            return output;
        }


        private string GetMediaUrl(
            long DwBlogIx,
            long? qwPostIx,
            int? nPartIz,
            long? qwPartIx,
            int? bMediaTypeIx,
            int? nWidth,
            int? nHeigh,
            int nThumb)
        {
            string sUrl = null;
            bool bThumb = false;
            bool bJpg = false;
            if (new[] { 4, 6, 7, 2, 3 }.Contains(bMediaTypeIx.Value))
            {
                bJpg = (bMediaTypeIx == 4 || bMediaTypeIx == 6 || bMediaTypeIx == 7) && nThumb > 0;
                bThumb = nThumb == 600 && nHeigh > 800 || nThumb == 1200 && nHeigh > 1200
                                                       || nThumb > 0 && nWidth > nThumb;
            }
            else if (bMediaTypeIx != 5)
            {
                sUrl = "";
            }

            if (sUrl == null)
            {
                string sPath = $"/{DwBlogIx}/{qwPostIx}/{nPartIz}/{qwPartIx}/nT_";
                string sThumb = bThumb ? "_" + nThumb : "";
                string sExt = bThumb || bJpg ? "jpg" : i_aExt[bMediaTypeIx.Value];
                string sHost = "dn" + qwPostIx % 4;
                sUrl = "https://" + sHost + ".newtumbl.com/img" + sPath + GetBase32(GetSha256(sPath))[..24] +
                       sThumb + "." + sExt;
            }

            return sUrl;
        }


        private static WebException CreateWebException(HttpStatusCode httpStatus)
        {
            return new WebException();
            /*
            using (var listener = new HttpListener())
            {
                var prefix = "";
                var port = 56789;
                var count = 0;
                do
                {
                    count++;
                    try
                    {
                        prefix = $"http://localhost:{port}/mocking/";
                        listener.Prefixes.Clear();
                        listener.Prefixes.Add(prefix);
                        listener.Start();
                        port = 0;
                    }
                    catch (NotSupportedException)
                    {
                        port = 0;
                    }
                    catch (HttpListenerException)
                    {
                        port = new Random().Next(50000, 65000);
                    }
                } while (port != 0 && count < 3);

                try
                {
                    listener.BeginGetContext(ar =>
                    {
                        var context = listener.EndGetContext(ar);
                        var request = context.Request;
                        var response = context.Response;
                        response.StatusCode = (int)httpStatus;
                        response.Close();
                    }, null);
                    using (WebClient client = new())
                    {
                        try
                        {
                            client.OpenRead(prefix + "error.aspx");
                        }
                        catch (WebException e)
                        {
                            var httpWebResponse = e.Response as HttpWebResponse;
                            return new WebException("Error", null, WebExceptionStatus.ProtocolError, e.Response);
                        }
                    }
                }
                finally
                {
                    listener.Stop();
                }
            }

            return null;
            */
        }


        public static void GetDashboardPosts(string referer, string blogIx)
        {
            ApiController apiController = new ApiController("");
            ApiHttpRequest req = apiController.BuildHttpRequestMessage(EnumApiEndPoints.search_Blog_Posts, blogIx,
                referer);
            _ = apiController.BuildParams(0);
            //var jsonobj = JsonConvert.DeserializeObject<DashPosts.RootObject>(rawpost);
            //var posts = GetPosts(rawpost);
        }


        /// <summary>
        ///     Method adds newly created post to the actual post queue
        ///     for processing
        /// </summary>
        /// <param name="addToList"></param>
        protected void AddToDownloadList(TumblrPost addToList)
        {
            //Add to download que
            DownloadQue.Add(addToList);
            Console.WriteLine($"Download Added to Que Count= {DownloadQue.Count}");
            TumblrPost tmp = addToList.CloneWithAdjustedUrl(addToList.Id);
            //Add the Url to the statistics container
            StatisticsBag.Add(tmp);
            Blog.AddFileToDb(Path.GetFileName(tmp.Url));
        }


        protected static string FileName(string url)
        {
            return url.Split('/').Last();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                semaphoreSlim?.Dispose();
                i_downloader.Dispose();
            }
        }


        public static void GetDashboardPosts2(string referer, string blogIx)
        {
            ApiController apiController = new ApiController("");
            ApiHttpRequest req = apiController.BuildHttpRequestMessage(EnumApiEndPoints.search_Blog_Posts, blogIx,
                referer);
            _ = apiController.BuildParams(0);
            //var jsonobj = JsonConvert.DeserializeObject<DashPosts.RootObject>(rawpost);
            //var posts = GetPosts(rawpost);
        }


        /// <summary>
        ///     Method adds newly created post to the actual post queue
        ///     for processing
        /// </summary>
        /// <param name="addToList"></param>
        protected void AddToDownloadList2(TumblrPost addToList)
        {
            //Add to download que
            DownloadQue.Add(addToList);

            TumblrPost tmp = addToList.CloneWithAdjustedUrl(addToList.Id);
            //Add the Url to the statistics container
            StatisticsBag.Add(tmp);
            Blog.AddFileToDb(Path.GetFileName(tmp.Url));
        }
    }
}