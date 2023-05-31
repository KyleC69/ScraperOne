using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;
using KC;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ScraperOne.DataModels;
using ScraperOne.DataModels.NewTumbl;
using ScraperOne.Modules.Downloaders;
using ScraperOne.Properties;
using ScraperOne.Services;

namespace ScraperOne.Modules.Crawlers
{
    public interface IAbstractCrawler
    {
    }

    public abstract class AbstractCrawler : IAbstractCrawler, IDisposable
    {
        protected static readonly Regex sro_ExtractJsonFromPage =
            new Regex("var Data_Session[ ]+?=[ ]??(.*?)};", RegexOptions.Singleline);

        public static ApiClient ApiHttpClient = new();
        private static ILogger s_logger;
        protected readonly CancellationTokenSource InterruptionTokenSource;
        private CancellationTokenSource LinkedTokenSource;


        protected AbstractCrawler(
            ICrawlerService crawlerService,
            IWebRequestFactory webRequestFactory,
            CookieService cookieService,
            IPostQueue<AbstractPost> downloadQue,
            IBlog blog,
            NewTumblDownloader downloader,
            ILogger logger,
            IProgress<DownloadProgress> progress,
            CancellationToken ct)
        {
            CrawlerService = crawlerService;
            WebRequestFactory = webRequestFactory;
            CookieService = cookieService;
            DownloadQue = downloadQue;
            Blog = blog;
            Progress = progress;
            Downloader = downloader;
            s_logger = logger;
            // TODO: Find a better way for this construct
            InterruptionTokenSource = new CancellationTokenSource();
            LinkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct, InterruptionTokenSource.Token);
            Ct = LinkedTokenSource.Token;
        }

