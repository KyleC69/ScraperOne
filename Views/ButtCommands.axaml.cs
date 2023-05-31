using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ScraperOne.ViewModels;


namespace ScraperOne.Views;

public partial class ButtCommands : ReactiveUserControl<ManagerButtonsViewModel>
{
    public ButtCommands()
    {
        InitializeComponent();
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}