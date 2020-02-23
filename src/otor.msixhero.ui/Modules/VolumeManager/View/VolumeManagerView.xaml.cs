using System;
using System.Windows;
using System.Windows.Controls;
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
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.Packages));
        }
    }
}
