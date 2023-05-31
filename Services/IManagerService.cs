using System.Collections.ObjectModel;
using System.ComponentModel;

using ScraperOne.DataModels;
using ScraperOne.DataModels.Files;
using ScraperOne.DataModels.NewTumbl;

namespace ScraperOne.Services
{
#pragma warning disable CS0169, CS0649
    public interface IManagerService
    {
        ObservableCollection<IBlog> BlogFiles { get; }
        TaskCompletionSource<bool> ManagerServiceLoaded { get; }
        TaskCompletionSource<bool> DatabasesLoaded { get; set; }
        TaskCompletionSource<bool> LibraryLoaded { get; set; }
        event ManagerService.FinishedCrawlingLastBlogEventHandler FinishedCrawlingLastBlog;
        bool CheckIfFileExistsInDb(string filename, bool checkOriginalLinkFirst, bool checkArchive);
        void RemoveDatabase(IFiles database);
        void AddFileDatabase(IFiles database);
        void ClearDatabases();
        Task<IBlog> AddBlogAsync(string blogUrl);

        /// <summary>
        ///     Method starts the loading of the data for Blogs and QueueMaanager
        /// </summary>
        /// <returns>Task</returns>
        Task LoadDataBasesAsync();

        /// <summary>
        ///     Loads a list of files already downloaded
        /// </summary>
        /// <returns>Task</returns>
        Task LoadCompletedFilesDatabaseAsync();

        void Enqueue(IEnumerable<IBlog> blogFiles);
        void Enqueue(IBlog iBlog);
        void EnqueueAutoDownload();
        Task ImportFollowedBlogsAsync();
        List<ARow> GetBlogs(Root obj, int type);
        Task<IReadOnlyList<IBlog>> GetIBlogsAsync(string directory);
        bool CheckIfFileExistsInDbNew(TumblrPost downloadItem);
        Task UpdateMetaInformationAsync(IBlog blog, IProgress<DownloadProgress> p);
        Task UpdateMetaInformationAsync(IBlog blog);

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;
    }
}