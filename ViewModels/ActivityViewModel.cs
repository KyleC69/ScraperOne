using System.Collections.ObjectModel;
using ScraperOne.DataModels;

namespace ScraperOne.ViewModels;

public class ActivityViewModel : ViewModelBase
{
    public ActivityViewModel()
    {
        ActivityItems = new ObservableCollection<IBlog>();
    }

    public static ObservableCollection<IBlog> ActivityItems { get; set; }
}