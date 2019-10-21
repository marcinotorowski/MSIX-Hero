using otor.msihero.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MSI_Hero.Domain;
using MSI_Hero.Domain.Events;
using MSI_Hero.Modules.Installed;
using MSI_Hero.Modules.Installed.View;
using MSI_Hero.ViewModel;
using Prism.Events;
using Prism.Regions;

namespace MSI_Hero
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly IApplicationStateManager appStateManager;

        public MainView()
        {
            this.InitializeComponent();
        }

        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad obj)
        {
            this.Ribbon.ContextualGroups[0].Visibility = this.appStateManager.CurrentState.Packages.SelectedItems.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        public MainView(IRegionManager regionManager, IApplicationStateManager appStateManager) : this()
        {
            regionManager.RegisterViewWithRegion("ContentRegion", typeof(InstalledView));
            regionManager.RequestNavigate("ContentRegion", InstalledModule.Path);
            this.appStateManager = appStateManager;
            appStateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
        }
    }
}
