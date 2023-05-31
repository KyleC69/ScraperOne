// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// QueueController.csQueueController.cs032320233:30 AM


using System.Runtime.Serialization.Json;
using System.Text;
using ScraperOne.DataModels;
using ScraperOne.Logger;
using ScraperOne.Properties;

namespace ScraperOne.Services;

public class QueueController : ServiceBase
{

    private static ColorLogger _logger;

    //#######################
    //### 
    //###  Public Methods
    //#######################


    public QueueController(ColorLogger createLogger)
    {
        
        _logger = createLogger;
        InitializationComplete = new TaskCompletionSource<bool>();
    }


    public TaskCompletionSource<bool> InitializationComplete { get; set; }


    public void Initialize()
    {
        LoadQueue();
    }


    public static void InsertBlogFiles(int index, IEnumerable<IBlog> blogFiles)
    {
        QueueManager.InsertItems(index, blogFiles.Select(x => new QueueListItem(x)));
    }


    public void LoadQueue()
    {
        ClearList();
        var blogNamesToLoad = QueueSettings.Names;
        var blogTypesToLoad = QueueSettings.Types;
        InsertFilesCore(0, blogNamesToLoad, blogTypesToLoad);
        InitializationComplete.SetResult(true);
    }


    public void OpenList()
    {
        var jsonqueFilename = "Jsonque.json";
        if (File.Exists(jsonqueFilename)) OpenListCore(jsonqueFilename);
    }


    public void Shutdown()
    {
        QueueSettings.ReplaceAll(QueueManager.Items.Select(x => x.Blog.Name),
            QueueManager.Items.Select(x => x.Blog.BlogType));
    }


    private static void ClearList()
    {
        QueueManager.ClearItems();
    }


    private void InsertFilesCore(int index, IEnumerable<string> names, IEnumerable<BlogTypes> blogTypes)
    {
        try
        {
            InsertBlogFiles(index,
                names.Zip(blogTypes, Tuple.Create).Select(x =>
                    ManagerService.BlogFiles.First(
                        blogs => blogs.Name.Equals(x.Item1) && blogs.BlogType.Equals(x.Item2))));
        }
        catch (Exception)
        {
            _logger.LogError("QueueController.InsertFileCore:");
        }
    }


    private void OpenListCore(string queuelistFileName)
    {
        QueueSettings queueList;
        try
        {
            using FileStream stream = new(queuelistFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            DataContractJsonSerializer serializer = new(typeof(QueueSettings));
            queueList = (QueueSettings)serializer.ReadObject(stream);
        }
        catch (Exception)
        {
            _logger.LogError("QueueController:OpenListCore:");
            return;
        }

        InsertFilesCore(QueueManager.Items.Count, queueList.Names.ToArray(), queueList.Types.ToArray());
    }


    private void SaveList()
    {
        QueueSettings queueList = new();
        queueList.ReplaceAll(QueueManager.Items.Select(item => item.Blog.Name).ToList(),
            QueueManager.Items.Select(item => item.Blog.BlogType).ToList());
        try
        {
            var targetFolder = "3Blogs/Index/jsonque.que";
            using FileStream stream = new(targetFolder, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "  ");
            DataContractJsonSerializer serializer = new(typeof(QueueSettings));
            serializer.WriteObject(writer, queueList);
            writer.Flush();
        }
        catch (Exception)
        {
            _logger.LogError("QueueController:SaveList: ");
            //   _shellService.ShowError(new QueuelistSaveException(ex), Resources.CouldNotSaveQueueList);
        }
    }
}