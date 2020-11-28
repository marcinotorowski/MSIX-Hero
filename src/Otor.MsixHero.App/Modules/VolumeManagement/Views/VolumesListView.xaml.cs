using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.VolumeManagement.ViewModels;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.VolumeManagement.Views
{
    /// <summary>
    /// Interaction logic for VolumesListView.
    /// </summary>
    public partial class VolumesListView
    {
        private readonly IMsixHeroApplication application;

        public VolumesListView(IMsixHeroApplication application)
        {
            InitializeComponent();
            this.application = application;

            application.EventAggregator.GetEvent<UiFailedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesFailed, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutingEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesExecuting);
            application.EventAggregator.GetEvent<UiCancelledEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesCancelled, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesExecuted, ThreadOption.UIThread);
            this.InitializeComponent();
            this.ListBox.PreviewKeyDown += ListBoxOnKeyDown;
            this.ListBox.PreviewKeyUp += ListBoxOnKeyUp;
        }

        private void ListBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged -= this.OnSelectionChanged;
            }
        }

        private void ListBoxOnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged += this.OnSelectionChanged;

                this.application.CommandExecutor.Invoke(this, new SelectVolumesCommand(this.ListBox.SelectedItems.OfType<VolumeViewModel>().Select(p => p.PackageStorePath)));
            }
        }

        private void OnGetVolumesExecuting(UiExecutingPayload<GetVolumesCommand> obj)
        {
            this.ListBox.SelectionChanged -= this.OnSelectionChanged;
        }

        private void OnGetVolumesFailed(UiFailedPayload<GetVolumesCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetVolumesCancelled(UiCancelledPayload<GetVolumesCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetVolumesExecuted(UiExecutedPayload<GetVolumesCommand> obj)
        {
            this.ListBox.SelectedItems.Clear();

            foreach (var item in this.ListBox.Items.OfType<VolumeViewModel>())
            {
                if (!this.application.ApplicationState.Volumes.SelectedVolumes.Contains(item.Model))
                {
                    continue;
                }

                this.ListBox.SelectedItems.Add(item);
            }

            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }
        
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.application.CommandExecutor.Invoke(this, new SelectVolumesCommand(this.ListBox.SelectedItems.OfType<VolumeViewModel>().Select(p => p.PackageStorePath)));
        }
    }
}
