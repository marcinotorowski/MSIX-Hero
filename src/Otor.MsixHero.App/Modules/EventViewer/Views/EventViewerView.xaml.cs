using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.EventViewer.ViewModels;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;

namespace Otor.MsixHero.App.Modules.EventViewer.Views
{
    /// <summary>
    /// Interaction logic for EventViewerView.
    /// </summary>
    public partial class EventViewerView
    {
        private SortAdorner sortAdorner;
        private string currentSortColumn;
        private bool currentSortDescending;

        private readonly EventViewerCommandHandler commandHandler;
        // private int lastHashCode;

        public EventViewerView(IMsixHeroApplication application, IInteractionService interactionService, IBusyManager busyManager)
        {
            this.InitializeComponent();
            this.IsVisibleChanged += OnIsVisibleChanged;
            this.commandHandler = new EventViewerCommandHandler(this, application, interactionService, busyManager);
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
    }
}
