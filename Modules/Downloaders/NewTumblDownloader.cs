// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// NewTumblDownloader.csNewTumblDownloader.cs032320233:30 AM

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using ScraperOne.DataModels;
using ScraperOne.DataModels.Files;

namespace ScraperOne.Modules.Downloaders;

public class NewTumblDownloader : AbstractDownloader
{
    public NewTumblDownloader(
        IPostQueue<AbstractPost> postQueue,
        IBlog blog,
        IFiles files,
        IProgress<DownloadProgress> progress,
        ILogger logger,
        CancellationToken ct1) : base(postQueue, blog, files, progress, ct1, logger)
    {
        _Ct = ct1;
        Guard.IsNotNull(logger);
        Guard.IsNotNull(blog);
        Guard.IsNotNull(postQueue);
        Guard.IsNotNull(files);
        Guard.IsNotNull(progress);
    }


    public static async Task<bool> RawUrlDownloadAsync(string url, string fileLocation)
    {
        try
        {
            var client = new HttpClient();
            using var fstream = new FileStream(fileLocation, FileMode.CreateNew, FileAccess.ReadWrite);
            using var stream = await client.GetStreamAsync(url);
            await stream.CopyToAsync(fstream);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool CheckIfMediaExists(string mediaIx)
    {
        // return _Files.CheckForPostId(mediaIx);
        return false;
    }

    public void AddMediaToDb(string mediaIx)
    {
        _Files.AddMediaToDb(mediaIx);
    }
}