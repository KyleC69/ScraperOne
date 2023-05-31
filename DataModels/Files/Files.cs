// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// Files.csFiles.cs032320233:28 AM


using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace ScraperOne.DataModels.Files;

[DataContract]
public class Files : Model, IFiles
{
    private const int MaxSupportedDbVersion = 6;
    [DataMember(Name = "Entries")] protected HashSet<FileEntry> _Entries;


    protected bool _IsDirty;

    [DataMember(Name = "Links", IsRequired = false, EmitDefaultValue = false)]
    protected List<string> _Links;

    private ReaderWriterLockSlim _lock = new();

    private ReaderWriterLockSlim i_lock = new();


    public Files()
    {
        Init();
    }


    /// <summary>
    ///     CTOR
    /// </summary>
    /// <param name="name"></param>
    /// <param name="location"></param>
    public Files(string name, string location)
    {
        Name = name;
        Location = location;
        Version = MaxSupportedDbVersion.ToString();
        //links = new List<string>();
        _Entries = new HashSet<FileEntry>(new FileEntryComparer());
        //_originalLinks = new HashSet<string>();
    }


    [DataMember] public string Location { get; set; }

    [DataMember] public string Updates { get; set; }


    [DataMember] public BlogTypes BlogType { get; set; }

    //public IList<string> Links => links;

    public IEnumerable<FileEntry> Entries => _Entries;


    public bool IsDirty
    {
        get => _IsDirty;
        set
        {
            if (_IsDirty == value) return;
            SetProperty(ref _IsDirty, value);
        }
    }


    [DataMember] public string Name { get; set; }

    [DataMember] public string Version { get; set; }


    public void AddFileToDb(string fileNameUrl, string fileNameOriginalUrl, string fileName, int postId)
    {
        _lock.EnterWriteLock();
        try
        {
            _Entries.Add(new FileEntry
            {
                Link = fileNameUrl,
                OriginalLink = fileNameOriginalUrl,
                Filename = fileName,
                PostId = postId.ToString()
            });
            //    _originalLinks.Add(fileNameOriginalUrl);
            IsDirty = true;
        }
        finally
        {
            i_lock.ExitWriteLock();
        }
    }


    public void AddMediaToDb(string mediaIx)
    {
        try
        {
            _Entries.Add(new FileEntry
            {
                Link = string.Empty,
                OriginalLink = string.Empty,
                Filename = string.Empty,
                PostId = mediaIx.ToString()
            });
            //    _originalLinks.Add(fileNameOriginalUrl);
            IsDirty = true;
        }
        finally
        {
        }
    }


    public string AddFileToDb(string fileNameUrl, string fileNameOriginalUrl, string fileName, string appendTemplate)
    {
        var n = _Entries.Count(x => IsMatch(x, fileName, appendTemplate));
        if (n > 0)
            fileName = Path.GetFileNameWithoutExtension(fileName) + appendTemplate.Replace("<0>", (n + 1).ToString()) +
                       Path.GetExtension(fileName);
        _Entries.Add(new FileEntry { Link = fileNameUrl, OriginalLink = fileNameOriginalUrl, Filename = fileName });

        IsDirty = true;
        Save();
        return fileName;
    }

    public bool CheckIfMediaExistsInDb(string szMediaIx)
    {
        return CheckForPostId(szMediaIx);
    }


    public void UpdateOriginalLink(string filenameUrl, string filenameOriginalUrl)
    {
        var entry = _Entries.First(x => x.Link == filenameUrl);
        //   _originalLinks.Remove(entry.OriginalLink);
        entry.OriginalLink = filenameOriginalUrl;
        //   _originalLinks.Add(filenameOriginalUrl);
    }


