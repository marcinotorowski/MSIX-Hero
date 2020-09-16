using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.Events;
using Otor.MsixHero.Ui.Commands;
using Otor.MsixHero.Ui.Helpers;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands.Packages;
using Otor.MsixHero.Ui.Hero.Events.Base;
using Otor.MsixHero.Ui.Modules.PackageList.ViewModel;
using Otor.MsixHero.Ui.Modules.PackageList.ViewModel.Elements;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.PackageList.View
{
    /// <summary>
    /// Interaction logic for PackageListView.xaml
    /// </summary>
    public partial class PackageListView
    {
        private readonly IMsixHeroApplication application;
        private readonly IConfigurationService configService;
        private IList<MenuItem> tools;

        // ReSharper disable once IdentifierTypo
        private SortAdorner sortAdorner;

        public PackageListView(IMsixHeroApplication application, IConfigurationService configService, IRegionManager regionManager = null)
        {
            this.application = application;
            this.configService = configService;
            InitializeComponent();
            Debug.Assert(regionManager != null);

            regionManager.RegisterViewWithRegion(Constants.RegionPackageSidebar, typeof(EmptySelectionView));

            // Subscribe to events
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSidebarVisibilityCommand>>().Subscribe(this.OnSetPackageSidebarVisibility, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSortingCommand>>().Subscribe(this.OnSetPackageSorting, ThreadOption.UIThread);

            application.EventAggregator.GetEvent<UiExecutedEvent<SelectPackagesCommand>>().Subscribe(this.OnSelectPackages, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutingEvent<GetPackagesCommand>>().Subscribe(this.OnExecutingGetPackages);
            application.EventAggregator.GetEvent<UiCancelledEvent<GetPackagesCommand>>().Subscribe(this.OnCancelledGetPackages, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<ToolsChangedEvent>().Subscribe(payload => this.tools = null);

            // Set up defaults
            this.UpdateSidebarVisibility();

            var focusable = this.PanelListView.IsVisible ? this.ListView : this.ListBox;
            FocusManager.SetFocusedElement(this, focusable);
            this.Loaded += this.OnLoaded;
            this.IsVisibleChanged += OnIsVisibleChanged;
        }
        
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                FocusManager.SetFocusedElement(Application.Current.MainWindow, this.ListBox);
                FocusManager.SetFocusedElement(this, this.ListBox);
                this.ListBox.Focus();
                Keyboard.Focus(this.ListBox);
            }, DispatcherPriority.ApplicationIdle);
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;
            this.SetSorting(this.application.ApplicationState.Packages.SortMode, this.application.ApplicationState.Packages.SortDescending);
        }

        private void OnSetPackageSorting(UiExecutedPayload<SetPackageSortingCommand> obj)
        {
            this.SetSorting(this.application.ApplicationState.Packages.SortMode, this.application.ApplicationState.Packages.SortDescending);
        }

        private void SetSorting(PackageSort sorting, bool descending)
        {
            // ReSharper disable once IdentifierTypo
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
            this.PackageDetails.Visibility = this.application.ApplicationState.Packages.ShowSidebar ? Visibility.Visible : Visibility.Collapsed;
            this.GridSplitter.Visibility = this.PackageDetails.Visibility;
            this.Separator.Visibility = this.PackageDetails.Visibility;

            if (this.application.ApplicationState.Packages.ShowSidebar)
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

        private void OnSetPackageSidebarVisibility(UiExecutedPayload<SetPackageSidebarVisibilityCommand> obj)
        {
            this.UpdateSidebarVisibility();
        }

        private async void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ignoreListViewSelectionEvents)
            {
                return;
            }

            try
            {
                this.ignoreListBoxSelectionEvents = true;
                this.ListBox.SelectedItems.Clear();

                foreach (var item in this.ListView.SelectedItems)
                {
                    this.ListBox.SelectedItems.Add(item);
                }
            }
            finally
            {
                this.ignoreListBoxSelectionEvents = false;
            }

            try
            {
                this.ignoreListViewSelectionEvents = true;
                var selected = ((ListView)sender).SelectedItems.OfType<InstalledPackageViewModel>().Select(v => v.Model.ManifestLocation);
                await this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand(selected)).ConfigureAwait(false);
            }
            finally
            {
                this.ignoreListViewSelectionEvents = false;
            }
        }
        
        private async void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ignoreListBoxSelectionEvents)
            {
                return;
            }

            try
            {
                this.ignoreListViewSelectionEvents = true;
                this.ListView.SelectedItems.Clear();

                foreach (var item in this.ListBox.SelectedItems)
                {
                    this.ListView.SelectedItems.Add(item);
                }
            }
            finally
            {
                this.ignoreListViewSelectionEvents = false;
            }
            
            try
            {
                this.ignoreListBoxSelectionEvents = true;
                var selected = ((ListBox)sender).SelectedItems.OfType<InstalledPackageViewModel>().Select(v => v.Model.ManifestLocation);
                await this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand(selected)).ConfigureAwait(false);
            }
            finally
            {
                this.ignoreListBoxSelectionEvents = false;
            }
        }

        private void ColumnClicked(object sender, RoutedEventArgs e)
        {
            var source = (GridViewColumnHeader) sender;
            var sorting = (PackageSort)source.Tag;

            this.application.CommandExecutor.Invoke(this, new SetPackageSortingCommand(sorting));
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

            var wasListBoxVisible = this.PanelListBox.Visibility != Visibility.Collapsed;
            
            if (width > 500)
            {
                this.PanelListView.Visibility = Visibility.Visible;
                this.PanelListBox.Visibility = Visibility.Collapsed;

                if (wasListBoxVisible && this.ListView.SelectedItems.Count > 0)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    this.ListView.ScrollIntoView(this.ListView.SelectedItems[0]);
                }
            }
            else
            {
                this.PanelListView.Visibility = Visibility.Collapsed;
                this.PanelListBox.Visibility = Visibility.Visible;

                if (!wasListBoxVisible && this.ListBox.SelectedItems.Count > 0)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    this.ListBox.ScrollIntoView(this.ListBox.SelectedItems[0]);
                }
            }
        }

        private void ListOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((PackageListViewModel)this.DataContext).ShowSelectionDetails.Execute(null);
        }

        private void ClearSearchField(object sender, RoutedEventArgs e)
        {
            this.SearchBox.Text = string.Empty;
            this.SearchBox.Focus();
            FocusManager.SetFocusedElement(this, this.SearchBox);
            Keyboard.Focus(this.SearchBox);
        }
        
        private void ToggleContext_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    this.ToggleContext.Focus();
                    Keyboard.Focus(this.ToggleContext);
                    FocusManager.SetFocusedElement(this, this.ToggleContext);
                    // ReSharper disable once AssignNullToNotNullAttribute
                    FocusManager.SetFocusedElement(Window.GetWindow(this), this);
                },
                DispatcherPriority.ApplicationIdle);
        }

        private void UIElement_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.ToggleContext.IsChecked = false;
            ((RadioButton) sender).IsChecked = true;
            e.Handled = true;
        }
        
        private void SearchCommandBindingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Keyboard.Focus(this.SearchBox);
            FocusManager.SetFocusedElement(this, this.SearchBox);
            this.SearchBox.Focus();
        }

        private bool ignoreListBoxSelectionEvents;
        private bool ignoreListViewSelectionEvents;

        private void OnExecutingGetPackages(UiExecutingPayload<GetPackagesCommand> obj)
        {
            // this.ignoreSelectionEvents = true;
        }

        private void OnCancelledGetPackages(UiCancelledPayload<GetPackagesCommand> obj)
        {
            this.ignoreListBoxSelectionEvents = false;
            this.ignoreListViewSelectionEvents = false;
        }

        private void OnGetPackages(UiExecutedPayload<GetPackagesCommand> obj)
        {
            this.ignoreListViewSelectionEvents = false;
            this.ignoreListBoxSelectionEvents = false;

            try
            {
                this.ignoreListViewSelectionEvents = true;
                this.ignoreListBoxSelectionEvents = true;
                this.ListBox.SelectionChanged -= this.OnListBoxSelectionChanged;
                this.ListView.SelectionChanged -= this.OnListViewSelectionChanged;

                this.ListBox.ItemsSource = ((PackageListViewModel)this.DataContext).AllPackagesView;
                this.ListView.ItemsSource = ((PackageListViewModel)this.DataContext).AllPackagesView;
                ((PackageListViewModel)this.DataContext).AllPackagesView.Refresh();

                this.ListBox.SelectedItems.Clear();
                this.ListView.SelectedItems.Clear();
                foreach (var item in ((PackageListViewModel)this.DataContext).GetSelection())
                {
                    this.ListBox.SelectedItems.Add(item);
                    this.ListView.SelectedItems.Add(item);
                }
            }
            finally
            {
                this.ignoreListViewSelectionEvents = false;
                this.ignoreListBoxSelectionEvents = false;
                this.ListBox.SelectionChanged += this.OnListBoxSelectionChanged;
                this.ListView.SelectionChanged += this.OnListViewSelectionChanged;
            }
        }

        private void OnSelectPackages(UiExecutedPayload<SelectPackagesCommand> obj)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (obj.Sender == this)
            {
                return;
            }

            if (!this.ignoreListViewSelectionEvents)
            {
                try
                {
                    this.ignoreListViewSelectionEvents = true;

                    this.ListView.SelectedItems.Clear();
                    foreach (var item in ((PackageListViewModel)this.DataContext).GetSelection())
                    {
                        this.ListView.SelectedItems.Add(item);
                    }
                }
                finally
                {
                    this.ignoreListViewSelectionEvents = false;
                }
            }

            if (!this.ignoreListBoxSelectionEvents)
            {
                try
                {
                    this.ignoreListBoxSelectionEvents = true;

                    this.ListBox.SelectedItems.Clear();
                    foreach (var item in ((PackageListViewModel)this.DataContext).GetSelection())
                    {
                        this.ListBox.SelectedItems.Add(item);
                    }
                }
                finally
                {
                    this.ignoreListBoxSelectionEvents = false;
                }
            }
        }

        private void PackageContextMenu_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (this.tools != null)
            {
                return;
            }

            this.SetTools();
            var frameworkElement = (FrameworkElement) sender;
            // ReSharper disable once PossibleNullReferenceException
            var lastMenu = frameworkElement.ContextMenu.Items.OfType<MenuItem>().Last();
            
            lastMenu.Items.Clear();
            foreach (var item in this.tools)
            {
                lastMenu.Items.Add(item);
            }

            lastMenu.Items.Add(new Separator());
            lastMenu.Items.Add(new MenuItem
            {
                Command = MsixHeroCommands.Settings,
                CommandParameter = "tools",
                Header = "More commands..."
            });
        }

        private void SetTools()
        {
            if (this.tools != null)
            {
                return;
            }
            
            this.tools = new List<MenuItem>();
            var configuredTools = this.configService.GetCurrentConfiguration().List.Tools;
            
            foreach (var item in configuredTools)
            {
                this.tools.Add(new MenuItem
                {
                    Command = MsixHeroCommands.RunTool,
                    Icon = new Image() { Source = ShellIcon.GetIconFor(string.IsNullOrEmpty(item.Icon) ? item.Path : item.Icon) },
                    Header = item.Name,
                    CommandParameter = item
                });
            }
        }
    }

    internal class PackageContextDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PackageContext context)
            {
                switch (context)
                {
                    case PackageContext.CurrentUser:
                        return "Current user";
                    case PackageContext.AllUsers:
                        return "All users";
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
