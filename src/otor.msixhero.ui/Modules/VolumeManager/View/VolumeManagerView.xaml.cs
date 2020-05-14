using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Volumes;
using Prism.Events;

namespace otor.msixhero.ui.Modules.VolumeManager.View
{
    /// <summary>
    /// Interaction logic for Volume Manager.
    /// </summary>
    public partial class VolumeManagerView
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IEventAggregator eventAggregator;
        private bool ignoreSelectionChanged;

        public VolumeManagerView(IApplicationStateManager stateManager, IEventAggregator eventAggregator)
        {
            this.stateManager = stateManager;
            this.eventAggregator = eventAggregator;
            InitializeComponent();
            FocusManager.SetFocusedElement(this, this.ListBox);
            this.IsVisibleChanged += OnIsVisibleChanged;
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

        private void ListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ignoreSelectionChanged)
            {
                return;
            }

            try
            {
                this.ignoreSelectionChanged = true;
                var command = new SelectVolumes(((ListBox) sender).SelectedItems?.OfType<AppxVolume>());
                this.stateManager.CommandExecutor.Execute(command);
            }
            finally
            {
                this.ignoreSelectionChanged = false;
            }
        }

        private void ClearSearchField(object sender, RoutedEventArgs e)
        {
            this.SearchBox.Text = string.Empty;
            this.SearchBox.Focus();
            FocusManager.SetFocusedElement(this, this.SearchBox);
            Keyboard.Focus(this.SearchBox);
        }
    }
}
