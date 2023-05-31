// 
// Program: Scraper One
// Author:  Kyle Crowder
// License : Open Source
// Portions of code taken from TumblrThree
// 
// 052023

using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ScraperOne.DataModels;
using ScraperOne.ViewModels;

namespace ScraperOne.Views
{
    public partial class BlogManagerView : ReactiveUserControl<ManagerViewModel>
    {
        public BlogManagerView()
        {
            ViewModel = new ManagerViewModel();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind<ManagerViewModel, BlogManagerView, ReadOnlyObservableCollection<IBlog>, IEnumerable>(
                        ViewModel,
                        vm => vm.BlogData,
                        view => view.BlogDataGrid2.ItemsSource)
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }

        private DataGrid BlogDataGrid2 => this.FindControl<DataGrid>("BlogDataGrid");

        private void BlogDataGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void BlogDataGrid_OnSelectionChangedelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems as IList<IBlog>;
            ViewModelBase.SelectedItem = items?[0];
            
        }
    }
}