// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// IFiles.csIFiles.cs032320233:28 AM


using System.ComponentModel;

namespace ScraperOne.DataModels.Files;

public interface IFiles : INotifyPropertyChanged
{
    BlogTypes BlogType { get; }

    //IList<string> Links { get; }

    IEnumerable<FileEntry> Entries { get; }

    bool IsDirty { get; }
    string Name { get; }

    string Version { get; set; }

    void AddFileToDb(string fileNameUrl, string fileNameOriginalUrl, string fileName, int postId);
    void AddMediaToDb(string mediaIx);
    string AddFileToDb(string fileNameUrl, string fileNameOriginalUrl, string fileName, string appendTemplate);
    bool CheckIfMediaExistsInDb(string szMediaIx);
    bool CheckIfFileExistsInDb(string filenameUrl, bool checkOriginalLinkFirst);

    bool Save();

    void UpdateOriginalLink(string filenameUrl, string filenameOriginalUrl);
/// <summary>
/// Checks for id as string in downloaded files list.
/// changing from postid to MediaID to eliminate cross blog download dupes
/// </summary>
/// <param name="postId"></param>
/// <returns>boolean</returns>
    bool CheckForPostId(string postId);
    void UpdatePostId(TumblrPost downloadItem);
}