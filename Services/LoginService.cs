// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 


using System.Diagnostics;
using PuppeteerSharp;
using Splat;

namespace ScraperOne.Services
{
    /// <summary>
    /// </summary>
    public class LoginService : IDisposable
    {
        private readonly CookieService i_cookieService;


        private readonly object i_puppeteerLock = new();
        public TaskCompletionSource<bool> _LoginCompleted;
        private IBrowser i_puppetBrowser = null!;
        private IPage i_puppetPage = null!;


        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public LoginService()
        {
            _LoginCompleted = new TaskCompletionSource<bool>();
            i_cookieService = Locator.Current.GetService<CookieService>();
        }


        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            i_puppetBrowser?.Dispose();
            i_puppetPage = null;
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


        public static LaunchOptions GetLaunchOptions()
        {
            return new LaunchOptions
            {
                Headless = false,
                DefaultViewport = null,
                ExecutablePath = "/home/savaho/DevChrome/chrome-linux/chrome-wrapper",
                DumpIO = true,
                IgnoreDefaultArgs = true,
                Args = new[]
                {
                "%U", "--disable-gpu", "--user-data-dir=/home/savaho/Documents/chromedata",
                "--remote-debugging-port=5150", "--no-sandbox"
            }
            };
        }


        /// <summary>
        ///     Authenticates the user using a headless browser;
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AuthenticateNewtumblBrowserAsync()
        {
            App.ShowError("Authentication has been started");
            Debug.WriteLine("Authentication::Method:: Starting....");

            bool status = false;
            LaunchOptions lo = new()
            {
                ExecutablePath = "/storage/chrome/.local-chromium/Linux-1069273/chrome-linux/chrome",
                Headless = false,
                Timeout = 90_000,
                DumpIO = true,
                Args = new[] { "https://newtumbl.com/sign?in", "--disable-gpu-compositing", "--remote-debugging-port=5150" }
            };
            try
            {
                //Attempting to correct irregular behavior that may have to do with async context
                // var puppet = new PuppeteerSharp.FirefoxLauncher("/usr/bin/firefox",lo);
                // await puppet.StartAsync();

                await using (i_puppetBrowser = await Puppeteer.LaunchAsync(lo))
                await using (i_puppetPage = await i_puppetBrowser.NewPageAsync())
                {
                    _ = await i_puppetPage.GoToAsync("https://newtumbl.com/sign?in", WaitUntilNavigation.Networkidle2);

                    await i_puppetPage.WaitForXPathAsync("//input[@tabindex='1']").Result.TypeAsync("kcrow1969@gmail.com");
                    await i_puppetPage.WaitForXPathAsync("//input[@tabindex='2']").Result.TypeAsync("Angel1031");
                    await i_puppetPage.Keyboard.PressAsync("Enter");
                    await Task.Delay(6000);
                    CookieParam[] cookies = await i_puppetPage.GetCookiesAsync();
                    i_cookieService.AddCookies(cookies);
                    CookieService.SaveCookiesToFiles();
                    status = true;
                    App.ShowError("Authentication has reported successful");
                    Debug.WriteLine("Authentication::Method:: Method ended witout exception");
                }

                return status;
            }
            catch (TimeoutException)
            {
                App.ShowError("Timeout expired during authentication", "Auth failure");
                Debug.WriteLine("Authentication::Method:: Task Timeout");

            }
            catch (Exception)
            {
                App.ShowError("Timeout expired during authentication", "Auth failure");
                Debug.WriteLine("Authentication::Method:: Exception Thrown");
            }
            finally
            {
                i_puppetBrowser?.Dispose();
                i_puppetBrowser = null;
                i_puppetPage = null;
                _LoginCompleted.SetResult(true);
                Debug.WriteLine("Authentication::Method:: Ended");
            }


            return status;
        }

        private void OnDisconnectPuppet(object sender, EventArgs e)
        {
            Console.WriteLine(i_puppetPage.Client.CloseReason);
        }


        private void OnPageError(object sender, PageErrorEventArgs e)
        {
            Console.WriteLine(
                "A page error was detected during a hidden browser activity. Last requested action has been aborted, please try again.");
        }
    }
}