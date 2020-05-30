using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.EventViewer.ViewModel;

namespace otor.msixhero.ui.Modules.EventViewer.View
{
    /// <summary>
    /// Interaction logic for EventViewerView.
    /// </summary>
    public partial class EventViewerView
    {
        private SortAdorner sortAdorner;
        private string currentSortColumn;
        private bool currentSortDescending;

        public EventViewerView()
        {
            this.InitializeComponent();
        }

        private void SetSorting(string columnName, bool descending)
        {
            SortAdorner newSortAdorner = null;

            foreach (var item in this.GridView.Columns.Select(c => c.Header).OfType<GridViewColumnHeader>().Where(c => c.Tag is string))
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

                if ((string)item.Tag == columnName)
                {
                    newSortAdorner = new SortAdorner(item, descending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                    layer.Add(newSortAdorner);
                }
            }

            this.sortAdorner = newSortAdorner;
            ((EventViewerViewModel) this.DataContext).Sort(columnName, descending);
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
    }
}
