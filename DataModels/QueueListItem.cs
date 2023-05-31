// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// QueueListItem.csQueueListItem.cs032420233:36 PM

namespace ScraperOne.DataModels;

[Serializable]
public class QueueListItem : Model
{
    private string i_progress;


    public QueueListItem(IBlog blog)
    {
        Blog = blog;
    }


    public IBlog Blog { get; }


    public string Progress
    {
        get => i_progress;
        set
        {
            SetProperty(ref i_progress, value);
            RaisePropertyChanged();
        }
    }

    public event EventHandler InterruptionRequested;


    public void RequestInterruption()
    {
        var handler = InterruptionRequested;
        handler?.Invoke(this, EventArgs.Empty);
    }
}