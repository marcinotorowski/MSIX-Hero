using System.Collections.ObjectModel;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Prism.Commands;
using Prism.Navigation.Regions;

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
                if (param is string fullName)
                {
                    application.CommandExecutor.Invoke(this, new SelectPackagesCommand(new PackageLUID(fullName)));
                }
                else if (param is PackageEntry package)
                {
                    application.CommandExecutor.Invoke(this, new SelectPackagesCommand(new PackageLUID(package)));
                }
                else if (param is PackageLUID uid)
                {
                    application.CommandExecutor.Invoke(this, new SelectPackagesCommand(uid));
                }
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

        public ObservableCollection<PackageEntry> Apps { get; private set; } = new();

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
