// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// IBlog.csIBlog.cs032420233:55 PM

namespace ScraperOne.DataModels;

public interface IBlog
{
    int Answers { get; set; }
    int AudioMetas { get; set; }
    int Audios { get; set; }
    BlogTypes BlogType { get; set; }
    bool CheckDirectoryForFiles { get; set; }
    string ChildId { get; set; }
    int CollectionId { get; set; }
    int Conversations { get; set; }
    bool CreateAudioMeta { get; set; }
    bool CreatePhotoMeta { get; set; }
    bool CreateVideoMeta { get; set; }
    DateTime DateAdded { get; set; }
    string Description { get; set; }
    bool Dirty { get; set; }
    bool DownloadAnswer { get; set; }
    bool DownloadAudio { get; set; }
    bool DownloadCatBox { get; set; }
    bool DownloadConversation { get; set; }
    int DownloadedAnswers { get; set; }

    int DownloadedAudioMetas { get; set; }
    int DownloadedAudios { get; set; }
    int DownloadedConversations { get; set; }
    int DownloadedItems { get; }
    int DownloadedItemsNew { get; set; }
    int DownloadedLinks { get; set; }
    int DownloadedPhotoMetas { get; set; }
    int DownloadedPhotos { get; set; }
    int DownloadedQuotes { get; set; }
    int DownloadedTexts { get; set; }
    int DownloadedVideoMetas { get; set; }
    int DownloadedVideos { get; set; }
    string DownloadFrom { get; set; }
    bool DownloadGfycat { get; set; }
    bool DownloadImgur { get; set; }
    bool DownloadLink { get; set; }
    string DownloadPages { get; set; }
    bool DownloadPhoto { get; set; }
    bool DownloadQuote { get; set; }
    bool DownloadRebloggedPosts { get; set; }
    bool DownloadText { get; set; }
    string DownloadTo { get; set; }
    bool DownloadUguu { get; set; }
    bool DownloadUrlList { get; set; }
    bool DownloadVideo { get; set; }
    bool DownloadVideoThumbnail { get; set; }
    bool DownloadWebmshare { get; set; }
    bool DumpCrawlerData { get; set; }
    int DuplicateAudios { get; set; }
    int DuplicatePhotos { get; set; }
    int DuplicateVideos { get; set; }
    int? DwBlogIx { get; set; }
    int? DwUserIx { get; set; }
    string FileDownloadLocation { get; }
    string FilenameTemplate { get; set; }
    bool ForceRescan { get; set; }
    bool ForceSize { get; set; }
    bool GroupPhotoSets { get; set; }
    DateTime LastCompleteCrawl { get; set; }
    string LastDownloadedPhoto { get; set; }
    string LastDownloadedVideo { get; set; }
    ulong LastId { get; set; }
    long LastPreviewShown { get; set; }
    DateTime LatestPost { get; set; }
    List<string> Links { get; }
    Exception LoadError { get; set; }
    string Location { get; set; }

    MetadataType MetadataFormat { get; set; }
    string Name { get; set; }
    string Notes { get; set; }
    int NumberOfLinks { get; set; }
    bool Online { get; set; }
    BlogTypes OriginalBlogType { get; set; }
    int PageSize { get; set; }
    string Password { get; set; }
    int PhotoMetas { get; set; }
    int Photos { get; set; }
    string PnjDownloadFormat { get; set; }
    int Posts { get; set; }
    int Progress { get; set; }
    int Quotes { get; set; }
    int Rating { get; set; }
    bool RegExPhotos { get; set; }
    bool RegExVideos { get; set; }
    int SettingsTabIndex { get; set; }
    bool SkipGif { get; set; }
    Blog.PostType States { get; set; }
    string Tags { get; set; }
    int Texts { get; set; }
    string Title { get; set; }
    int TotalCount { get; set; }
    string Url { get; set; }
    string Version { get; set; }
    int VideoMetas { get; set; }
    int Videos { get; set; }
    Guid Id { get; set; }
    string LastStatus { get; set; }

    void AddFileToDb(string fileName);
    bool CheckIfFileExistsInDirectory(string filename, string filenameNew);
    bool CreateDataFolder();
    string DownloadLocation { get; set; }
    IBlog Load(string fileLocation);
    bool Save();
    void UpdatePostCount(string propertyName);
    void UpdateProgress(bool doCount);
    void AddMediaIndexToDb(long? mediaQwMediaIx);
    bool IsExistingInDb(long qwMediaIx,long blogIx);
}