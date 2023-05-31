// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// CookieService.csCookieService.cs0324202311:52 PM


using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using JetBrains.Annotations;
using PuppeteerSharp;

namespace ScraperOne.Services
{
    public class CookieService : ServiceBase
    {
        private readonly int InstantionCount;

        public CookieService()
        {
            InstantionCount++;
            CookieJar = new CookieContainer(200, 35, 7000);
            Console.WriteLine("Cookie Service has started");
            PopulateCookiesFromDisk();
        }


        public CookieContainer CookieJar { get; set; }


        public static TaskCompletionSource<bool> CookieLoadingComplete { get; } = new();

        [CanBeNull]
        public string GetLoginToken => GetCookieByName("LoginToken").Value;

        private void PopulateCookiesFromDisk()
        {
            string cookiesFileName = "Cookies.json";
            string path = Path.Combine(App.Settings.ProjectFolder, cookiesFileName);
            List<Cookie> cookies = LoadSettings<List<Cookie>>(path);
            SetUriCookie(cookies);
            CookieLoadingComplete.SetResult(true);
        }


        public IEnumerable<Cookie> GetAllCookies()
        {
            Hashtable k = (Hashtable)CookieJar.GetType()
                .GetField("m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(CookieJar);
            foreach (DictionaryEntry element in k)
            {
                SortedList l = (SortedList)element.Value.GetType()
                    .GetField("m_list", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(element.Value);
                foreach (object e in l)
                {
                    CookieCollection cl = (CookieCollection)((DictionaryEntry)e).Value;
                    foreach (Cookie fc in cl.Cast<Cookie>())
                    {
                        if (fc.Expires.Equals(DateTime.MinValue) && fc.Expires.Kind == DateTimeKind.Unspecified)
                        {
                            fc.Expires = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        }

                        yield return fc;
                    }
                }
            }
        }


        public void GetUriCookie(CookieContainer request, Uri uri)
        {
            foreach (Cookie cookie in CookieJar.GetCookies(uri).Cast<Cookie>())
            {
                request.Add(cookie);
            }
        }


        public static void SetAuthCookie([CanBeNull] object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;

            SetAuthCookies().Wait();

            _ = are.Set();
        }


        public static async Task SetAuthCookies()
        {
            LoginService ls = new();
            try
            {
                // If login is successful set date to eliminate unnecessary logins


                if (await ls.AuthenticateNewtumblBrowserAsync())
                {
                    App.Settings.LastLoginDate = DateTime.Now;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured trying to log in");
            }
            finally
            {
                CookieLoadingComplete.SetResult(true);
                ls.Dispose();
            }
        }


        public void RemoveUriCookie(Uri uri)
        {
            CookieCollection cookies = CookieJar.GetCookies(uri);
            foreach (Cookie cookie in cookies.Cast<Cookie>())
            {
                cookie.Expired = true;
            }
        }


        public void SetSingleCookie(Cookie cookie)
        {
            CookieJar.Add(cookie);
        }


        public void SetUriCookie(IEnumerable cookies)
        {
            if (cookies != null)
            {
                foreach (Cookie cookie in cookies)
                {
                    try
                    {
                        CookieJar.Add(cookie);
                    }
                    catch (CookieException e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }


        public void AddCookies(CookieParam[] cookies)
        {
            foreach (CookieParam cookie in cookies)
            {
                Cookie cook = new Cookie
                {
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Domain = cookie.Domain,
                    Expires = DateTimeOffset.FromUnixTimeSeconds((long)cookie.Expires).LocalDateTime,
                    Expired = false,
                    Path = cookie.Path,
                    HttpOnly = false,
                    Secure = (bool)cookie.Secure
                };
                SetSingleCookie(cook);
            }
        }

        [CanBeNull]
        internal Cookie FindCookieByName(string v)
        {
            CookieCollection cookies = CookieJar.GetAllCookies();
            Cookie cook = cookies.FirstOrDefault(c => c.Name == v);
            return cook;
        }


        public static void SaveCookiesToFiles()
        {
            string cookiesFileName = "Cookies.json";
            SaveSettings(Path.Combine(Environment.CurrentDirectory, cookiesFileName),
                new List<Cookie>(CookieService.GetAllCookies()));
        }

        [CanBeNull]
        internal Cookie GetCookieByName(string v)
        {
            return FindCookieByName(v);
        }
    }
}