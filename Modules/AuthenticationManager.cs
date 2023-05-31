using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using JetBrains.Annotations;

using Newtonsoft.Json;

using ScraperOne.DataModels;
using ScraperOne.DataModels.NewTumbl;
using ScraperOne.Properties;
using ScraperOne.Services;

using Splat;

namespace ScraperOne.Modules
{
    public partial class AuthenticationManager
    {
        public static HttpClient _HttpClient;

        protected static readonly Regex sroExtractJsonFromPage;

        private readonly CookieService i_cookieService;
        private readonly IBlog i_blog;

        private int i_blogIx;


        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public AuthenticationManager([CanBeNull] IBlog blog = null)
        {
            i_cookieService = Locator.Current.GetService<CookieService>();
            i_blog = blog;
            _HttpClient = new HttpClient(GetHandler());
        }


        private HttpClientHandler GetHandler()
        {
            CookieContainer cookieJar = i_cookieService.CookieJar;
            HttpClientHandler h = new()
            {
                UseCookies = true,
                CookieContainer = cookieJar,
                PreAuthenticate = true,
                Credentials = new NetworkCredential("kcrow1969@gmail.com", "Angel1031"),
                AllowAutoRedirect = true
            };
            return h;
        }


        public async Task<bool> CheckIfUserLoggedInAsync()
        {
            try
            {
                string blogJson = await DoApiGetRequest();
                Root obj = ConvertJsonToClassNew<Root>(blogJson);
                ARow user = BlogInfo(obj, 3);
                ARow blogi = BlogInfo(obj, 2);
                if (((user?.bLoggedIn ?? 0) == 0 && blogi?.bRatingIx > 2) || user?.bRatingIx < blogi?.bRatingIx)
                {
                    string msg = user?.bRatingIx < blogi?.bRatingIx ? Resources.BlogOverrated : Resources.NotLoggedInNT;
                    //  var errorMsg = $"{Blog.Name} ({Settings.GetCollection(Blog.CollectionId).Name}): Not Logged In";

                    App.ShowError($"NewTumblCrawler:CheckIfLoggedInAsync: {msg}");
                    // HandleAuthenticationError(Blog);

                    return false;
                }

                return (user?.bLoggedIn ?? 0) == 1;
            }
            catch (WebException webException) when (webException.Status == WebExceptionStatus.RequestCanceled)
            {
                return true;
            }
            catch (TimeoutException)
            {
                //HandleTimeoutException(timeoutException, Resources.Crawling);
                return false;
            }
        }


        public static T ConvertJsonToClassNew<T>(string json) where T : new()
        {
            try
            {
                json = json.Replace(":undefined", ":null");
                using MemoryStream ms = new(Encoding.UTF8.GetBytes(json));
                JsonSerializer deserializer = new();
                deserializer.Converters.Add(new SingleOrArrayConverter<T>());
                using StreamReader sr = new(ms);
                using JsonTextReader jsonTextReader = new(sr);
                return deserializer.Deserialize<T>(jsonTextReader);
            }
            catch (JsonException)
            {
                App.ShowError("AbstractCrawler:ConvertJsonToClassNew<T>: Could not parse data");
                return new T();
            }
        }


        protected static ARow BlogInfo(Root obj, int type)
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
                        return obj.aResultSet[type].aRow[0];
                    //    return obj.aResultSet[type].aRow.Where(w => w.dwBlogIx == blogIx).First();
                    case 3:
                        return obj.aResultSet[type].aRow[0];
                    case 8:
                        // if (obj.aResultSet.Length < 9) return null;
                        return obj.aResultSet[type].aRow[0];
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }


        public async Task<string> DoApiGetRequest()
        {
            string json = null;
            Root obj = null;
            for (int retries = 0; retries < 2; retries++)
            {
                try
                {
                    string document = await BasicInfoGetRequest(i_blog.Url);
                    json = sroExtractJsonFromPage.Match(document).Groups[1].Value + "}";
                    obj = ConvertJsonToClassNew<Root>(json);
                    _ = CheckError(obj);
                    break;
                }
                catch (APIException ex)
                {
                    if (retries == 0)
                    {
                        App.ShowError($"GetApiPageAsync, retrying: {ex.Message}");
                        await Task.Delay(10000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (obj.aResultSet[2].aRow.Count == 0)
            {
                App.ShowError("API reports not found??");
            }

            if (obj.aResultSet[3].aRow[0].bLoggedIn == 0)
            {
                App.ShowError("API::bLoggedIn reports zero");
            }

            i_blogIx = i_blogIx > 0 ? i_blogIx : BlogInfo(obj, 8)?.dwBlogIx ?? 0;
            return json;
        }


        private static bool CheckError(Root obj)
        {
            bool status = false;
            if (obj.nResult == "-1")
            {
                status = true;
                App.ShowError(
                    $"server returned: {obj.aResultSet[0].aRow[0].szError} ({obj.aResultSet[0].aRow[0].dwError})");
                return status;
            }

            if (obj.nResult == "-9999")
            {
                status = true;
                App.ShowError("{obj.sError}({obj.sAPIErrorCode}): {obj.sAPIErrorMessage} MyFeed");
                return status;
            }

            return status;
        }


        /// <summary>
        ///     HttpClient version to Get Basic info.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookieHosts"></param>
        /// <returns>string</returns>
        public async Task<string> BasicInfoGetRequest(
            string url,
            Dictionary<string, string> headers = null,
            IEnumerable<string> cookieHosts = null)
        {
            int redirectCount = 0;
            HttpRequestMessage req;
            _ = new HttpResponseMessage();

            req = new HttpRequestMessage(HttpMethod.Get, new Uri(url))
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            try
            {
                Uri uri = new(url);
                HttpResponseMessage resp;
                do
                {
                    resp = await _HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None);
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
                            i_blog.Url = uri.GetLeftPart(UriPartial.Authority);
                        }
                    }
                } while ((resp.StatusCode == HttpStatusCode.Found || resp.StatusCode == HttpStatusCode.Moved) &&
                         redirectCount++ < 5);


                string content;
                return resp.StatusCode == HttpStatusCode.Found
                    ? throw new WebException("Too many automatic redirections were attempted.",
                        WebExceptionStatus.ProtocolError)
                    : (content = await resp.Content.ReadAsStringAsync());
            }
            catch (Exception)
            {
                Debugger.Log(65, "BasicInfoGetRequest", "Error retrieving basic info");
                throw;
            }
        }

       
    }
}