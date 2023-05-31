// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// VideoPost.csVideoPost.cs032320233:29 AM


#region Interface Implementations

namespace ScraperOne.DataModels;

#region Interface Implementations

public class VideoPost : TumblrPost
{
    #region Setup/Teardown

    public VideoPost(string url, string id, string date, string filename) : base(url, null, id, date, filename)
    {
        PostFileType = PostFileType.Binary;
        DbType = "DownloadedVideos";
        TextFileLocation = "videos_url.txt";
    }


    public VideoPost(string url, string id, string filename) : this(url, id, string.Empty, filename)
    {
    }

    #endregion
}

#endregion

#endregion

#nullable disable