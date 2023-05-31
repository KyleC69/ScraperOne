// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// Blog.csBlog.cs032320233:45 AM


using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ScraperOne.DataModels
{
#pragma warning disable SX1309 // Field names should begin with underscore
    [DataContract]
    public class Blog : Model, IBlog
    {
        public enum PostType
        {
            Photo,
            Video
        }


        private int i_answers;
        private int i_audioMetas;
        private int i_audios;
        private BlogTypes i_blogType;
        private bool i_checkDirectoryForFiles;
        private int i_collectionId;
        private int i_conversations;
        private bool i_createAudioMeta;
        private bool i_createPhotoMeta;
        private bool i_createVideoMeta;
        private DateTime i_dateAdded;
        private bool i_downloadAnswer;
        private bool i_downloadAudio;
        private bool i_downloadCatBox;
        private bool i_downloadConversation;
        private int i_downloadedAnswers;
        private int i_downloadedAudioMetas;
        private int i_downloadedAudios;
        private int i_downloadedConversations;
        private int i_downloadedItems;
        private int i_downloadedItemsNew;
        private int i_downloadedLinks;
        private int i_downloadedPhotoMetas;
        private int i_downloadedPhotos;
        private int i_downloadedQuotes;
        private int i_downloadedTexts;
        private int i_downloadedVideoMetas;
        private int i_downloadedVideos;

        private string i_downloadFrom;
        private bool i_downloadGfycat;
        private bool i_downloadImgur;
        private bool i_downloadLink;
        private string i_downloadPages;
        private bool i_downloadPhoto;
        private bool i_downloadQuote;
        private bool i_downloadRebloggedPosts;
        private bool i_downloadText;
        private string i_downloadTo;
        private bool i_downloadUguu;
        private bool i_downloadUrlList;
        private bool i_downloadVideo;
        private bool i_downloadVideoThumbnail;
        private bool i_downloadWebmshare;
        private bool i_dumpCrawlerData;
        private int i_duplicateAudios;
        private int i_duplicatePhotos;
        private int i_duplicateVideos;
        private string i_fileDownloadLocation;
        private string i_filenameTemplate;
        private bool i_forceRescan;
        private bool i_forceSize;
        private bool i_groupPhotoSets;
        private DateTime i_lastCompleteCrawl;
        private string i_lastDownloadedPhoto;
        private string i_lastDownloadedVideo;
        private string i_lastStatus;
        private DateTime i_latestPost;


        private List<string> i_links;

        private object i_lockObjectDb = new();
        private object i_lockObjectDirectory = new();
        private object i_lockObjectPostCount = new();

        private object i_lockObjectProgress = new();

        private MetadataType i_metadataFormat;
        private string i_name;
        private string i_notes;
        private int i_numberOfLinks;
        private bool i_online;
        private int i_pageSize = 50;
        private string i_password;
        private int i_photoMetas;
        private int i_photos;
        private string i_pnjDownloadFormat;
        private int i_posts;
        private int i_progress;
        private int i_quotes;
        private int i_rating;
        private bool i_regExPhotos;
        private bool i_regExVideos;
        private int i_settingsTabIndex;
        private bool i_skipGif;
        private PostType i_states;
        private string i_tags;
        private int i_texts;
        private int i_totalCount;
        private int i_videoMetas;
        private int i_videos;

        public Blog()
        {
        }

        [DataMember]
        public string LastStatus
        {
            get => i_lastStatus;
            set => SetProperty(ref i_lastStatus, value);
        }


        [DataMember]
        public int Answers
        {
            get => i_answers;
            set => SetProperty(ref i_answers, value);
        }


        [DataMember]
        public int AudioMetas
        {
            get => i_audioMetas;
            set => SetProperty(ref i_audioMetas, value);
        }


        [DataMember]
        public int Audios
        {
            get => i_audios;
            set => SetProperty(ref i_audios, value);
        }


        [DataMember]
        public BlogTypes BlogType
        {
            get => i_blogType;
            set
            {
                _ = SetProperty(ref i_blogType, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool CheckDirectoryForFiles
        {
            get => i_checkDirectoryForFiles;
            set
            {
                _ = SetProperty(ref i_checkDirectoryForFiles, value);
                Dirty = true;
            }
        }


        [DataMember] public string ChildId { get; set; }


        [DataMember]
        public int CollectionId
        {
            get => i_collectionId;
            set
            {
                _ = SetProperty(ref i_collectionId, value);
                Dirty = true;
            }
        }


        [DataMember]
        public int Conversations
        {
            get => i_conversations;
            set => SetProperty(ref i_conversations, value);
        }


        [DataMember]
        public bool CreateAudioMeta
        {
            get => i_createAudioMeta;
            set
            {
                _ = SetProperty(ref i_createAudioMeta, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool CreatePhotoMeta
        {
            get => i_createPhotoMeta;
            set
            {
                _ = SetProperty(ref i_createPhotoMeta, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool CreateVideoMeta
        {
            get => i_createVideoMeta;
            set
            {
                _ = SetProperty(ref i_createVideoMeta, value);
                Dirty = true;
            }
        }


        [DataMember]
        public DateTime DateAdded
        {
            get => i_dateAdded;
            set => SetProperty(ref i_dateAdded, value);
        }


        [DataMember] public string Description { get; set; }

        // no DataMember
        public bool Dirty { get; set; }


        [DataMember]
        public bool DownloadAnswer
        {
            get => i_downloadAnswer;
            set
            {
                _ = SetProperty(ref i_downloadAnswer, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadAudio
        {
            get => i_downloadAudio;
            set
            {
                _ = SetProperty(ref i_downloadAudio, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadCatBox
        {
            get => i_downloadCatBox;
            set => SetProperty(ref i_downloadCatBox, value);
        }


        [DataMember]
        public bool DownloadConversation
        {
            get => i_downloadConversation;
            set
            {
                _ = SetProperty(ref i_downloadConversation, value);
                Dirty = true;
            }
        }


        [DataMember]
        public int DownloadedAnswers
        {
            get => i_downloadedAnswers;
            set { SetProperty(ref i_downloadedAnswers, value); }
        }


        [DataMember]
        public int DownloadedAudioMetas
        {
            get => i_downloadedAudioMetas;
            set { SetProperty(ref i_downloadedAudioMetas, value); }
        }


        [DataMember]
        public int DownloadedAudios
        {
            get => i_downloadedAudios;
            set { SetProperty(ref i_downloadedAudios, value); }
        }


        [DataMember]
        public int DownloadedConversations
        {
            get => i_downloadedConversations;
            set { SetProperty(ref i_downloadedConversations, value); }
        }


        /// <summary>
        ///     Number of already downloaded (media) items.
        /// </summary>
        [DataMember]
        public int DownloadedItems
        {
            get => i_downloadedItems;

            set => SetProperty(ref i_downloadedItems, value);
        }


        public int DownloadedItemsNew
        {
            get => i_downloadedItemsNew;
            set => SetProperty(ref i_downloadedItemsNew, value);
        }


        [DataMember]
        public int DownloadedLinks
        {
            get => i_downloadedLinks;
            set { SetProperty(ref i_downloadedLinks, value); }
        }


        [DataMember]
        public int DownloadedPhotoMetas
        {
            get => i_downloadedPhotoMetas;
            set { SetProperty(ref i_downloadedPhotoMetas, value); }
        }


        [DataMember]
        public int DownloadedPhotos
        {
            get => i_downloadedPhotos;
            set { SetProperty(ref i_downloadedPhotos, value); }
        }


        [DataMember]
        public int DownloadedQuotes
        {
            get => i_downloadedQuotes;
            set { SetProperty(ref i_downloadedQuotes, value); }
        }


        [DataMember]
        public int DownloadedTexts
        {
            get => i_downloadedTexts;
            set { SetProperty(ref i_downloadedTexts, value); }
        }


        [DataMember]
        public int DownloadedVideoMetas
        {
            get => i_downloadedVideoMetas;
            set { SetProperty(ref i_downloadedVideoMetas, value); }
        }


        [DataMember]
        public int DownloadedVideos
        {
            get => i_downloadedVideos;
            set { SetProperty(ref i_downloadedVideos, value); }
        }


        [DataMember]
        public string DownloadFrom
        {
            get => i_downloadFrom;
            set
            {
                _ = SetProperty(ref i_downloadFrom, value);
                Dirty = true;
            }
        }


        /*
                [DataMember]
                public MetadataType MetadataFormat
                {
                    get => metadataFormat;
                    set => SetProperty(ref metadataFormat, value);
                }
        */
        [DataMember]
        public bool DownloadGfycat
        {
            get => i_downloadGfycat;
            set => SetProperty(ref i_downloadGfycat, value);
        }


        [DataMember]
        public bool DownloadImgur
        {
            get => i_downloadImgur;
            set => SetProperty(ref i_downloadImgur, value);
        }


        [DataMember]
        public bool DownloadLink
        {
            get => i_downloadLink;
            set
            {
                _ = SetProperty(ref i_downloadLink, value);
                Dirty = true;
            }
        }


        [DataMember]
        public string DownloadPages
        {
            get => i_downloadPages;
            set
            {
                _ = SetProperty(ref i_downloadPages, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadPhoto
        {
            get => i_downloadPhoto;
            set
            {
                _ = SetProperty(ref i_downloadPhoto, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadQuote
        {
            get => i_downloadQuote;
            set
            {
                _ = SetProperty(ref i_downloadQuote, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadRebloggedPosts
        {
            get => i_downloadRebloggedPosts;
            set
            {
                _ = SetProperty(ref i_downloadRebloggedPosts, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadText
        {
            get => i_downloadText;
            set
            {
                _ = SetProperty(ref i_downloadText, value);
                Dirty = true;
            }
        }


        [DataMember]
        public string DownloadTo
        {
            get => i_downloadTo;
            set
            {
                _ = SetProperty(ref i_downloadTo, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadUguu
        {
            get => i_downloadUguu;
            set => SetProperty(ref i_downloadUguu, value);
        }


        [DataMember]
        public bool DownloadUrlList
        {
            get => i_downloadUrlList;
            set
            {
                _ = SetProperty(ref i_downloadUrlList, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadVideo
        {
            get => i_downloadVideo;
            set
            {
                _ = SetProperty(ref i_downloadVideo, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadVideoThumbnail
        {
            get => i_downloadVideoThumbnail;
            set
            {
                _ = SetProperty(ref i_downloadVideoThumbnail, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool DownloadWebmshare
        {
            get => i_downloadWebmshare;
            set => SetProperty(ref i_downloadWebmshare, value);
        }


        [DataMember]
        public bool DumpCrawlerData
        {
            get => i_dumpCrawlerData;
            set
            {
                _ = SetProperty(ref i_dumpCrawlerData, value);
                Dirty = true;
            }
        }


        [DataMember]
        public int DuplicateAudios
        {
            get => i_duplicateAudios;
            set => SetProperty(ref i_duplicateAudios, value);
        }


        [DataMember]
        public int DuplicatePhotos
        {
            get => i_duplicatePhotos;
            set => SetProperty(ref i_duplicatePhotos, value);
        }


        [DataMember]
        public int DuplicateVideos
        {
            get => i_duplicateVideos;
            set => SetProperty(ref i_duplicateVideos, value);
        }

        [DataMember] public int? DwBlogIx { get; set; }

        [DataMember] public int? DwUserIx { get; set; }


        public string FileDownloadLocation
        {
            get
            {
                if (String.IsNullOrEmpty(DownloadLocation))
                {
                    DownloadLocation = Directory.GetCurrentDirectory();
                }

                return Path.Combine(DownloadLocation, Name);
            }
        }


        [DataMember]
        public string FilenameTemplate
        {
            get => i_filenameTemplate;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "%f";
                }

                _ = SetProperty(ref i_filenameTemplate, value);
            }
        }


        [DataMember]
        public bool ForceRescan
        {
            get => i_forceRescan;
            set
            {
                _ = SetProperty(ref i_forceRescan, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool ForceSize
        {
            get => i_forceSize;
            set
            {
                _ = SetProperty(ref i_forceSize, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool GroupPhotoSets
        {
            get => i_groupPhotoSets;
            set
            {
                _ = SetProperty(ref i_groupPhotoSets, value);
                Dirty = true;
            }
        }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime LastCompleteCrawl
        {
            get => i_lastCompleteCrawl;
            set => SetProperty(ref i_lastCompleteCrawl, value);
        }


        public string LastDownloadedPhoto
        {
            get => i_lastDownloadedPhoto;
            set
            {
                _ = SetProperty(ref i_lastDownloadedPhoto, value);
                States = PostType.Photo;
            }
        }


        public string LastDownloadedVideo
        {
            get => i_lastDownloadedVideo;
            set
            {
                _ = SetProperty(ref i_lastDownloadedVideo, value);
                States = PostType.Video;
            }
        }


        [DataMember] public ulong LastId { get; set; }

        [IgnoreDataMember] public long LastPreviewShown { get; set; }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime LatestPost
        {
            get => i_latestPost;
            set => SetProperty(ref i_latestPost, value);
        }

        [DataMember]
        public List<string> Links
        {
            get => i_links;

            set => SetProperty(ref i_links, value);
        }

        // no DataMember
        public Exception LoadError { get; set; }

        [DataMember] public string Location { get; set; }


        [DataMember]
        public MetadataType MetadataFormat
        {
            get => i_metadataFormat;
            set => SetProperty(ref i_metadataFormat, value);
        }


        [DataMember]
        public string Name
        {
            get => i_name;
            set
            {
                _ = SetProperty(ref i_name, value);
                Dirty = true;
            }
        }


        [DataMember]
        public string Notes
        {
            get => i_notes;
            set
            {
                _ = SetProperty(ref i_notes, value);
                Dirty = true;
            }
        }


        [DataMember]
        public int NumberOfLinks
        {
            get => i_numberOfLinks;
            set => SetProperty(ref i_numberOfLinks, value);
        }


        [DataMember]
        public bool Online
        {
            get => i_online;
            set => SetProperty(ref i_online, value);
        }


        [DataMember] public BlogTypes OriginalBlogType { get; set; }


        [DataMember]
        public int PageSize
        {
            get => i_pageSize;
            set
            {
                _ = SetProperty(ref i_pageSize, value);
                Dirty = true;
            }
        }


        [DataMember]
        public string Password
        {
            get => i_password;
            set
            {
                _ = SetProperty(ref i_password, value);
                Dirty = true;
            }
        }


        [DataMember]
        public int PhotoMetas
        {
            get => i_photoMetas;
            set => SetProperty(ref i_photoMetas, value);
        }


        [DataMember]
        public int Photos
        {
            get => i_photos;
            set => SetProperty(ref i_photos, value);
        }


        [DataMember]
        public string PnjDownloadFormat
        {
            get => i_pnjDownloadFormat;
            set
            {
                _ = SetProperty(ref i_pnjDownloadFormat, value);
                Dirty = true;
            }
        }


        [DataMember]
        public int Posts
        {
            get => i_posts;
            set => SetProperty(ref i_posts, value);
        }


        [DataMember]
        public int Progress
        {
            get => i_progress;
            set { _ = SetProperty(ref i_progress, value); }
        }


        [DataMember]
        public int Quotes
        {
            get => i_quotes;
            set => SetProperty(ref i_quotes, value);
        }


        [DataMember]
        public int Rating
        {
            get => i_rating;
            set
            {
                _ = SetProperty(ref i_rating, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool RegExPhotos
        {
            get => i_regExPhotos;
            set
            {
                _ = SetProperty(ref i_regExPhotos, value);
                Dirty = true;
            }
        }


        [DataMember]
        public bool RegExVideos
        {
            get => i_regExVideos;
            set
            {
                _ = SetProperty(ref i_regExVideos, value);
                Dirty = true;
            }
        }


        [DataMember]
        public int SettingsTabIndex
        {
            get => i_settingsTabIndex;
            set => SetProperty(ref i_settingsTabIndex, value);
        }


        [DataMember]
        public bool SkipGif
        {
            get => i_skipGif;
            set
            {
                _ = SetProperty(ref i_skipGif, value);
                Dirty = true;
            }
        }


        [DataMember]
        public PostType States
        {
            get => i_states;
            set => SetProperty(ref i_states, value);
        }


        [DataMember]
        public string Tags
        {
            get => i_tags;
            set
            {
                _ = SetProperty(ref i_tags, value);
                Dirty = true;
            }
        }


        [DataMember]
        public int Texts
        {
            get => i_texts;
            set => SetProperty(ref i_texts, value);
        }


        [DataMember] public string Title { get; set; }


        /// <summary>
        ///     Number of items to download.
        /// </summary>
        [DataMember]
        public int TotalCount
        {
            get => i_totalCount;
            set => SetProperty(ref i_totalCount, value);
        }


        [DataMember] public string Url { get; set; }

        [DataMember] public string Version { get; set; }


        [DataMember]
        public int VideoMetas
        {
            get => i_videoMetas;
            set => SetProperty(ref i_videoMetas, value);
        }


        [DataMember]
        public int Videos
        {
            get => i_videos;
            set => SetProperty(ref i_videos, value);
        }

        public Guid Id { get; set; }


        /// <summary>
        ///     Increments session total and progress bar
        /// </summary>
        /// <param name="doCount"></param>
        public void UpdateProgress(bool doCount)
        {
            lock (i_lockObjectProgress)
            {
                if (doCount)
                {
                    DownloadedItemsNew++;
                }

                Progress = (int)(DownloadedItems / (double)TotalCount * 100);
                _ = Save();
            }
        }

        public void AddMediaIndexToDb(long? mediaQwMediaIx)
        {
        }

        public bool IsExistingInDb(long qwMediaIx, long blogIx)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Increments count for file type
        /// </summary>
        /// <param name="propertyName"></param>
        public void UpdatePostCount(string propertyName)
        {
            lock (i_lockObjectPostCount)
            {
                System.Reflection.PropertyInfo property = typeof(IBlog).GetProperty(propertyName);
                int postCounter = (int)property.GetValue(this);
                postCounter++;
                property.SetValue(this, postCounter, null);
            }
        }


        public void AddFileToDb(string fileName)
        {
            Links ??= new();
            lock (i_lockObjectDb)
            {
                Links.Add(fileName);
            }
        }


        public bool CreateDataFolder()
        {
            if (!Directory.Exists(DownloadLocation))
            {
                _ = Directory.CreateDirectory(DownloadLocation);
                return true;
            }

            return false;
        }


        public virtual bool CheckIfFileExistsInDirectory(string filename, string filenameNew)
        {
            Monitor.Enter(i_lockObjectDirectory);
            string blogPath = DownloadLocation;
            try
            {
                string filepath = Path.Combine(blogPath, filename);
                string filepathNew = Path.Combine(blogPath, filenameNew);
                bool result = File.Exists(filepath);
                if (result && !string.IsNullOrEmpty(filenameNew))
                {
                    if (File.Exists(filepathNew))
                    {
                        //Logger.Warning("{0}: Cannot rename file to '{1}', a file with that name already exists!", Name, filenameNew);
                    }
                    else
                    {
                        File.Move(filepath, filepathNew);
                    }
                }

                return result || string.IsNullOrEmpty(filenameNew) ? result : File.Exists(filepathNew);
            }
            finally
            {
                Monitor.Exit(i_lockObjectDirectory);
            }
        }


        public string DownloadLocation
        {
            get => App.Settings.DownloadLocation;

            set => SetProperty(ref i_fileDownloadLocation, value);
            /*
                        string.IsNullOrWhiteSpace(FileDownloadLocation)
                            ? Path.Combine(Directory.GetParent(Location).FullName, Name)
                            : FileDownloadLocation;*/
        }


        /// <summary>
        ///     Loads Blog from saved config files
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <returns>IBlog</returns>
        public IBlog Load(string fileLocation)
        {
            try
            {
                return LoadCore(fileLocation);
            }
            catch (Exception ex) when (ex is SerializationException or FileNotFoundException)
            {
                ex.Data.Add("Filename", fileLocation);
                throw;
            }
        }


        public bool Save()
        {
            try
            {
                Dirty = false;
                SaveBlog();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // Logger.Error("Blog:Save: {0}", ex);
                throw;
            }
        }

        private void OnDirtyBlogHandler(object sender, PropertyChangedEventArgs e)
        {
            //   SaveBlog();
        }


        public static string ExtractName(string url)
        {
            return ExtractSubDomain(url);
        }

//TODO:Error handling
        private IBlog LoadCore(string fileLocation)
        {
            if (!File.Exists(fileLocation))
            {
                return this;
            }

            using FileStream stream = new(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read);
            DataContractJsonSerializer serializer = new(GetType());
            Blog blog = (Blog)serializer.ReadObject(stream);
            if (blog.Version == "3")
            {
                _ = Enum.TryParse(Path.GetExtension(fileLocation).Replace(".", ""), out BlogTypes blogType);
                blog.OriginalBlogType = blogType;
                blog.Version = "4";
            }

            if (string.IsNullOrEmpty(blog.FilenameTemplate))
            {
                blog.FilenameTemplate = "%f";
            }

            if (string.IsNullOrEmpty(blog.Location))
            {
                blog.Location = Path.Combine(Directory.GetParent(fileLocation).FullName, "Index");
            }

            if (string.IsNullOrEmpty(blog.ChildId))
            {
                blog.ChildId = Path.Combine(blog.Location, blog.Name + "_files." + blog.OriginalBlogType);
            }

            if (blog.Links != null && blog.BlogType != BlogTypes.Twitter && blog.BlogType != BlogTypes.Newtumbl &&
                blog.BlogType != BlogTypes.Instagram)
            {
                // use leftover property Links to indicate if one-time update of DownloadVideoThumbnail was done
                blog.DownloadVideoThumbnail = true;
                blog.i_links = null;
            }

            return blog;
        }


        private void SaveBlog()
        {
            string currentIndex = Path.Combine(Location, Name + "." + OriginalBlogType);
            string newIndex = Path.Combine(Location, Name + "." + OriginalBlogType + ".new");
            string backupIndex = Path.Combine(Location, Name + "." + OriginalBlogType + ".bak");


            if (File.Exists(currentIndex))
            {
                try
                {
                    if (File.Exists(backupIndex))
                    {
                        File.Delete(backupIndex);
                    }

                    if (SaveCore(newIndex))
                    {
                        if (File.Exists(newIndex))
                        {
                            File.Replace(newIndex, currentIndex, backupIndex, true);
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    File.Delete(backupIndex);
                }
            }
            else
            {
                _ = SaveCore(currentIndex);
            }
        }


        private bool SaveCore(string path)
        {
            try
            {
                using FileStream stream = new(path, FileMode.Create, FileAccess.ReadWrite);
                using System.Xml.XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(
                    stream, Encoding.UTF8, true, true, "  ");
                DataContractJsonSerializer serializer = new(GetType());
                serializer.WriteObject(writer, this);
                writer.Flush();
                return File.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
        }


        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            i_lockObjectProgress = new object();
            i_lockObjectPostCount = new object();
            i_lockObjectDb = new object();
            i_lockObjectDirectory = new object();
        }


        protected static string ExtractSubDomain(string url)
        {
            string[] source = url.Split('.');
            return source.Length >= 3 && source[0].StartsWith("http://", true, null)
                ? source[0].Replace("http://", string.Empty)
                : source.Length >= 3 && source[0].StartsWith("https://", true, null)
                    ? source[0].Replace("https://", string.Empty)
                    : null;
        }
    }
#pragma warning restore SX1309 // Field names should begin with underscore

    public enum BlogTypes
    {
        Newtumbl,
        Other,
        Twitter,
        Instagram,
        Tumblr,
        Tmblrpriv
    }
}