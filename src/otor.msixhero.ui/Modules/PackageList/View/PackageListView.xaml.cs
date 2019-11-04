using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.ViewModel;
using Prism.Events;

namespace otor.msixhero.ui.Modules.PackageList.View
{
    /// <summary>
    /// Interaction logic for PackageListView.xaml
    /// </summary>
    public partial class PackageListView
    {
        private readonly IApplicationStateManager applicationStateManager;

        private SortAdorner sortAdorner;

        public PackageListView(IApplicationStateManager applicationStateManager = null)
        {
            this.applicationStateManager = applicationStateManager;
            InitializeComponent();
            Debug.Assert(applicationStateManager != null);

            // Subscribe to events
            applicationStateManager.EventAggregator.GetEvent<PackagesSidebarVisibilityChanged>().Subscribe(OnSidebarVisibilityChanged, ThreadOption.UIThread);
            applicationStateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(OnPackagesSelectionChanged, ThreadOption.UIThread);
            applicationStateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Subscribe(OnPackageGroupAndSortChanged, ThreadOption.UIThread);

            // Set up defaults
            this.UpdateSidebarVisibility();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.SetSorting(this.applicationStateManager.CurrentState.Packages.Sort, this.applicationStateManager.CurrentState.Packages.SortDescending);
        }

        private void OnPackageGroupAndSortChanged(PackageGroupAndSortChangedPayload groupAndSortSettings)
        {
            this.SetSorting(groupAndSortSettings.Sorting, groupAndSortSettings.SortingDescending);
        }

        private void SetSorting(PackageSort sorting, bool descending)
        {
            SortAdorner newSortAdorner = null;

            foreach (var item in this.GridView.Columns.Select(c => c.Header).OfType<GridViewColumnHeader>().Where(c => c.Tag is PackageSort))
            {
                var layer = AdornerLayer.GetAdornerLayer(item);
                if (layer == null)
                {
                    continue;
                }

                if (this.sortAdorner != null)
                {
                    layer.Remove(this.sortAdorner);
                }

                if ((PackageSort) item.Tag == sorting)
                {
                    newSortAdorner = new SortAdorner(item, descending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                    layer.Add(newSortAdorner);
                }
            }

            this.sortAdorner = newSortAdorner;
        }

        private void UpdateSidebarVisibility()
        {
            this.PackageDetails.Visibility = this.applicationStateManager.CurrentState.LocalSettings.ShowSidebar ? Visibility.Visible : Visibility.Collapsed;
            this.GridSplitter.Visibility = this.PackageDetails.Visibility;

            if (this.applicationStateManager.CurrentState.LocalSettings.ShowSidebar)
            {
                this.Grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                this.Grid.ColumnDefinitions[1].Width = GridLength.Auto;
                this.Grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                this.Grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                this.Grid.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel); 
                this.Grid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Pixel);
            }
        }

        private bool disableSelectionNotification = false;

        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad selectionChanged)
        {
            try
            {
                this.disableSelectionNotification = true;

                foreach (var item in selectionChanged.Selected)
                {
                    this.ListView.SelectedItems.Add(item);
                    this.ListBox.SelectedItems.Add(item);
                }

                foreach (var item in selectionChanged.Unselected)
                {
                    this.ListView.SelectedItems.Remove(item);
                    this.ListBox.SelectedItems.Remove(item);
                }
            }
            finally
            {
                this.disableSelectionNotification = false;
            }
        }

        private void OnSidebarVisibilityChanged(bool explicitSidebarVisible)
        {
            this.UpdateSidebarVisibility();
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.disableSelectionNotification)
            {
                return;
            }

            this.applicationStateManager.CommandExecutor.Execute(new SelectPackages(this.ListView.SelectedItems.OfType<PackageViewModel>().Select(s => s.Model)));
            if (this.PanelListBox.Visibility == Visibility.Collapsed)
            {
                if (e.AddedItems != null)
                {
                    foreach (var item in e.AddedItems)
                    {
                        this.ListBox.SelectedItems.Add(item);
                    }
                }

                if (e.RemovedItems != null)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        this.ListBox.SelectedItems.Remove(item);
                    }
                }
            }
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.disableSelectionNotification)
            {
                return;
            }

            this.applicationStateManager.CommandExecutor.Execute(new SelectPackages(this.ListBox.SelectedItems.OfType<PackageViewModel>().Select(s => s.Model)));
            if (this.PanelListView.Visibility == Visibility.Collapsed)
            {
                if (e.AddedItems != null)
                {
                    foreach (var item in e.AddedItems)
                    {
                        this.ListView.SelectedItems.Add(item);
                    }
                }

                if (e.RemovedItems != null)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        this.ListView.SelectedItems.Remove(item);
                    }
                }
            }
        }

        private void ColumnClicked(object sender, RoutedEventArgs e)
        {
            var source = (GridViewColumnHeader) sender;
            var sorting = (PackageSort)source.Tag;

            this.applicationStateManager.CommandExecutor.ExecuteAsync(new SetPackageSorting(sorting));
            e.Handled = true;
        }

        private void OnViewListSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged)
            {
                return;
            }

            this.SetVisibility();
        }

        private void SetVisibility()
        {
            var width = this.Search.ActualWidth;

            if (width > 500)
            {
                this.PanelListView.Visibility = Visibility.Visible;
                this.PanelListBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.PanelListView.Visibility = Visibility.Collapsed;
                this.PanelListBox.Visibility = Visibility.Visible;
            }
        }
    }
}
