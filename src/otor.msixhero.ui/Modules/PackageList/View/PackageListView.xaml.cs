using System;
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
using Otor.MsixHero.Ui.Helpers;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands.Packages;
using Otor.MsixHero.Ui.Hero.Events.Base;
using Otor.MsixHero.Ui.Modules.PackageList.ViewModel;
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

        // ReSharper disable once IdentifierTypo
        private SortAdorner sortAdorner;

        public PackageListView(IMsixHeroApplication application, IRegionManager regionManager = null)
        {
            this.application = application;
            InitializeComponent();
            Debug.Assert(regionManager != null);

            regionManager.RegisterViewWithRegion(Constants.RegionPackageSidebar, typeof(EmptySelectionView));

            // Subscribe to events
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSidebarVisibilityCommand>>().Subscribe(this.OnSetPackageSidebarVisibility, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSortingCommand>>().Subscribe(this.OnSetPackageSorting, ThreadOption.UIThread);
            
            // Set up defaults
            this.UpdateSidebarVisibility();

            var focusable = this.PanelListView.IsVisible ? this.ListView : this.ListBox;
            FocusManager.SetFocusedElement(this, focusable);
            this.Loaded += this.OnLoaded;
            this.IsVisibleChanged += OnIsVisibleChanged;

            this.ListView.SelectionChanged += this.OnListViewSelectionChanged;
            this.ListBox.SelectionChanged += this.OnListBoxSelectionChanged;
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

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0 && e.AddedItems[0] != null)
            {
                this.ListBox.ScrollIntoView(e.AddedItems[0]);
            }
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0 && e.AddedItems[0] != null)
            {
                this.ListView.ScrollIntoView(e.AddedItems[0]);
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
