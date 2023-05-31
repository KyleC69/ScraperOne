using ReactiveUI;
using ScraperOne.DataModels;
using ScraperOne.Services;

namespace ScraperOne.ViewModels
{
    public class ViewModelBase : ReactiveObject, IActivatableViewModel
    {
        public ViewModelBase()
        {
            Activator = new();
        }

        public static IBlog SelectedItem { get; set; }

        public ManagerService iManagerService => DiServiceLoader.ManagerService;
        public ViewModelActivator Activator { get; }
    }
}