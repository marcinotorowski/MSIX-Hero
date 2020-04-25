using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.State;
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
            Keyboard.Focus(this.ListBox);
            this.ListBox.Focus();
            this.Loaded += this.OnLoaded;
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Application.Current.Dispatcher.BeginInvoke(
                (Action)(() =>
                {
                    FocusManager.SetFocusedElement(this, this.ListBox);
                    Keyboard.Focus(this.ListBox);
                    this.ListBox.Focus();
                }),
                DispatcherPriority.ApplicationIdle);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.Packages));
        }
    }
}
