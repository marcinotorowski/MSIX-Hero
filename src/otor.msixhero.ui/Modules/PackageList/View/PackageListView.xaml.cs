using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.ViewModel;
using Prism.Events;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.PackageList.View
{
    /// <summary>
    /// Interaction logic for PackageListView.xaml
    /// </summary>
    public partial class PackageListView
    {
        private readonly IApplicationStateManager applicationStateManager;
        private readonly IRegionManager regionManager;
        private bool disableSelectionNotification;

        private SortAdorner sortAdorner;

        public PackageListView(IApplicationStateManager applicationStateManager = null, IRegionManager regionManager = null)
        {
            this.applicationStateManager = applicationStateManager;
            this.regionManager = regionManager;
            InitializeComponent();
            Debug.Assert(applicationStateManager != null);
            Debug.Assert(regionManager != null);

            regionManager.RegisterViewWithRegion("PackageSidebar", typeof(EmptySelectionView));

            // Subscribe to events
            applicationStateManager.EventAggregator.GetEvent<PackagesSidebarVisibilityChanged>().Subscribe(OnSidebarVisibilityChanged);
            applicationStateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(OnPackagesSelectionChanged, ThreadOption.UIThread);
            applicationStateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Subscribe(OnPackageGroupAndSortChanged);

            // Set up defaults
            this.UpdateSidebarVisibility();
            this.Loaded += OnLoaded;

            FocusManager.SetFocusedElement(this, this.ListBox);
            Keyboard.Focus(this.ListBox);
            this.ListBox.Focus();

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Application.Current.Dispatcher.BeginInvoke(
                (Action)(() =>
                {
                    FocusManager.SetFocusedElement(this, this.ListBox);
                    Keyboard.Focus(this.ListBox);
                    this.ListBox.Focus();
                }), 
                DispatcherPriority.ApplicationIdle);
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
            this.PackageDetails.Visibility = this.applicationStateManager.CurrentState.Packages.ShowSidebar ? Visibility.Visible : Visibility.Collapsed;
            this.GridSplitter.Visibility = this.PackageDetails.Visibility;

            if (this.applicationStateManager.CurrentState.Packages.ShowSidebar)
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

        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad selectionChanged)
        {
            var selectedPackages = ((PackageListViewModel)this.DataContext).SelectedPackages;
            if (!this.disableSelectionNotification)
            {
                try
                {
                    this.disableSelectionNotification = true;

                    this.ListView.SelectedItems.Clear();
                    this.ListBox.SelectedItems.Clear();

                    foreach (var item in selectedPackages)
                    {
                        this.ListView.SelectedItems.Add(item);
                        this.ListBox.SelectedItems.Add(item);
                    }
                }
                finally
                {
                    this.disableSelectionNotification = false;
                }
            }

            var selected = selectedPackages.Select(p => p.ProductId).ToArray();
            switch (selected.Length)
            {
                case 0:
                {
                    this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(PackageListModule.SidebarEmptySelection, UriKind.Relative), new NavigationParameters { { "Packages", selected } });
                    break;
                }

                case 1:
                {
                    this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(PackageListModule.SidebarSingleSelection, UriKind.Relative), new NavigationParameters { { "Packages", selected } });
                    break;
                }

                default:
                {
                    this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(PackageListModule.SidebarMultiSelection, UriKind.Relative), new NavigationParameters { { "Packages", selected } });
                    break;
                }
            }
        }

        private void OnSidebarVisibilityChanged(bool explicitSidebarVisible)
        {
            this.UpdateSidebarVisibility();
        }

        private async void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.disableSelectionNotification)
            {
                return;
            }

            try
            {
                this.disableSelectionNotification = true;
                await this.applicationStateManager.CommandExecutor.ExecuteAsync(new SelectPackages(this.ListView.SelectedItems.OfType<InstalledPackageViewModel>().Select(s => s.Model))).ConfigureAwait(true);
                
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

                    if (e.AddedItems?.Count > 0)
                    {
                        this.ListBox.ScrollIntoView(e.AddedItems[0]);
                    }
                }
            }
            finally
            {
                this.disableSelectionNotification = false;
            }
        }

        private async void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.disableSelectionNotification)
            {
                return;
            }

            try
            {
                this.disableSelectionNotification = true;
                await this.applicationStateManager.CommandExecutor.ExecuteAsync(new SelectPackages(this.ListBox.SelectedItems.OfType<InstalledPackageViewModel>().Select(s => s.Model))).ConfigureAwait(false);

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

                    if (e.AddedItems?.Count > 0)
                    {
                        this.ListView.ScrollIntoView(e.AddedItems[0]);
                    }
                }
            }
            finally
            {
                this.disableSelectionNotification = false;
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
