using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.ui.Modules.PackageList;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.VolumeManager.View
{
    /// <summary>
    /// Interaction logic for VolumeManager.xaml
    /// </summary>
    public partial class VolumeManagerView : UserControl
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IRegionManager navigationService;

        public VolumeManagerView(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;
            InitializeComponent();

            FocusManager.SetFocusedElement(this, this.ListBox);
            Keyboard.Focus(this.ListBox);
            this.ListBox.Focus();

            this.Loaded += this.OnLoaded;
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
