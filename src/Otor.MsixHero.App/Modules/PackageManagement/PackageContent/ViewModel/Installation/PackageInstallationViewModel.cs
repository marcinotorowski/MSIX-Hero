using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation.InstalledBy;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation.Source;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation
{
    public class PackageInstallationViewModel : PackageLazyLoadingViewModel
    {
        private readonly IUacElevation _uacElevation;
        private ICommand _findUsers;
        private string _filePath;

        public PackageInstallationViewModel(IPackageContentItemNavigation navigation, IUacElevation uacElevation)
        {
            this._uacElevation = uacElevation;
            this.Summary = new SummaryInstallationViewModel(navigation, uacElevation);

            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }

        public ICommand GoBack { get; }

        public override PackageContentViewType Type { get; } = PackageContentViewType.Installation;

        public SummaryInstallationViewModel Summary { get; }
        
        public ObservableCollection<InstalledPackage> AddOns { get; private set; }

        public UserDetailsViewModel Users { get; private set; }

        public bool HasAddOns => this.AddOns?.Any() == true;

        public PackageSourceViewModel ExtraSourceInformation { get; private set; }

        public bool HasExtraSourceInformation => this.ExtraSourceInformation != null;

        public ICommand FindUsers
        {
            get
            {
                return this._findUsers ??= new DelegateCommand(
                    async () =>
                    {
#pragma warning disable 4014
                        using var reader = FileReaderFactory.CreateFileReader(this._filePath);
                        var manifestReader = new AppxManifestReader();
                        var manifest = await manifestReader.Read(reader).ConfigureAwait(false);
                        
                        this.Users = await this.GetUsers(manifest, true).ConfigureAwait(false);
                        this.OnPropertyChanged(null);
#pragma warning restore 4014
                    });
            }
        }

        protected override async Task DoLoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            await this.Summary.LoadPackage(model, filePath, cancellationToken).ConfigureAwait(false);

            if (model.Source is StorePackageSource storePackageSource)
            {
                this.ExtraSourceInformation = new StorePackageSourceViewModel(storePackageSource);
            }
            else if (model.Source is AppInstallerPackageSource appInstallerPackageSource)
            {
                this.ExtraSourceInformation = new AppInstallerSourceViewModel(appInstallerPackageSource);
            }
            else
            {
                this.ExtraSourceInformation = null;
            }

            this._filePath = filePath;
            
            var addOns = new ObservableCollection<InstalledPackage>();
            var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken);

            var usersTask = this.GetUsers(model, false, cancellationToken);
            var addOnsTask = this._uacElevation.AsHighestAvailable<IAppxPackageQuery>().GetModificationPackages(model.FullName, PackageFindMode.Auto, cancellationToken);

            await Task.WhenAll(usersTask, addOnsTask).ConfigureAwait(false);
            
            foreach (var pkg in await addOnsTask.ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                addOns.Add(pkg);
            }

            this.AddOns = addOns;
            this.Users = await usersTask.ConfigureAwait(false);
            this.OnPropertyChanged(null);
        }

        private async Task<UserDetailsViewModel> GetUsers(
            AppxPackage package,
            bool forceElevation,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            try
            {
                IAppxPackageQuery actualSource;

                if (forceElevation)
                {
                    actualSource = this._uacElevation.AsAdministrator<IAppxPackageQuery>();
                }
                else
                {
                    actualSource = this._uacElevation.AsHighestAvailable<IAppxPackageQuery>();
                }

                var stateDetails = await actualSource.GetUsersForPackage(package.FullName, cancellationToken, progress).ConfigureAwait(false);

                if (stateDetails?.Count > 0)
                {
                    return new UserDetailsViewModel(stateDetails, ElevationStatus.Ok);
                }

                return new UserDetailsViewModel(new List<User>(), ElevationStatus.ElevationRequired);
            }
            catch
            {
                return new UserDetailsViewModel(new List<User>(), ElevationStatus.ElevationRequired);
            }
        }
    }
}
