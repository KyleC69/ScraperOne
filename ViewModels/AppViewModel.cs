using ReactiveUI;

namespace ScraperOne.ViewModels;

public class AppViewModel : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<bool> i_isBusy;


    public AppViewModel()
    {
        i_isBusy = this
            .WhenAnyValue(x => x.IsCrawling == true)
            .ToProperty(this, x => x.IsBusy);
    }

    public bool IsBusy => i_isBusy.Value;

    public bool IsCrawling { get; set; }
}