using System.Collections.ObjectModel;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Prism.Commands;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.Details.ViewModels
{
    internal class PackageManyDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IMsixHeroApplication _application;

        public PackageManyDetailsViewModel(IMsixHeroApplication application)
        {
            this._application = application;
            this.Select = new DelegateCommand<object>(param =>
            {
                application.CommandExecutor.Invoke(this, new SelectPackagesCommand((string)param));
            });
        }
        
        public ICommand Select { get; }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.Apps.Clear();
            foreach (var item in this._application.ApplicationState.Packages.SelectedPackages)
            {
                this.Apps.Add(item);
            }
        }

        public ObservableCollection<InstalledPackage> Apps { get; private set; } = new ObservableCollection<InstalledPackage>();

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