        protected NewTumblDownloader Downloader { get; set; }
        protected static IBlog Blog { get; set; }
        protected static CookieService CookieService { get; set; }
        protected static ICrawlerService CrawlerService { get; set; }
        protected CancellationToken Ct { get; set; }
        protected object LockObjectDb { get; } = new();
        protected object LockObjectDirectory { get; } = new();
        protected object LockObjectDownload { get; } = new();
        protected object LockObjectProgress { get; } = new();
        protected IPostQueue<AbstractPost> DownloadQue { get; }
        protected IProgress<DownloadProgress> Progress { get; set; }
        protected ConcurrentBag<TumblrPost> StatisticsBag { get; set; } = new();
        protected List<string> Tags { get; set; } = new();
        protected IWebRequestFactory WebRequestFactory { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public void InterruptionRequestedEventHandler(object sender, EventArgs e)
        {
            InterruptionTokenSource.Cancel();
        }


        /// <summary>
        ///     HTTP GET of target blog to retrieve index info
        /// </summary>
        /// <param name="url">Url of the blog to retrieve info for</param>
        /// <param name="mode">Select mode of operation</param>
        /// <remarks>PORT</remarks>
        /// <returns>Task-string</returns>
        public static async Task<string> GetBasicBlogInfoCoreAsync(string url, int mode = 0)
        {
            ApiHttpClient.SetRequestCookies(CookieService.CookieJar.GetAllCookies());
            string json;

            switch (mode)
            {
                case 0:
                {
                    ApiController api = new();
                    ApiHttpRequest req = new();

                    try
                    {
                        req.RequestUri = new Uri(url);
                        req.Method = HttpMethod.Get;
                        var resp = await ApiHttpClient.SendAsync(req);
                        var data = await resp.Content.ReadAsStringAsync();
                        json = sro_ExtractJsonFromPage.Match(data).Groups[1].Value + "}";
                        var obj = ConvertJsonToClassNew<Root>(json);
                        var cook = ApiClient.GetCookieJar().GetAllCookies();

                        var info2 = BlogInfo(obj, 2);
                        var info3 = BlogInfo(obj, 3);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return string.Empty;
                    }
                    finally
                    {
                        api.Dispose();
                    }

                    //ApiHttpClient.SaveCookies();
                    return json;
                }
                case 1:
                {
                    string document = await ApiHttpClient.GetStringAsync(url);
                    json = sro_ExtractJsonFromPage.Match(document).Groups[1].Value + "}";
                    //var obj = ConvertDocumentToStructuredJson<Root>(json);
                    Root obj = ConvertJsonToClassNew<Root>(json);

                    ARow blog = BlogInfo(obj, 2);
                    ARow user = BlogInfo(obj, 3);

                    if (user.bLoggedIn == 0)
                    {
                        s_logger.LogError("AbstractCrawler::GetBasicBlogInfoCore::GetApi - User NotLogged in");
                        return null;
                    }

                    var dwBlogIx = blog.dwBlogIx;

                    return dwBlogIx.ToString();
                }

                default:
                    break;
            }

            return null;
        }


        public static T ConvertJsonToClassNew<T>(string json) where T : new()
        {
            try
            {
                Guard.IsNotNullOrEmpty(json, json);

                json = json.Replace(":undefined", ":null");
                using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                if (json == "}")
                {
                    return new T();
                }

                JsonSerializer deserializer = new JsonSerializer();
                deserializer.Converters.Add(new SingleOrArrayConverter<T>());
                using StreamReader sr = new StreamReader(ms);
                using JsonTextReader jsonTextReader = new JsonTextReader(sr);
                return deserializer.Deserialize<T>(jsonTextReader);
            }
            catch (JsonException)
            {
                s_logger.LogError("AbstractCrawler:ConvertJsonToClassNew<T>: {0}", "Could not parse data");
                Console.WriteLine(Resources.PostNotParsable, Blog.Name);
                return new T();
            }
        }


        public virtual async Task IsBlogOnlineAsync()
        {
            try
            {
                ApiClient client = new();
                HttpResponseMessage resp = await client.GetAsync(new Uri(Blog.Url));
                _ = resp.EnsureSuccessStatusCode();
                string str = await resp.Content.ReadAsStringAsync();
                string json = sro_ExtractJsonFromPage.Match(str).Groups[1].Value + "}";
                Root obj = JsonConvert.DeserializeObject<Root>(json);
                //  document.LoadHtml(str);
                // taking some things for granted here. 
                // Subset 2 is used for blog info. if the blog is not active 
                // The api should not return this row. if it's a temporary errr we may recv dadta here
                Blog.Online = obj.aResultSet[2].nTotalRows > 0;


                Blog.DwBlogIx = obj.aResultSet[2].aRow[0].dwBlogIx;
                Blog.DwUserIx = obj.aResultSet[2].aRow[0].dwUserIx;
                //                    Blog.LastStatus = obj.aResultSet[2].aRow[0].dwBlogIx.ToString();
                //                  Blog.LastStatus = $"Offline: {DateTime.Now:d}";
                //Newtumbl in an effort to prevent dead hosts in Search engines when they are disabled
                // for administrative reasons the hosts will still respond to requests with a page
                // that only contains a  Marquee. We must analyze results to make sure page is empty.
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.RequestCanceled)
                {
                    return;
                }

                s_logger.LogError("AbstractCrawler:IsBlogOnlineAsync:WebException");
                // Settings.ShowError(webException, Resources.BlogIsOffline, Blog.Name);
                Blog.Online = false;
            }
            catch (TimeoutException timeoutException)
            {
                HandleTimeoutException(timeoutException, Resources.OnlineChecking);
                Blog.Online = false;
            }
            catch (Exception ex) when (ex.Message == "Acceptance of privacy consent needed!")
            {
                Blog.Online = false;
            }
            finally
            {
                _ = Blog.Save();
            }
        }

        /*
                public virtual async Task UpdateMetaInformationAsync()
                {
                    _ = await Task.FromResult<object>(null);
                }*/


        public void UpdateProgressQueueInformation(int nposts, int pagecnt)
        {
            DownloadProgress newProgress = new DownloadProgress
            {
                //SProgress = string.Format(CultureInfo.CurrentCulture, format, args)
            };
            //HACK: Refacter and sanitize
            Blog.TotalCount += pagecnt;

            Blog.Posts += nposts;
            Progress.Report(newProgress);
        }


