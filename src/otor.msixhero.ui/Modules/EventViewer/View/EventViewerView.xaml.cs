using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.EventViewer.ViewModel;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;

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
        // private int lastHashCode;

        public EventViewerView()
        {
            this.InitializeComponent();
            this.IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && this.currentSortColumn == null)
            {
                Application.Current.Dispatcher.InvokeAsync(
                () =>
                {
                    this.SetSorting("DateTime", true);
                }, 
                DispatcherPriority.ApplicationIdle);
            }
        }

        private void ClearSearchField(object sender, RoutedEventArgs e)
        {
            this.SearchBox.Text = string.Empty;
            this.SearchBox.Focus();
            FocusManager.SetFocusedElement(this, this.SearchBox);
            Keyboard.Focus(this.SearchBox);
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
                    this.currentSortColumn = columnName;
                    break;
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

        private void SearchCommandBindingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Keyboard.Focus(this.SearchBox);
            FocusManager.SetFocusedElement(this, this.SearchBox);
            this.SearchBox.Focus();
        }
    }
}
