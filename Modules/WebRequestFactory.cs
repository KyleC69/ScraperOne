// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// WebRequestFactory.csWebRequestFactory.cs032320233:30 AM

#pragma warning disable SYSLIB0014


using System.Net;
using System.Text;
using System.Web;
using ScraperOne.Modules.Crawlers;

namespace ScraperOne.Modules
{
    public interface IWebRequestFactory
    {
        HttpWebRequest CreateGetRequest(
            string url,
            string referer = "",
            Dictionary<string, string> headers = null,
            bool allowAutoRedirect = true);


        HttpWebRequest CreateGetXhrRequest(string url, string referer = "", Dictionary<string, string> headers = null);

        HttpWebRequest CreatePostRequest(string url, string referer = "", Dictionary<string, string> headers = null);

        HttpWebRequest CreatePostXhrRequest(string url, string referer = "", Dictionary<string, string> headers = null);

        Task PerformPostRequestAsync(HttpWebRequest request, Dictionary<string, string> parameters);

        Task PerformPostXhrRequestAsync(HttpWebRequest request, string requestBody, bool useUtf8);

        Task<ResponseDetails> ReadRequestToEnd2Async(HttpWebRequest request, string cookieDomain);

        Task<string> ReadRequestToEndAsync(HttpWebRequest request, bool storeCookies = false);

        Task<bool> RemotePageIsValidAsync(string url);

        string UrlEncode(IDictionary<string, string> parameters);
    }

    public class WebRequestFactory : IWebRequestFactory
    {


        public HttpWebRequest CreateGetRequest(
            string url,
            string referer = "",
            Dictionary<string, string> headers = null,
            bool allowAutoRedirect = true)
        {
            HttpWebRequest request = CreateStubRequest(url, referer, headers, allowAutoRedirect);
            request.Method = "GET";
            return request;
        }


        public HttpWebRequest CreateGetXhrRequest(
            string url,
            string referer = "",
            Dictionary<string, string> headers = null)
        {
            HttpWebRequest request = CreateStubRequest(url, referer, headers);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            return request;
        }


        public HttpWebRequest CreatePostRequest(string url, string referer = "", Dictionary<string, string> headers = null)
        {
            HttpWebRequest request = CreateStubRequest(url, referer, headers);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }


        public HttpWebRequest CreatePostXhrRequest(
            string url,
            string referer = "",
            Dictionary<string, string> headers = null)
        {
            HttpWebRequest request = CreatePostRequest(url, referer, headers);
            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            return request;
        }


        public async Task PerformPostRequestAsync(HttpWebRequest request, Dictionary<string, string> parameters)
        {
            string requestBody = UrlEncode(parameters);
            /*
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var myWebHeaderCollection = response.Headers;
            for (int i = 0; i < myWebHeaderCollection.Count; i++)
            {
                String header = myWebHeaderCollection.GetKey(i);
                String[] values = myWebHeaderCollection.GetValues(header);
                if (values.Length > 0)
                {
                    Console.WriteLine("The values of {0} header are : ", header);
                    for (int j = 0; j < values.Length; j++)
                        Console.WriteLine("\t{0}", values[j]);
                }
                else
                {
                    Console.WriteLine("There is no value associated with the header");
                }
            }*/
            using Stream postStream = await request.GetRequestStreamAsync();

            //Console.WriteLine("Is the response from the cache? {0}", response.IsFromCache);
            byte[] postBytes = Encoding.ASCII.GetBytes(requestBody);
            await postStream.WriteAsync(postBytes);
            await postStream.FlushAsync();
        }


        public async Task PerformPostXhrRequestAsync(HttpWebRequest request, string requestBody, bool useUtf8)
        {
            using Stream postStream = await request.GetRequestStreamAsync();
            byte[] postBytes = useUtf8 ? Encoding.UTF8.GetBytes(requestBody) : Encoding.ASCII.GetBytes(requestBody);
            await postStream.WriteAsync(postBytes);
            await postStream.FlushAsync();
        }


        public async Task<bool> RemotePageIsValidAsync(string url)
        {
            HttpWebRequest request = CreateStubRequest(url);
            request.Method = "HEAD";
            request.AllowAutoRedirect = false;
            HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
            response.Close();
            return response.StatusCode == HttpStatusCode.OK;
        }


        public async Task<string> ReadRequestToEndAsync(HttpWebRequest request, bool storeCookies = false)
        {
            using HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
            if (storeCookies)
            {
                //CookieService.SetUriCookie(response.Cookies);
            }

            using Stream stream = response.GetResponseStream();
            using BufferedStream buffer = new(stream);
            using StreamReader reader = new(buffer);
            return reader.ReadToEnd();
        }


        public async Task<ResponseDetails> ReadRequestToEnd2Async(HttpWebRequest request, string cookieDomain)
        {
            using HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
            if (response.Headers.AllKeys.Contains("Set-Cookie"))
            {
                //CookieService.SetUriCookie(
                  //  CookieParser.GetAllCookiesFromHeader(response.Headers["Set-Cookie"], cookieDomain));
            }

            if (response.StatusCode is HttpStatusCode.Found or HttpStatusCode.Moved)
            {
                response.Close();
                return new ResponseDetails
                {
                    HttpStatusCode = response.StatusCode,
                    RedirectUrl = response.Headers["Location"]
                };
            }

            using Stream stream = response.GetResponseStream();
            using BufferedStream buffer = new(stream);
            using StreamReader reader = new(buffer);
            string content = reader.ReadToEnd();
            return new ResponseDetails { HttpStatusCode = response.StatusCode, Response = content };
        }


        public string UrlEncode(IDictionary<string, string> parameters)
        {
            StringBuilder sb = new();
            foreach (KeyValuePair<string, string> val in parameters)
            {
                _ = sb.AppendFormat("{0}={1}&", val.Key, HttpUtility.UrlEncode(val.Value));
            }

            _ = sb.Remove(sb.Length - 1, 1); // remove last '&'
            return sb.ToString();
        }


        private static HttpWebRequest CreateStubRequest(
            string url,
            string referer = "",
            Dictionary<string, string> headers = null,
            bool allowAutoRedirect = true)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url); //HttpUtility.UrlDecode(url) what was the use case!?
            request.ProtocolVersion = HttpVersion.Version11;
            request.UserAgent = App.Settings.UserAgent;
            request.AllowAutoRedirect = allowAutoRedirect;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.KeepAlive = true;
            request.Pipelined = true;

            // Timeouts don't work with GetResponseAsync() as it internally uses BeginGetResponse.
            // See docs: https://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.timeout(v=vs.110).aspx
            // Quote: The Timeout property has no effect on asynchronous requests made with the BeginGetResponse or BeginGetRequestStream method.
            // TODO: Use HttpClient instead?

            //  request.ReadWriteTimeout = settings.TimeOut * 1000;
            //  request.Timeout = settings.TimeOut * 1000;
            request.CookieContainer = new CookieContainer { PerDomainCapacity = 100 };
            ServicePointManager.DefaultConnectionLimit = 400;
            //request = SetWebRequestProxy(request, settings);
            request.Referer = referer;
            headers ??= new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers[header.Key] = header.Value;
            }

            return request;
        }
    }
}