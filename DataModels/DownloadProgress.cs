// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// DownloadProgress.csDownloadProgress.cs032320233:28 AM

namespace ScraperOne.DataModels;

public class DownloadProgress
{
    public DownloadProgress(int progress = 0, IBlog blog = null)
    {
        Progress = progress;
        Blog = blog;
    }

    public int Progress { get; set; }
    public string SProgress { get; set; }
    public IBlog Blog { get; set; }
}