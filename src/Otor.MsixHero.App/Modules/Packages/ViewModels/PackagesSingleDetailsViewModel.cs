using System.Collections;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Prism.Regions;
using System.Linq;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels
{
    public class PackagesSingleDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private string filePath;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.First().Value is IEnumerable pkgs)
            {
                this.FilePath = pkgs.OfType<InstalledPackage>().FirstOrDefault()?.InstallLocation ??
                                pkgs.OfType<string>().FirstOrDefault();
            }
            else if (navigationContext.Parameters.First().Value is InstalledPackage ip)
            {
                this.FilePath = ip.InstallLocation;
            }
            else if (navigationContext.Parameters.First().Value is string fp)
            {
                this.FilePath = fp;
            }
            else
            {
                this.FilePath = null;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public string FilePath
        {
            get => this.filePath;
            set
            {
                if (!this.SetField(ref this.filePath, value))
                {
                    return;
                }
            }
        }
    }
}
