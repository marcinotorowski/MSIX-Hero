using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands.Volumes;
using Otor.MsixHero.Ui.Hero.Events.Base;
using Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel;
using Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel.Elements;
using Prism.Events;

namespace Otor.MsixHero.Ui.Modules.VolumeManager.View
{
    /// <summary>
    /// Interaction logic for Volume Manager.
    /// </summary>
    public partial class VolumeManagerView
    {
        private readonly IMsixHeroApplication application;

        public VolumeManagerView(IMsixHeroApplication application)
        {
            this.application = application;
            InitializeComponent();
            FocusManager.SetFocusedElement(this, this.ListBox);
            this.IsVisibleChanged += OnIsVisibleChanged;

            application.EventAggregator.GetEvent<UiExecutedEvent<SelectVolumesCommand>>().Subscribe(this.OnSelectVolumes, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutingEvent<GetVolumesCommand>>().Subscribe(this.OnExecutingGetVolumes);
            application.EventAggregator.GetEvent<UiCancelledEvent<GetVolumesCommand>>().Subscribe(this.OnCancelledGetVolumes, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumes, ThreadOption.UIThread);
        }

        private bool ignoreSelectionEvents;

        private async void ListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ignoreSelectionEvents)
            {
                return;
            }

            try
            {
                this.ignoreSelectionEvents = true;
                var selected = ((ListBox)sender).SelectedItems.OfType<VolumeViewModel>().Select(v => v.Model.PackageStorePath);
                await this.application.CommandExecutor.Invoke(this, new SelectVolumesCommand(selected)).ConfigureAwait(false);
            }
            finally
            {
                this.ignoreSelectionEvents = false;
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool) e.NewValue)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                FocusManager.SetFocusedElement(Application.Current.MainWindow, this.ListBox);
                FocusManager.SetFocusedElement(this, this.ListBox);
                this.ListBox.Focus();
                Keyboard.Focus(this.ListBox);
            }, DispatcherPriority.ApplicationIdle);
        }
        
        private void ClearSearchField(object sender, RoutedEventArgs e)
        {
            this.SearchBox.Text = string.Empty;
            this.SearchBox.Focus();
            FocusManager.SetFocusedElement(this, this.SearchBox);
            Keyboard.Focus(this.SearchBox);
        }

        private void SearchCommandBindingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Keyboard.Focus(this.SearchBox);
            FocusManager.SetFocusedElement(this, this.SearchBox);
            this.SearchBox.Focus();
        }

        private void OnExecutingGetVolumes(UiExecutingPayload<GetVolumesCommand> obj)
        {
            // this.ignoreSelectionEvents = true;
        }

        private void OnCancelledGetVolumes(UiCancelledPayload<GetVolumesCommand> obj)
        {
            this.ignoreSelectionEvents = false;
        }

        private void OnGetVolumes(UiExecutedPayload<GetVolumesCommand> obj)
        {
            this.ignoreSelectionEvents = false;

            try
            {
                this.ignoreSelectionEvents = true;
                this.ListBox.SelectionChanged -= this.ListBoxOnSelectionChanged;
                this.ListBox.ItemsSource = ((VolumeManagerViewModel) this.DataContext).AllVolumesView;
                ((ICollectionView)this.ListBox.ItemsSource).Refresh();

                this.ListBox.SelectedItems.Clear();
                foreach (var item in ((VolumeManagerViewModel)this.DataContext).GetSelection())
                {
                    this.ListBox.SelectedItems.Add(item);
                }
            }
            finally
            {
                this.ignoreSelectionEvents = false;
                this.ListBox.SelectionChanged += this.ListBoxOnSelectionChanged;
            }
        }

        private void OnSelectVolumes(UiExecutedPayload<SelectVolumesCommand> obj)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (obj.Sender == this || this.ignoreSelectionEvents)
            {
                return;
            }

            try
            {
                this.ignoreSelectionEvents = true;

                this.ListBox.SelectedItems.Clear();
                foreach (var item in ((VolumeManagerViewModel)this.DataContext).GetSelection())
                {
                    this.ListBox.SelectedItems.Add(item);
                }
            }
            finally
            {
                this.ignoreSelectionEvents = false;
            }
        }
    }
}
