








///TODO: TEMPORARY WRAPPER TO BE REMOVED

using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using ScraperOne.Logger;

namespace ScraperOne.Services
{
    /// <summary>
    ///     Dependencty Injection Control Service
    /// </summary>
    public class DiServiceLoader : ServiceBase
    {
        private static ColorLogger s_logger;
        public static TaskCompletionSource<bool> DIServiceAppStartupComplete;

        public DiServiceLoader(
            ColorLogger logger,
            ManagerService managerService,
            CrawlerController crawlerController,
            QueueController queueController,
            QueueManager queueManager) : base(managerService, crawlerController, queueManager,
            queueController)
        {
            Guard.IsNotNull(logger);
            Guard.IsNotNull(managerService);
            Guard.IsNotNull(crawlerController);
            Guard.IsNotNull(queueController);
            Guard.IsNotNull(queueManager);


            DIServiceAppStartupComplete = new();
            Environment.CurrentDirectory = App.Settings.ProjectFolder;
            s_logger = logger;
            s_logger.LogInformation("DIServiceLoader::ctor::Ended");
        }

        /// <summary>
        ///     provides the LoggerFactory instance for any class to create their own ILogger instance from.
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnApplicationExiting(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            ShutDown();
            SaveSettings();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnApplicationStartup(object sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
        }

        public static async Task DoLogin()
        {
            LoginService ls = new();
            _ = await ls.AuthenticateNewtumblBrowserAsync();
        }

        private void OnSettingsUpdated(object sender, EventArgs e)
        {
            SaveSettings();
        }


        public static void Initialize()
        {
            try
            {
                s_logger.LogInformation("DIServiceLoader::ctor::Starting");
                string savePath = Environment.CurrentDirectory;
                string logPath = Environment.CurrentDirectory;
                s_logger.LogInformation("ScraperOne" + " start");
                s_logger.LogInformation("AppPath: {en}", Environment.CurrentDirectory);
                s_logger.LogInformation("AppSettingsPath: {pa}", Path.Combine(savePath, "Properties"));
                s_logger.LogInformation("LogFilename: {paa}", Path.Combine(logPath, "Scraper3.log"));
                s_logger.LogInformation("Version: {ver}", "1.00");
                s_logger.LogInformation("DIServiceLoader::ctor::ending");
                LoadSettings();

                /* App.ShowInfo(
                     $"There are ManagerService.BlogFilesView.Count {ManagerService.BlogFilesView.Count} blogs loaded for datagrid",
                     "AppStart-Final");*/
                //BlogManagerView.ItemsProperty.Value.BindCommand(ManagerService.BlogFilesView);
            }
            catch (Exception e)
            {
                App.ShowError(e.Message);
            }
            finally
            {
                //Save settings back to drive to include any changes made during startup
                SaveSettings();
            }
        }


        public static void ShutDown()
        {
            QueueController.Shutdown();
            CrawlerController.Shutdown();
            SaveSettings();
            s_logger.LogInformation("Scraper has been shutdown gracefully");
        }
    }
}