using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Controls.TreeListView;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.View
{
    public partial class DuplicatesView
    {
        private SortAdorner sortAdorner;
        private bool currentSortDescending = true;
        private string currentSortColumn;

        public DuplicatesView()
        {
            InitializeComponent();
            this.SetSorting(nameof(DuplicatedElementViewModel.UpdateImpact), true, true);
        }

        private void SetSorting(string columnName, bool descending, bool onlyView = false)
        {
            SortAdorner newSortAdorner = null;

            var listView = this.FileGrid;

            foreach (var item in listView.Columns.Select(c => c.Header).OfType<GridViewColumnHeader>().Where(c => c.Tag is string))
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

            value?.DuplicatesView.SortDescriptions.Clear();
            if (this.currentSortColumn != null)
            {
                value?.DuplicatesView.SortDescriptions.Add(new SortDescription(this.currentSortColumn, this.currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
            }

            value?.DuplicatesView.Refresh();
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
