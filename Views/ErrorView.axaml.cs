using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ScraperOne.ViewModels;

namespace ScraperOne.Views;

public partial class ErrorView : ReactiveUserControl<ErrorViewModel>
{
    public ErrorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}