    public bool CheckForPostId(string postId)
    {
        i_lock = new ReaderWriterLockSlim();
        i_lock.EnterReadLock();
        try
        {
            //################################
            return _Entries.Contains(new FileEntry { PostId = postId });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        finally
        {
            i_lock.ExitReadLock();
        }
    }


    public void UpdatePostId(TumblrPost downloadItem)
    {
        i_lock.EnterWriteLock();
        try
        {
            if (_Entries.Any())
            {
                var entry = _Entries.First(x => x.Link == downloadItem.Url);
                //var entry = _entries.First(x => x.Link == filenameUrl);

                //  _originalLinks.Remove(entry.OriginalLink);
                entry.OriginalLink = downloadItem.Url;
                entry.PostId = downloadItem.Id;
            }
            //_originalLinks.Add(downloadItem.Url);
        }
        finally
        {
            Save();
            i_lock.ExitWriteLock();
        }
    }


    public virtual bool CheckIfFileExistsInDb(string filenameUrl, bool checkOriginalLinkFirst)
    {
        //_lock ??= new ReaderWriterLockSlim();
        i_lock.EnterReadLock();
        try
        {
            return _Entries.Contains(new FileEntry { Link = filenameUrl });
        }
        finally
        {
            i_lock.ExitReadLock();
        }
    }


    public bool Save()
    {
        try
        {
            var currentIndex = Path.Combine(Location, Name + "_files." + BlogType);
            var newIndex = Path.Combine(Location, Name + "_files." + BlogType + ".new");
            var backupIndex = Path.Combine(Location, Name + "_files." + BlogType + ".bak");
            if (File.Exists(currentIndex))
            {
                Save(newIndex);
                File.Replace(newIndex, currentIndex, backupIndex, true);
                File.Delete(backupIndex);
            }
            else
            {
                Save(currentIndex);
            }

            IsDirty = false;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Debug.WriteLine("Files:Save:Error {0}", ex);
            return false;
        }
    }


    public static IFiles Load(string fileLocation, bool isArchive = false)
    {
        try
        {
            return LoadCore(fileLocation, isArchive);
        }
        catch (Exception ex) when (ex is SerializationException || ex is FileNotFoundException || ex is IOException)
        {
            ex.Data.Add("Filename", fileLocation);
            throw;
        }
    }


    private static bool IsMatch(FileEntry x, string filename, string appendTemplate)
    {
        if (x.Filename == filename) return true;
        if (string.Compare(Path.GetExtension(x.Filename), Path.GetExtension(filename),
                StringComparison.InvariantCultureIgnoreCase) != 0) return false;
        var pattern = Regex.Escape(Path.GetFileNameWithoutExtension(filename) + appendTemplate)
            .Replace("<0>", @"[\d]+");
        return Regex.IsMatch(Path.GetFileNameWithoutExtension(x.Filename) ?? string.Empty, pattern);
    }


    private static IFiles LoadCore(string fileLocation, bool isArchive)
    {
        if (!File.Exists(fileLocation))
        {
            return new Files();
        }

        using var stream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read);

        var serializer = new DataContractJsonSerializer(typeof(Files));
        if (!stream.CanRead) return new Files();
        Files file = null;
        try
        {
            file = (Files)serializer.ReadObject(stream);
        }
        catch (System.Exception)
        {
            File.Delete(fileLocation);
            return null;
        }

        file!._Entries = file._Entries is null
            ? new HashSet<FileEntry>(new FileEntryComparer())
            : new HashSet<FileEntry>(file._Entries, new FileEntryComparer());
        if (!isArchive) DoUpdates(file);
        if (file.Version == "1")
        {
            for (var i = 0; i < file._Links.Count; i++)
                if (!file._Links[i].StartsWith("tumblr") && file._Links[i].Count(c => c == '_') == 2)
                    file._Links[i] = file._Links[i][(file._Links[i].LastIndexOf('_') + 1)..];
            file.Version = "2";
            file.IsDirty = true;
        }

        if (file.Version == "2")
        {
            file._Entries = new HashSet<FileEntry>(new FileEntryComparer());
            for (var i = 0; i < file._Links.Count; i++)
            {
                var fn = file._Links[i];
                file._Entries.Add(new FileEntry { Link = fn, Filename = fn });
            }

            file.Version = "3";
            file.IsDirty = true;
            if (!Path.GetDirectoryName(fileLocation)!.Contains("\\Index\\Archive\\"))
            {
                var backupPath = Path.Combine(Path.GetDirectoryName(fileLocation)!, "backup");
                var backupFilename = Path.Combine(backupPath, Path.GetFileName(fileLocation));
                Directory.CreateDirectory(backupPath);
                if (!File.Exists(backupFilename)) File.Copy(fileLocation, backupFilename);
            }
        }

        if (file.Version == "3")
        {
            // the cleanup 1 -> 2 destroyed valid links too, so clean up these orphaned links
            //var invalidChars = Path.GetInvalidFileNameChars();
            var newList = new HashSet<FileEntry>(new FileEntryComparer());
            if (file._Entries != null)
                foreach (var entry in file._Entries)
                {
                    var re = new Regex(@"^(1280|540|500|400|250|100|75sq|720|640)(\.[^.]*$|$)");
                    if (!re.IsMatch(entry.Link))
                        newList.Add(entry);
                }

            file.Version = "4";
            file.IsDirty = true;
            file._Entries = newList;
        }

        if (file.Version == "4")
        {
            // cleanup wrong twitter links (incompletely fixing issue 231)
        }

        if (file.Version == "5")
        {
            // going back to older app version would drop OriginalLinks
            file.Version = "6";
            file.IsDirty = true;
        }

        if (int.Parse(file.Version) > MaxSupportedDbVersion)
            //_logger.Error("{0}: DB version {1} not supported!", file.Name, file.Version);
            throw new SerializationException($"{file.Name}: DB version {file.Version} not supported!");
        foreach (var entry in file._Entries)
            // remove erroneously added original links
            if (file.BlogType == BlogTypes.Newtumbl && entry.OriginalLink != null)
                entry.OriginalLink = null;
        //   file._originalLinks.Add(entry.OriginalLink);
        file.Location = Path.Combine(Directory.GetParent(fileLocation)?.FullName!);
        return file;
    }


