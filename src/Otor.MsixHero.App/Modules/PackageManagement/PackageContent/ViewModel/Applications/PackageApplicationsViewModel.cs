using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Applications.Items;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Applications
{
    public class PackageApplicationsViewModel : PackageLazyLoadingViewModel
    {
        public PackageApplicationsViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }
        
        public override PackageContentViewType Type => PackageContentViewType.Applications;
        
        public ObservableCollection<ApplicationViewModel> StartMenuApplications { get; private set; }

        public ObservableCollection<ApplicationViewModel> CommandLineApplications { get; private set; }

        public ObservableCollection<ApplicationViewModel> OtherApplications { get; private set; }

        public bool HasStartMenuApplications => this.StartMenuApplications?.Any() == true;

        public bool HasCommandLineApplications => this.CommandLineApplications?.Any() == true;

        public bool HasOtherApplications => this.OtherApplications?.Any() == true;

        public ICommand GoBack { get; }

        protected override Task DoLoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
        {
            var startMenuApps = new ObservableCollection<ApplicationViewModel>();
            var commandLineApps = new ObservableCollection<ApplicationViewModel>();
            var otherApps = new ObservableCollection<ApplicationViewModel>();

            foreach (var app in model.Applications)
            {
                var appViewModel = new ApplicationViewModel(app, model);

                if (app.Visible)
                {
                    startMenuApps.Add(appViewModel);
                }
                else if (app.ExecutionAlias?.Any(alias => !string.IsNullOrEmpty(alias)) == true)
                {
                    commandLineApps.Add(appViewModel);
                }
                else
                {
                    otherApps.Add(appViewModel);
                }
            }

            this.StartMenuApplications = startMenuApps;
            this.OtherApplications = otherApps;
            this.CommandLineApplications = commandLineApps;

            if (this.StartMenuApplications.Count > 0)
            {
                if (this.StartMenuApplications.Count < 5)
                {
                    // Do not expand if there is more than 4 apps in the group.
                    this.StartMenuApplications[0].IsExpanded = true;
                }
            }
            else if (this.CommandLineApplications.Count > 0)
            {
                if (this.CommandLineApplications.Count < 5)
                {
                    // Do not expand if there is more than 4 apps in the group.
                    this.CommandLineApplications[0].IsExpanded = true;
                }
            }
            else if(this.OtherApplications.Count > 0)
            {
                if (this.OtherApplications.Count < 5)
                {
                    // Do not expand if there is more than 4 apps in the group.
                    this.OtherApplications[0].IsExpanded = true;
                }
            }

            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }
    }
}
