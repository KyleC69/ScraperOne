// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// BrowserControl.csBrowserControl.cs0323202310:42 PM


using System.Diagnostics;

using PuppeteerSharp;

namespace ScraperOne.Modules
{
    [DebuggerDisplay("Count = {i_count}")]
    public class BrowserControl
    {
        private readonly int i_count;


        public BrowserControl()
        {
            Console.WriteLine($@"Browser Control Started {i_count}");
            i_count++;
        }


        public static string DownloadPath => "/home/savaho/Documents/CustomChromium";

        private static async void OnBrowserDisconnected(object sender, EventArgs e)
        {
            await Task.CompletedTask;
        }


        public static async Task<IBrowser> ConnectToOpenBrowserAsync()
        {
            try
            {
                return await Puppeteer.ConnectAsync(GetConnectOptions());
            }
            catch
            {
                return await Puppeteer.LaunchAsync(GetLaunchOptions());
            }
        }


        public static async Task DownloadBrowser()
        {
            Console.WriteLine(@"This example downloads the default version of Chromium to a custom location");
            _ = Directory.GetCurrentDirectory();
            Console.WriteLine(@"GetCurrentDirectory reports: {gcd}");
            Console.WriteLine(Environment.CurrentDirectory);
            Console.WriteLine(@"Downloading Chromium");
            _ = new BrowserFetcherOptions { Product = Product.Chrome, Path = "/home/savaho/DevChrome" };
            // using var temp = new BrowserFetcher(val);
            //   await temp.DownloadAsync();
            LaunchOptions lo = new()
            {
                ExecutablePath = "/home/savaho/DevChrome/chrome-linux/chrome",
                DefaultViewport = null,
                UserDataDir = "/home/savaho/Documents/userdata",
                Headless = false
            };
            IBrowser browser = await Puppeteer.LaunchAsync(lo);
            IPage page = await browser.NewPageAsync();
            _ = await page.GoToAsync("https://newtumbl.com");
            await Task.Delay(2000);
            _ = await page.GetCookiesAsync();
            Console.WriteLine(@"Browser download complete");
            Environment.Exit(0);
        }


        public static async Task<IBrowser> GetBrowserObjAsync()
        {
            return await Puppeteer.LaunchAsync(GetLaunchOptions());
        }


        public static ConnectOptions GetConnectOptions()
        {
            return new ConnectOptions
            {
                BrowserWSEndpoint = "",
                BrowserURL = "http://localhost:5150/json",
                DefaultViewport = null
                //InitAction = WireUpEvents,
            };
        }


        public static async Task<IBrowser> GetHeadlessBrowserObjAsync()
        {
            return await Puppeteer.LaunchAsync(GetHeadlessLaunchOptions());
        }


        public static LaunchOptions GetHeadlessLaunchOptions()
        {
            return new LaunchOptions
            {
                Headless = true,
                DefaultViewport = null,
                ExecutablePath = "/home/savaho/Documents/CustomChromium/Linux-1069273/chrome-linux",
                UserDataDir = "/home/savaho/Documents/chromedata"
            };
        }


        public static LaunchOptions GetLaunchOptions()
        {
            return new LaunchOptions
            {
                IgnoreHTTPSErrors = false,
                Headless = true,
                DefaultViewport = null,
                EnqueueTransportMessages = false,
                Product = Product.Chrome,
                EnqueueAsyncMessages = false,
                TargetFilter = null,
                ExecutablePath = "/storage/chrome/chrome-linux/chrome-wrapper",
                SlowMo = 0,
                UserDataDir = "/storage/chrome/chromedata",
                Devtools = false,
                //DumpIO = true,
                LogProcess = true,
                IgnoreDefaultArgs = false,
                IgnoredDefaultArgs = new string[]
                {
                },
                WebSocketFactory = null,
                TransportFactory = null,
                Timeout = 60_000,
                DumpIO = false,


                //IgnoreDefaultArgs = true,
                Args = new[]
                {
                    "%U",
                    "--disable-gpu",
                    "--user-data-dir=/home/savaho/Documents/chromedata",
                    "--remote-debugging-port=5150"
                }
            };
        }


        public static async Task<string> LoadPageGetContentAsync(string location)
        {
            IBrowser browser = await GetBrowserObjAsync();
            IPage page = await browser.NewPageAsync();
            if (page is null)
            {
                return string.Empty;
            }

            _ = await page.GoToAsync(location, WaitUntilNavigation.Networkidle2);
            _ = await page.EvaluateExpressionAsync("window.scrollTo(0,document.body.scrollHeight)");
            _ = await page.EvaluateExpressionAsync("window.scrollTo(0,document.body.scrollHeight)");
            await Task.Delay(5000);
            return await page.GetContentAsync();
        }


        /// <summary>
        ///     Encapsulates several task. Starts local browser, navigates to tghe needed page
        ///     and then returns the page content back to the caller.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static async Task<string> LoadTargetPageGetContentAsync(string location)
        {

            LaunchOptions lo = new()
            {
                ExecutablePath = "/storage/chrome/chrome-linux/chrome",
                Headless = true,
                Timeout = 90_000,
                DumpIO = true,
                Args = new[] { "https://newtumbl.com/sign?in", "--disable-gpu-compositing", "--remote-debugging-port=5150" }
            };
            string result = string.Empty;
            IBrowser i_puppetBrowser = null;
            IPage i_puppetPage = null;

            try
            {

                i_puppetBrowser = await Puppeteer.LaunchAsync(lo);

                i_puppetPage = await i_puppetBrowser.NewPageAsync();

                _ = await i_puppetPage.GoToAsync("https://newtumbl.com/sign?in", WaitUntilNavigation.Networkidle2);
                await Task.Delay(5000);


                if (i_puppetPage.Url.EndsWith("feed"))
                {
                    _ = await i_puppetPage.GoToAsync(location, WaitUntilNavigation.Networkidle2);
                    result = await i_puppetPage.GetContentAsync();
                    return result;
                }
                else
                {

                    await i_puppetPage.WaitForXPathAsync("//input[@tabindex='1']").Result.TypeAsync("kcrow1969@gmail.com");
                    await i_puppetPage.WaitForXPathAsync("//input[@tabindex='2']").Result.TypeAsync("Angel1031");
                    await i_puppetPage.Keyboard.PressAsync("Enter");
                    await Task.Delay(6000);
                    _ = await i_puppetPage.GoToAsync(location, WaitUntilNavigation.Networkidle2);
                    await Task.Delay(4000);
                    result = await i_puppetPage.GetContentAsync();
                    return result;
                }
            }
            catch
            {
                // throw new ScraperException("Page load failure. Task was aborted.");
            }
            finally
            {
                i_puppetPage.Dispose();
                i_puppetBrowser.Dispose();
            }



            return result;
        }


    }
}