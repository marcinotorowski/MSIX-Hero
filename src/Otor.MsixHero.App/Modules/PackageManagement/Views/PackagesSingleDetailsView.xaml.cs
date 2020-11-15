using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.Views
{
    /// <summary>
    /// Interaction logic for PackagesDetailsView.xaml
    /// </summary>
    public partial class PackagesSingleDetailsView : INavigationAware
    {
        public PackagesSingleDetailsView()
        {
            InitializeComponent();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
