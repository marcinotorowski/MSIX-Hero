using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.View
{
    public partial class FilesView
    {
        private SortAdorner sortAdorner;
        private bool currentSortDescending = true;
        private string currentSortColumn;
        
        public FilesView()
        {
            this.InitializeComponent();
            this.SetSorting(nameof(FileViewModel.UpdateImpact), true, true);
        }

        private void SetSorting(string columnName, bool descending, bool onlyView = false)
        {
            SortAdorner newSortAdorner = null;

            var listView = this.FileGrid;
            var gridView = (GridView)listView.View;

            foreach (var item in gridView.Columns.Select(c => c.Header).OfType<GridViewColumnHeader>().Where(c => c.Tag is string))
            {
                var layer = AdornerLayer.GetAdornerLayer(item);
                if (layer == null)
                {
                    break;
                }

                if (this.sortAdorner != null)
                {
                    layer.Remove(this.sortAdorner);
                }

                if ((string)item.Tag == columnName)
                {
                    newSortAdorner = new SortAdorner(item, descending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                    layer.Add(newSortAdorner);
                    this.currentSortColumn = columnName;
                    break;
                }
            }

            this.sortAdorner = newSortAdorner;

            if (onlyView)
            {
                return;
            }    
            
            var value = ((UpdateImpactViewModel)this.DataContext).Results.CurrentValue;

            value?.FilesView.SortDescriptions.Clear();
            if (this.currentSortColumn != null)
            {
                value?.FilesView.SortDescriptions.Add(new SortDescription(this.currentSortColumn, this.currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
            }

            value?.FilesView.Refresh();
        }

        private void GridHeaderOnClick(object sender, RoutedEventArgs e)
        {
            var tag = (string)((GridViewColumnHeader)sender).Tag;

            if (this.currentSortColumn == tag)
            {
                this.currentSortDescending = !this.currentSortDescending;
            }
            else
            {
                this.currentSortDescending = false;
            }

            this.currentSortColumn = tag;

            this.SetSorting(this.currentSortColumn, this.currentSortDescending);
        }

        private void FilesDockVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && this.currentSortColumn == null)
            {
                Application.Current.Dispatcher.InvokeAsync(
                    () =>
                    {
                        this.SetSorting(nameof(FileViewModel.UpdateImpact), true);
                    },
                    DispatcherPriority.ApplicationIdle);
            }
        }
    }
}
