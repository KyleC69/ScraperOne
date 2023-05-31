// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ICrawlerService.csICrawlerService.cs0324202312:04 PM


using System.Collections.ObjectModel;
using System.Windows.Input;
using ScraperOne.DataModels;

namespace ScraperOne.Services;

public interface ICrawlerService
{
    int ActiveCollectionId { get; set; }

    ReadOnlyObservableCollection<QueueListItem> ActiveItems { get; }
    ICommand AddBlogCommand { get; set; }
    static ICommand CrawlCommand { get; set; }
    TaskCompletionSource<bool> DatabasesLoaded { get; set; }

    ICommand ImportBlogsCommand { get; set; }
    TaskCompletionSource<bool> InitializationComplete { get; set; }
    bool IsCrawl { get; set; }
    bool IsPaused { get; set; }
    bool IsTimerSet { get; set; }
    TaskCompletionSource<bool> LibraryLoaded { get; set; }
    string NewBlogUrl { get; set; }

    void AddActiveItems(QueueListItem itemToAdd);

    void ClearItems();

    void RemoveActiveItem(QueueListItem itemToRemove);

    void UpdateCollectionsList(bool v);
}