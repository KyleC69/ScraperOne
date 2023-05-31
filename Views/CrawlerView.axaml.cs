using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ScraperOne.ViewModels;


namespace ScraperOne.Views;

public partial class CrawlerView : ReactiveUserControl<CrawlerViewModel>
{
    public CrawlerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}