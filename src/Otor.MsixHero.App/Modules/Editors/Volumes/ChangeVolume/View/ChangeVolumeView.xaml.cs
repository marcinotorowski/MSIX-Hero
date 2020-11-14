using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Otor.MsixHero.App.Modules.Editors.Volumes.ChangeVolume.ViewModel;
using Otor.MsixHero.App.Modules.Editors.Volumes.ChangeVolume.ViewModel.Items;

namespace Otor.MsixHero.App.Modules.Editors.Volumes.ChangeVolume.View
{
    /// <summary>
    /// Interaction logic for Change Volume View.
    /// </summary>
    public partial class ChangeVolumeView
    {
        public ChangeVolumeView()
        {
            InitializeComponent();
        }

        private void CreateNew(object sender, RoutedEventArgs e)
        {
            ((ChangeVolumeViewModel) this.DataContext).CreateNew();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var oldValue = e.RemovedItems?.OfType<VolumeCandidateViewModel>().FirstOrDefault();
            var newValue = e.AddedItems?.OfType<VolumeCandidateViewModel>().FirstOrDefault();

            if (newValue?.Name == null)
            {
                ((Selector) sender).SelectedValue = oldValue?.Name;
                ((ChangeVolumeViewModel)this.DataContext).CreateNew();
            }
        }
    }
}
