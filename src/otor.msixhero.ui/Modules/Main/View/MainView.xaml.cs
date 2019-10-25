using System.Linq;
using System.Windows;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.ui.Modules.PackageList;
using otor.msixhero.ui.Modules.PackageList.View;
using Prism.Events;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.Main.View
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
            regionManager.RegisterViewWithRegion("ContentRegion", typeof(PackageListView));
            regionManager.RequestNavigate("ContentRegion", PackageListModule.Path);
            this.appStateManager = appStateManager;
            appStateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
        }
    }
}
