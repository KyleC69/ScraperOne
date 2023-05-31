// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// PhotoPost.csPhotoPost.cs032320233:29 AM


#region Interface Implementations

namespace ScraperOne.DataModels;

#region Interface Implementations

public class PhotoPost : TumblrPost
{
    #region Setup/Teardown

    public PhotoPost(string url, string postedUrl, string id, string date, string filename) : base(url, postedUrl, id,
        date, filename)
    {
        PostFileType = PostFileType.Binary;
        DbType = "DownloadedPhotos";
        TextFileLocation = "images_url.txt";
    }


    public PhotoPost(string url, string postedUrl, string id, string filename) : this(url, postedUrl, id, string.Empty,
        filename)
    {
    }

    #endregion
}

#endregion

#endregion