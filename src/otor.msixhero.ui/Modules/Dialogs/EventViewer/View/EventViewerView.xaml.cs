using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.EventViewer.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.EventViewer.View
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

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((EventViewerViewModel)this.DataContext).Save().ContinueWith(t =>
            {
                if (t.Exception == null && !t.IsCanceled && t.IsCompleted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Window.GetWindow(this).Close();
                }
            });
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
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

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            var dataContext = ((EventViewerViewModel) this.DataContext);
            e.CanExecute = dataContext.CanCloseDialog() && dataContext.CanSave();
            e.ContinueRouting = !e.CanExecute;
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