        [CanBeNull]
        protected static ARow BlogInfo([NotNull] Root obj, int type)
        {
            /*
             * 0.0 - user account details
             * 1.1 - blog image
             * 1.2 - blog banner
             * 2.0 - blog info
             * 3.0 - user settings
             * 4.0 - user's active blog
             * 7.0 - search time
             * 8.0 - blog stati
             * 12 - genres
             * */
            try
            {
                switch (type)
                {
                    case 2:
                        if ((obj.aResultSet[type]?.aRow).Count > 0)
                        {
                            return obj.aResultSet[type]?.aRow[0];
                        }

                        //    return obj.aResultSet[type]?.aRow.Where(w => w.dwBlogIx == blogIx).First();
                        break;
                    case 3:
                        return obj.aResultSet[type].aRow[0];
                    case 8:
                        if (obj.aResultSet.Count < 9)
                        {
                            return null;
                        }

                        return obj.aResultSet[type].aRow[0];
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }


        protected static List<ARow> GetBlogs(Root obj, int type)
        {
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
                case 8:
                    return obj.aResultSet[type].aRow;
                default:
                    break;
            }

            return null;
        }


        protected static async Task<T> ThrottleConnectionAsync<T>(string url, Func<string, Task<T>> method)
        {
            if (App.Settings.LimitConnectionsApi)
            {
                //  CrawlerService.TimeconstraintApi.Acquire();
            }

            return await method(url);
        }


        /// <summary>
        ///     HttpClient version to Get Basic info.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookieHosts"></param>
        /// <returns>string</returns>
        protected async Task<string> BasicInfoGetRequest(
            string url,
            Dictionary<string, string> headers = null,
            IEnumerable<string> cookieHosts = null)
        {
            int redirectCount = 0;
            HttpRequestMessage req;
            HttpResponseMessage resp = new();

            req = new HttpRequestMessage(HttpMethod.Get, new Uri(url))
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            try
            {
                Uri uri = new Uri(url);
                do
                {
                    resp = await ApiHttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, Ct);
                    _ = resp.EnsureSuccessStatusCode();

                    Debugger.Log(65, "BasicInfoGetRequest", $"StatusCode::{resp.StatusCode}");
                    if (!resp.IsSuccessStatusCode && resp.StatusCode == HttpStatusCode.Moved)
                    {
                        uri = resp.Headers.Location; //Resource has been relocated so set URI and loop around
                    }

                    if (resp.StatusCode == HttpStatusCode.Moved)
                    {
                        uri = new Uri(url);
                        if (!uri.Authority.Contains(".tumblr."))
                        {
                            Blog.Url = uri.GetLeftPart(UriPartial.Authority);
                        }
                    }
                } while ((resp.StatusCode == HttpStatusCode.Found || resp.StatusCode == HttpStatusCode.Moved) &&
                         redirectCount++ < 5);

                if (resp.StatusCode == HttpStatusCode.Found)
                {
                    throw new WebException("Too many automatic redirections were attempted.",
                        WebExceptionStatus.ProtocolError);
                }

                string content;
                return content = await resp.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                Debugger.Log(65, "BasicInfoGetRequest", "Error retrieving basic info");
                throw;
            }
        }


        protected async Task<string> RequestDataAsync(
            string url,
            Dictionary<string, string> headers = null,
            IEnumerable<string> cookieHosts = null)
        {
            CancellationTokenRegistration requestRegistration = new CancellationTokenRegistration();
            try
            {
                int redirects = 0;
                ResponseDetails responseDetails;
                do
                {
                    HttpWebRequest request = WebRequestFactory.CreateGetRequest(url, string.Empty, headers, false);
                    cookieHosts ??= new List<string>();
                    string cookieDomain = null;
                    foreach (string cookieHost in cookieHosts)
                    {
                        cookieDomain ??= new Uri(cookieHost).Host;
                        CookieService.GetUriCookie(request.CookieContainer, new Uri(cookieHost));
                    }

                    requestRegistration = Ct.Register(request.Abort);
                    responseDetails = await WebRequestFactory.ReadRequestToEnd2Async(request, cookieDomain);
                    url = responseDetails.RedirectUrl ?? url;
                    if (responseDetails.HttpStatusCode == HttpStatusCode.Found)
                    {
                        if (url.Contains("privacy/consent"))
                        {
                            string ex = "Acceptance of privacy consent needed!";
                            App.ShowError(ex);
                        }

                        if (!url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                        {
                            url = request.RequestUri.GetLeftPart(UriPartial.Authority) + url;
                        }
                    }

                    if (responseDetails.HttpStatusCode == HttpStatusCode.Moved)
                    {
                        Uri uri = new Uri(url);
                        if (!uri.Authority.Contains(".tumblr."))
                        {
                            Blog.Url = uri.GetLeftPart(UriPartial.Authority);
                        }
                    }
                } while ((responseDetails.HttpStatusCode == HttpStatusCode.Found ||
                          responseDetails.HttpStatusCode == HttpStatusCode.Moved) && redirects++ < 5);

                if (responseDetails.HttpStatusCode == HttpStatusCode.Found)
                {
                    App.ShowError("Too many automatic redirections were attempted.");
                }

                return responseDetails.Response;
            }
            catch (WebException wx)
            {
                s_logger.LogError("A WebException has been thrown");
                s_logger.LogError(wx.Message);
            }
            catch (Exception e)
            {
                App.ShowError($"AbstractCrawler.RequestDataAsync: {e.Message}");
            }
            finally
            {
                requestRegistration.Dispose();
            }

            return string.Empty;
        }


        protected async Task<string> RequestApiDataAsync(
            string url,
            string bearerToken,
            Dictionary<string, string> headers = null,
            IEnumerable<string> cookieHosts = null)
        {
            CancellationTokenRegistration requestRegistration = new CancellationTokenRegistration();
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateGetRequest(url, string.Empty, headers);
                cookieHosts ??= new List<string>();
                foreach (string cookieHost in cookieHosts)
                {
                    CookieService.GetUriCookie(request.CookieContainer, new Uri(cookieHost)); // adds cookies to request
                }

                request.PreAuthenticate = true;
                request.Headers.Add("Authorization", "Bearer " + bearerToken);
                request.Accept = "application/json";
                requestRegistration = Ct.Register(request.Abort);
                return await WebRequestFactory.ReadRequestToEndAsync(request);
            }
            finally
            {
                requestRegistration.Dispose();
            }
        }


