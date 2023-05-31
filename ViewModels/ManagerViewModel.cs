// 
// Program: Scraper One
// Author:  Kyle Crowder
// License : Open Source
// Portions of code taken from TumblrThree
// 
// 052023

using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using ReactiveUI;
using ScraperOne.DataModels;

namespace ScraperOne.ViewModels
{
    public class ManagerViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<IBlog> _data;
        public ReadOnlyObservableCollection<IBlog> BlogData => _data;


        public ManagerViewModel()
        {
            iManagerService.IBlogConnection()
                .Sort(SortExpressionComparer<IBlog>.Ascending(e => e.Name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _data)
                .Subscribe();
        }



        private void BlogDataGrid_OnSelectionChanged([CanBeNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            if (sender is not DataGrid) return;
            SelectedItem = (IBlog)e.AddedItems[0];
            e.Handled = true;
        }
    }
}