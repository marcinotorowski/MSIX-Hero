using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.UpdateImpact.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.UpdateImpact.ViewModel.Items;
using otor.msixhero.ui.Modules.EventViewer.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.UpdateImpact.View
{
    /// <summary>
    /// Interaction logic for UpdateImpactView.
    /// </summary>
    public partial class UpdateImpactView
    {
        private SortAdorner sortAdorner;
        private bool currentSortDescending = true;
        private string currentSortColumn;

        public UpdateImpactView()
        {
            this.InitializeComponent();
        }
        
        private void SetSorting(string columnName, bool descending)
        {
            SortAdorner newSortAdorner = null;

            var listView = this.Dialog.FindDialogTemplateName<ListView>("FileGrid");
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

        private void Header1Clicked(object sender, MouseButtonEventArgs e)
        {
            var dataContext = (UpdateImpactViewModel) this.DataContext;
            var current = dataContext.Path1.CurrentValue;
            dataContext.Path1.Browse.Execute(null);
            if (current == dataContext.Path1.CurrentValue)
            {
                return;
            }

            dataContext.Compare.Execute(null);
        }

        private void Header2Clicked(object sender, MouseButtonEventArgs e)
        {
            var dataContext = (UpdateImpactViewModel)this.DataContext;
            var current = dataContext.Path2.CurrentValue;
            dataContext.Path2.Browse.Execute(null);
            if (current == dataContext.Path1.CurrentValue)
            {
                return;
            }

            dataContext.Compare.Execute(null);
        }
    }
}
