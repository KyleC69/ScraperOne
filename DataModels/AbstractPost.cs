// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// AbstractPost.csAbstractPost.cs032320233:28 AM


#region Interface Implementations

namespace ScraperOne.DataModels;

#region Interface Implementations

public enum PostFileType
{
    Binary,
    Text
}

public abstract class AbstractPost
{
    #region Setup/Teardown

    protected AbstractPost(string url, string postedUrl, string id, string date, string filename)
    {
        Url = url;
        PostedUrl = postedUrl;
        Id = id;
        Date = date;
        Filename = filename;
    }

    #endregion


    #region Properties

    public string Date { get; }

    public string DbType { get; protected set; }
    public string Filename { get; }

    public string Id { get; }

    public string PostedUrl { get; }

    public PostFileType PostFileType { get; protected set; }


    public string TextFileLocation { get; protected set; }

    public string Url { get; protected set; }

    #endregion
}

#endregion

#endregion

#nullable disable