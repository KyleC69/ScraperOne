using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScraperOne.Views;

public partial class DummyWindow : Window
{
    public DummyWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}