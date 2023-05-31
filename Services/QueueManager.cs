// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// QueueManager.csQueueManager.cs0324202310:11 AM


using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Extensions.Logging;
using ScraperOne.DataModels;

namespace ScraperOne.Services;

public class QueueManager : Model
{
    private static ILogger s_logger;
    private readonly ObservableCollection<QueueListItem> i_items;
    private int i_queueDownloadedImageCount;
    private int i_queueTotalImageCount;


    public QueueManager(ILogger createLogger)
    {
        s_logger = createLogger;
        s_logger.LogInformation("Queue Manager has started");
        i_items = new ObservableCollection<QueueListItem>();
        Items = new ReadOnlyObservableCollection<QueueListItem>(i_items);
        i_items.CollectionChanged += ItemsCollectionChanged;
    }


    public ReadOnlyObservableCollection<QueueListItem> Items { get; }


    public int QueueDownloadedImageCount
    {
        get => i_queueDownloadedImageCount;
        private set => SetProperty(ref i_queueDownloadedImageCount, value);
    }


    public int QueueTotalImageCount
    {
        get => i_queueTotalImageCount;
        private set => SetProperty(ref i_queueTotalImageCount, value);
    }

    private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove)
        {
            UpdateTotalImageCount();
            if (e.Action == NotifyCollectionChangedAction.Add) s_logger.LogDebug("Item has been added");
        }

        if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
            s_logger.LogDebug("Item removed from QueueManager");
    }


    public void AddAndReplaceItems(IEnumerable<QueueListItem> itemsToAdd)
    {
        i_items.Clear();
        foreach (var ite in itemsToAdd) AddItem(ite);
    }


    public void AddItems(IEnumerable<QueueListItem> itemsToAdd)
    {
        InsertItems(i_items.Count, itemsToAdd);
    }


    public void ClearItems()
    {
        i_items.Clear();
    }

    public void AddItem(QueueListItem itemToAdd)
    {
        s_logger.LogDebug($"Adding blog {itemToAdd.Blog.Name} to the crawler Que");
        i_items.Add(itemToAdd);
    }

    public void InsertItems(int index, IEnumerable<QueueListItem> itemsToInsert)
    {
        foreach (var item in itemsToInsert)
        {
            s_logger.LogDebug($"Inserting blog {item.Blog.Name} to the Que");
            i_items.Insert(index++, item);
        }
    }


    public void MoveItems(int newIndex, IEnumerable<QueueListItem> itemsToMove)
    {
        var listItems = itemsToMove.ToList();
        var oldIndex = i_items.IndexOf(listItems.First());
        if (oldIndex != newIndex)
        {
            if (newIndex < oldIndex) listItems.Reverse();
            foreach (var item in listItems)
            {
                var currentIndex = i_items.IndexOf(item);
                if (currentIndex != newIndex) i_items.Move(currentIndex, newIndex);
            }
        }
    }


    public void RemoveItem(QueueListItem itemToRemove)
    {
        i_items.Remove(itemToRemove);
    }


    public void RemoveItems(IEnumerable<QueueListItem> itemsToRemove)
    {
        foreach (var item in itemsToRemove.ToArray()) _ = i_items.Remove(item);
    }


    private void UpdateTotalImageCount()
    {
        if (i_items.Any())
        {
            var loadedItems = i_items.Where(x => x.Blog.TotalCount > 0);
            QueueTotalImageCount = loadedItems.Select(x => x.Blog.TotalCount).DefaultIfEmpty()
                .Aggregate((current, next) => current + next);
        }
        else
        {
            QueueTotalImageCount = 0;
        }
    }
}