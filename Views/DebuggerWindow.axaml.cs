using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScraperOne.ViewModels;

namespace ScraperOne.Views;

public partial class DebuggerWindow : Window
{
    public DebuggerWindow()
    {
        InitializeComponent();
        DataContext = new DebuggerViewModel();
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}