        protected async Task<string> PostDataAsync(
            string url,
            string referer,
            Dictionary<string, string> parameters,
            IEnumerable<string> cookieHosts = null)
        {
            CancellationTokenRegistration requestRegistration = new CancellationTokenRegistration();
            try
            {
                HttpWebRequest request = WebRequestFactory.CreatePostRequest(url, referer);
                cookieHosts ??= new List<string>();
                foreach (string cookieHost in cookieHosts)
                {
                    CookieService.GetUriCookie(request.CookieContainer, new Uri(cookieHost));
                }

                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                requestRegistration = Ct.Register(request.Abort);
                await WebRequestFactory.PerformPostRequestAsync(request, parameters);
                string document = await WebRequestFactory.ReadRequestToEndAsync(request);
                return document;
            }
            finally
            {
                requestRegistration.Dispose();
            }
        }


        protected static string UrlEncode(IDictionary<string, string> parameters)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> val in parameters)
            {
                _ = sb.AppendFormat("{0}={1}&", val.Key, HttpUtility.UrlEncode(val.Value));
            }

            _ = sb.Remove(sb.Length - 1, 1); // remove last '&'
            return sb.ToString();
        }


        protected virtual IEnumerable<int> GetPageNumbers()
        {
            return string.IsNullOrEmpty(Blog.DownloadPages)
                ? Enumerable.Range(0, App.Settings.ConcurrentScans)
                : RangeToSequence(Blog.DownloadPages);
        }


        protected static bool TestRange(int numberToCheck, int bottom, int top)
        {
            return numberToCheck >= bottom && numberToCheck <= top;
        }


        protected static IEnumerable<int> RangeToSequence(string input)
        {
            string[] parts = input.Split(',');
            foreach (string part in parts)
            {
                if (!part.Contains('-'))
                {
                    yield return int.Parse(part);
                    continue;
                }

                string[] rangeParts = part.Split('-');
                int start = int.Parse(rangeParts[0]);
                int end = int.Parse(rangeParts[1]);
                while (start <= end)
                {
                    yield return start;
                    start++;
                }
            }
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected static ulong GetLastPostId()
        {
            return Blog.ForceRescan ? 0 : !string.IsNullOrEmpty(Blog.DownloadPages) ? 0 : Blog.LastId;
        }


        protected void GenerateTags()
        {
            if (!string.IsNullOrWhiteSpace(Blog.Tags))
            {
                Tags = Blog.Tags.Split(',').Select(x => x.Trim()).ToList();
            }
        }


        /// <summary>
        ///     Updates the blog file statistical counts for the blog being crawled
        /// </summary>
        /// <param name="add"></param>
        protected void UpdateBlogStats(bool add)
        {
            if (add)
            {
                Blog.TotalCount += StatisticsBag.Count;
                Blog.Photos += StatisticsBag.Count(post => post.GetType() == typeof(PhotoPost));
                Blog.Videos += StatisticsBag.Count(post => post.GetType() == typeof(VideoPost));
            }
            else
            {
                Blog.TotalCount = StatisticsBag.Count;
                Blog.Photos = StatisticsBag.Count(post => post.GetType() == typeof(PhotoPost));
                Blog.Videos = StatisticsBag.Count(post => post.GetType() == typeof(VideoPost));
            }

            Blog.Save();
        }


        protected int DetermineDuplicates<T>()
        {
            int dupe = StatisticsBag.Where(entry => entry.GetType() == typeof(T)).GroupBy(id => id.Id)
                .Where(g => g.Count() > 1).Sum(g => g.Count() - 1);


            int duped = StatisticsBag.Where(url => url.GetType() == typeof(T)).GroupBy(url => url.Url)
                .Where(g => g.Count() > 1).Sum(g => g.Count() - 1);

            return dupe > 0 ? dupe : duped;
        }


        protected void ClearCollectedBlogStatistics()
        {
            StatisticsBag.Clear();
        }


        protected static void HandleTimeoutException(TimeoutException timeoutException, string duringAction)
        {
            s_logger.LogError("{0}, {1}",
                string.Format(CultureInfo.CurrentCulture, Resources.TimeoutReached, duringAction, Blog.Name),
                timeoutException?.Message);
            s_logger.LogDebug($"Timeout:::{Resources.TimeoutReached}, {duringAction}, {Blog.Name}");
        }


        protected static bool HandleNotFoundWebException(WebException webException)
        {
            HttpWebResponse resp = (HttpWebResponse)webException?.Response;
            if (resp == null || resp.StatusCode != HttpStatusCode.NotFound)
            {
                return false;
            }

            s_logger.LogError("{0}, {1}", string.Format(CultureInfo.CurrentCulture, Resources.BlogIsOffline, Blog.Name),
                webException.Message);
            // Settings.ShowError(webException, Resources.BlogIsOffline, Blog.Name);
            return true;
        }


        protected static bool HandleLimitExceededWebException(WebException webException)
        {
            HttpWebResponse resp = (HttpWebResponse)webException?.Response;
            if (resp == null || (int)resp.StatusCode != 429)
            {
                return false;
            }

            s_logger.LogError("{0}, {1}", string.Format(CultureInfo.CurrentCulture, Resources.LimitExceeded, Blog.Name),
                webException.Message);
            AppSettings.ShowError(webException, Resources.LimitExceeded, Blog.Name);
            return true;
        }


        protected static bool HandleUnauthorizedWebException(WebException webException)
        {
            HttpWebResponse resp = (HttpWebResponse)webException?.Response;
            if (resp == null || resp.StatusCode != HttpStatusCode.Unauthorized)
            {
                return false;
            }

            s_logger.LogError("{0}, {1}",
                string.Format(CultureInfo.CurrentCulture, Resources.PasswordProtected, Blog.Name),
                webException.Message);
            AppSettings.ShowError(webException, Resources.PasswordProtected, Blog.Name);
            return true;
        }


        protected static string BuildParms(
            long activeBlog = 1616730,
            int pageNum = 0,
            string loginToken = "",
            int pageSz = 0,
            string dtSearchField = "",
            string vblogIx = "0")
        {
            //  return new StringContent(sb.ToString());
            string json =
                $$"""json={"Params":["[{IPADDRESS}]","{{loginToken}}",null,0,0,{{activeBlog}},null,null,{{pageNum}},50,0,null,0,"",0,0,0,0,0,{{vblogIx}},null]}""";
            return json;
        }


        protected struct ApiHostUri
        {
            public const string TagsFollowed = "https://api-ro.newtumbl.com/sp/NewTumbl/get_User_FollowTags";
            public const string SearchDashPost = "https://api-ro.newtumbl.com/sp/NewTumbl/search_Dash_Posts";
            public const string SearchSiteBlogs = "https://api-ro.newtumbl.com/sp/NewTumbl/search_Site_Blogs";
            public const string SearchSitePosts = "https://api-ro.newtumbl.com/sp/NewTumbl/search_Site_Posts";
            public const string SearchBlogPosts = "https://api-ro.newtumbl.com/sp/NewTumbl/search_Blog_Posts";
            public const string UserPostsLike = "https://api-ro.newtumbl.com/sp/NewTumbl/search_User_Posts_Like";
            public const string UserPostsFaves = "https://api-ro.newtumbl.com/sp/NewTumbl/search_User_Posts_Favorite";

            public const string UserBlogsFollow =
                "https://api-rw.newtumbl.com/sp/NewTumbl/search_User_Blogs_Follow_Aux";

            public const string GetUserSettings = "https://api-rw.newtumbl.com/sp/NewTumbl/get_User_Settings";
            public const string GetBlogMarquee = "https://api-ro.newtumbl.com/sp/NewTumbl/get_Blog_Marquee";
        }
    }
}