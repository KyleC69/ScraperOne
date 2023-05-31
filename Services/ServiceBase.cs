// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ServiceBase.csServiceBase.cs032320233:30 AM


using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using ScraperOne.Logger;
using ScraperOne.Properties;
using Splat;

namespace ScraperOne.Services
{
    public abstract class ServiceBase
    {
        private const string AppSettingsFileName = "Settings.json";
        private const string QueueSettingsFileName = "Queuelist.json";
        private const string CookiesFileName = "Cookies.json";

        protected static SettingsProvider _SettingsProvider = new();


        protected ServiceBase()
        {
        }


        protected ServiceBase(
            ManagerService managerService,
            CrawlerController crawlerController,
            QueueManager queueManager,
            QueueController queueController)
        {
            QueueSettings =
                LoadSettings<QueueSettings>(Path.Combine(Environment.CurrentDirectory, QueueSettingsFileName));

            ManagerService = managerService;
            QueueController = queueController;
            QueueManager = queueManager;
            CrawlerController = crawlerController;

            CookieService = Locator.Current.GetService<CookieService>();
        }

        public static ColorLoggerProvider LoggerFactory { get; } = new ColorLoggerProvider();


        // public static AppSettings AppSettings { get; set; }
        // public static List<Cookie> CookieList { get; set; }
        public static QueueSettings QueueSettings { get; private set; }
        public static CookieService CookieService { get; set; }
        public static CrawlerController CrawlerController { get; set; }


        public static ManagerService ManagerService { get; set; }
        public static QueueController QueueController { get; set; }
        public static QueueManager QueueManager { get; set; }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;


        public static void SaveSettings()
        {
            SaveSettings(Path.Combine(Environment.CurrentDirectory, AppSettingsFileName), App.Settings);
            SaveSettings(Path.Combine(Environment.CurrentDirectory, QueueSettingsFileName), QueueSettings);
            SaveSettings(Path.Combine(Environment.CurrentDirectory, CookiesFileName),
                new List<Cookie>(CookieService.GetAllCookies()));
        }

        public static void LoadSettings()
        {
            //AppSettings = LoadSettings<AppSettings>(Path.Combine(Environment.CurrentDirectory, AppSettingsFileName));
            QueueSettings =
                LoadSettings<QueueSettings>(Path.Combine(Environment.CurrentDirectory, QueueSettingsFileName));
            List<Cookie> cookies =
                LoadSettings<List<Cookie>>(Path.Combine(Environment.CurrentDirectory, CookiesFileName));
            CookieService.SetUriCookie(cookies);
        }


        protected static T LoadSettings<T>(string fileName) where T : class, new()
        {
            try
            {
                return _SettingsProvider.LoadSettings<T>(fileName);
            }
            catch (Exception)
            {
                return new T();
            }
        }


        /// <summary>
        ///     Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }


        /// <summary>
        ///     Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="propertyName">
        ///     The property name of the property that has changed.
        ///     This optional parameter can be skipped because the compiler is able to create it automatically.
        /// </param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }


        protected static void SaveSettings(string fileName, object settings)
        {
            try
            {
                _SettingsProvider.SaveSettings(fileName, settings);
            }
            catch (Exception)
            {
                Debug.Print("Error saving settings file {0}", fileName);
            }
        }


        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}