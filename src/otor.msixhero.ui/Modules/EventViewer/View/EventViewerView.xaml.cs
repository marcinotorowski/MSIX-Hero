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

        // private void Popup_OnClosed(object sender, EventArgs e)
        //{
        //    var date1 = this.PART_Date1.Value;
        //    var date2 = this.PART_Date1.Value;
        //    var maxLogsString = this.PART_Max.Text;

        //    if (!int.TryParse(maxLogsString, out var maxLogs))
        //    {
        //        return;
        //    }

        //    var newHashCode = GetStateHash(date1 ?? DateTime.Now, date2 ?? DateTime.Now, maxLogs);
        //    if (newHashCode != this.lastHashCode)
        //    {
        //        var dc = (EventViewerViewModel)this.DataContext;
        //        dc.MaxLogs = maxLogs;
        //        dc.Start = date1 ?? DateTime.Now;
        //        dc.End = date2 ?? DateTime.Now;

        //        dc.Reload();
        //    }
        //}
           
        // private void Popup_OnOpened(object? sender, EventArgs e)
        //{
        //    var dc = (EventViewerViewModel) this.DataContext;
        //    var date1 = dc.Start;
        //    var date2 = dc.End;
        //    var max = dc.MaxLogs;

        //    this.PART_Date1.Value = date1;
        //    this.PART_Date2.Value = date2;
        //    this.PART_Max.Text = max.ToString("0");

        //    this.lastHashCode = GetStateHash(date1, date2, max);
        //}
           
        // private static int GetStateHash(DateTime date1, DateTime date2, int max)
        // {
        //     var recentHashCode = 37;
        //     unchecked
        //     {
        //         recentHashCode *= 31 + date1.GetHashCode();
        //         recentHashCode *= 31 + date2.GetHashCode();
        //         recentHashCode *= 31 + max.GetHashCode();
        //         return recentHashCode;
        //     }
        // }
    }
}
