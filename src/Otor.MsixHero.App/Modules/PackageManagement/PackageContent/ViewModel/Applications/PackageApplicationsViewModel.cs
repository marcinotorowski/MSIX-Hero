using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Applications
{
    public class PackageApplicationsViewModel : NotifyPropertyChanged, IPackageContentItem, ILoadPackage
    {
        public PackageApplicationsViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }

        private bool _isActive;

        public PackageContentViewType Type => PackageContentViewType.Applications;

        public bool IsActive
        {
            get => this._isActive;
            set => this.SetField(ref this._isActive, value);
        }

        public Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
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

            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }
        
        public ObservableCollection<ApplicationViewModel> StartMenuApplications { get; private set; }
        public ObservableCollection<ApplicationViewModel> CommandLineApplications { get; private set; }
        public ObservableCollection<ApplicationViewModel> OtherApplications { get; private set; }

        public bool HasStartMenuApplications => this.StartMenuApplications?.Any() == true;

        public bool HasCommandLineApplications => this.CommandLineApplications?.Any() == true;

        public bool HasOtherApplications => this.OtherApplications?.Any() == true;

        public ICommand GoBack { get; }
    }
}
