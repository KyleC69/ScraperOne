// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ICrawler.csICrawler.cs032320232:48 PM


using ScraperOne.DataModels.NewTumbl;

namespace ScraperOne.Modules.Crawlers
{
    public interface ICrawler
    {
        Task CrawlAsync();
        void Dispose();
        Task<Root> DoApiGetRequest();
        Task<string> GetApiPageAsync(int mode);
        Task IsBlogOnlineAsync();
        Task<string> GetBasicBlogInfo(string url);

        void InterruptionRequestedEventHandler(object sender, EventArgs e);
        void UpdateProgressQueueInformation(int nposts, int totalPosts);
        Task<List<ARow>> GetFollowedBlogsAsync();
    }
}