    private static void DoUpdates(Files file)
    {
        // T01
        if (!(file.Updates ?? "").Contains("T01") &&
            (file.BlogType == BlogTypes.Tumblr || file.BlogType == BlogTypes.Tmblrpriv) &&
            new[] { "1", "2", "3", "4", "5" }.Contains(file.Version) && Directory.Exists(file.Location))
        {
            if (new[] { "4", "5" }.Contains(file.Version))
                foreach (var entry in file._Entries.ToArray())
                {
                    if (!entry.Filename.ToLower().EndsWith(".mp4")) continue;
                    var filepath = Path.Combine(file.Location.Replace("\\Index", ""), file.Name, entry.Filename);
                    if (!File.Exists(filepath)) continue;
                    var fi = new FileInfo(filepath);
                    //var fileLength = fi.Length;
                    if (fi.Length <= 50 * 1024 && fi.CreationTime > new DateTime(2022, 4, 1))
                    {
                        var redo = false;
                        if (fi.Length < 8)
                            redo = true;
                        else
                            using (var fs = File.OpenRead(filepath))
                            {
                                var ba = new byte[8];
                                // ReSharper disable once MustUseReturnValue
                                fs.Read(ba, 0, 8);
                                if (ba[4] == 0x66 && ba[5] == 0x74 && ba[6] == 0x79 && ba[7] == 0x70) continue;
                                if (ba[0] == 0x7B && ba[1] == 0x0D && ba[2] == 0x0A) redo = true;
                            }

                        if (redo)
                        {
                            File.Delete(filepath);
                            file._Entries.Remove(entry);
                        }
                    }
                }

            file.Updates = (string.IsNullOrEmpty(file.Updates) ? "" : "|") + "T01";
            file.IsDirty = true;
        }
    }


    private void Save(string path)
    {
        //  Console.WriteLine($"Save::PATH::=={path}");
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "  ");
        var serializer = new DataContractJsonSerializer(GetType());
        _Links = null;
        serializer.WriteObject(writer, this);
        writer.Flush();
    }


    private void Init()
    {
        Version = MaxSupportedDbVersion.ToString();
        _Links = new List<string>();
        _Entries = new HashSet<FileEntry>(new FileEntryComparer());
        // _originalLinks = new HashSet<string>();
        _lock = new ReaderWriterLockSlim();
    }
}