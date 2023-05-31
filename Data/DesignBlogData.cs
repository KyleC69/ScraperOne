using CommunityToolkit.Mvvm.ComponentModel;
using ScraperOne.DataModels;

namespace ScraperOne.Data;

public class DesignBlogData : ObservableObject
{
    public DesignBlogData(string name, string url, int downloadedVideos, int downloadedphotos, int progress,
        int dupevids)
    {
    }


    public int Answers { get; set; }
    public int AudioMetas { get; set; }
    public int Audios { get; set; }
    public BlogTypes BlogType { get; set; }
    public bool CheckDirectoryForFiles { get; set; }
    public string ChildId { get; set; }
    public int CollectionId { get; set; }
    public int Conversations { get; set; }

    public DateTime DateAdded { get; set; }
    public string Description { get; set; }
    public bool Dirty { get; set; }
    public bool DownloadAnswer { get; set; }
    public bool DownloadAudio { get; set; }
    public bool DownloadCatBox { get; set; }
    public bool DownloadConversation { get; set; }

    public int DownloadedAnswers { get; set; }
    public int DownloadedAudioMetas { get; set; }
    public int DownloadedAudios { get; set; }
    public int DownloadedConversations { get; set; }
    public int DownloadedItems { get; }
    public int DownloadedItemsNew { get; set; }
    public int DownloadedLinks { get; set; }
    public int DownloadedPhotoMetas { get; set; }
    public int DownloadedPhotos { get; set; }
    public int DownloadedQuotes { get; set; }
    public int DownloadedTexts { get; set; }
    public int DownloadedVideoMetas { get; set; }
    public int DownloadedVideos { get; set; }

    public string DownloadFrom { get; set; }
    public string DownloadPages { get; set; }
    public bool DownloadPhoto { get; set; }
    public bool DownloadQuote { get; set; }
    public bool DownloadRebloggedPosts { get; set; }
    public bool DownloadText { get; set; }
    public string DownloadTo { get; set; }
    public bool DownloadUguu { get; set; }
    public bool DownloadUrlList { get; set; }
    public bool DownloadVideo { get; set; }
    public bool DownloadVideoThumbnail { get; set; }
    public bool DownloadWebmshare { get; set; }
    public bool DumpCrawlerData { get; set; }
    public int DuplicateAudios { get; set; }
    public int DuplicatePhotos { get; set; }
    public int DuplicateVideos { get; set; }
    public int? DwBlogIx { get; set; }
    public int DwUserIx { get; set; }
    public string FileDownloadLocation { get; set; }
    public string FilenameTemplate { get; set; }
    public bool ForceRescan { get; set; }
    public bool ForceSize { get; set; }
    public bool GroupPhotoSets { get; set; }
    public DateTime LastCompleteCrawl { get; set; }
    public string LastDownloadedPhoto { get; set; }
    public string LastDownloadedVideo { get; set; }
    public ulong LastId { get; set; }
    public long LastPreviewShown { get; set; }
    public DateTime LatestPost { get; set; }
    public List<string> Links { get; }
    public Exception LoadError { get; set; }
    public string Location { get; set; }
    public MetadataType MetadataFormat { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public int NumberOfLinks { get; set; }
    public bool Online { get; set; }
    public BlogTypes OriginalBlogType { get; set; }
    public int PageSize { get; set; }
    public string Password { get; set; }
    public int PhotoMetas { get; set; }
    public int Photos { get; set; }
    public string PnjDownloadFormat { get; set; }
    public int Posts { get; set; }
    public int Progress { get; set; }
    public int Quotes { get; set; }
    public int Rating { get; set; }
    public bool RegExPhotos { get; set; }
    public bool RegExVideos { get; set; }
    public int SettingsTabIndex { get; set; }
    public bool SkipGif { get; set; }
    public Blog.PostType States { get; set; }
    public string Tags { get; set; }
    public int Texts { get; set; }
    public string Title { get; set; }
    public int TotalCount { get; set; }
    public string Url { get; set; }
    public string Version { get; set; }
    public int VideoMetas { get; set; }
    public int Videos { get; set; }
    public Guid Id { get; set; }
}