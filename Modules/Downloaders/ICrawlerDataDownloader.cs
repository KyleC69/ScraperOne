// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ICrawlerDataDownloader.csICrawlerDataDownloader.cs032320233:29 AM


#region Interface Implementations

namespace ScraperOne.Modules.Downloaders;

#region Interface Implementations

public interface ICrawlerDataDownloader
{
    #region Methods

    Task DownloadCrawlerDataAsync();

    void ChangeCancellationToken(CancellationToken ct);

    #endregion
}

#endregion

#endregion