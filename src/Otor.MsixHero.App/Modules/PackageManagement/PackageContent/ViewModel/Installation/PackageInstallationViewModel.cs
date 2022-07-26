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
using Otor.MsixHero.App.Mvvm;
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
    public class PackageInstallationViewModel : NotifyPropertyChanged, IPackageContentItem, ILoadPackage
    {
        private readonly IUacElevation uacElevation;
        private bool _isActive;
        private ICommand _findUsers;
        private string _filePath;

        public PackageInstallationViewModel(IPackageContentItemNavigation navigation, IUacElevation uacElevation)
        {
            this.uacElevation = uacElevation;
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }

        public ICommand GoBack { get; }

        public PackageContentViewType Type { get; } = PackageContentViewType.Installation;

        public bool IsActive
        {
            get => _isActive;
            set => this.SetField(ref this._isActive, value);
        }

        public PackageSourceViewModel Source { get; private set; }

        public ObservableCollection<InstalledPackage> AddOns { get; private set; }

        public UserDetailsViewModel Users { get; private set; }

        public bool HasAddOns => this.AddOns?.Any() == true;

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

        public async Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this._filePath = filePath;
            this.Source = model.Source switch
            {
                NotInstalledSource nis => new NotInstalledSourceViewModel(nis),
                AppInstallerPackageSource appInstaller => new AppInstallerSourceViewModel(appInstaller),
                DeveloperSource developer => new DeveloperSourceViewModel(developer),
                StandardSource standard => new StandardSourceViewModel(standard),
                StorePackageSource store => new StorePackageSourceViewModel(store),
                SystemSource system => new SystemSourceViewModel(system),
                _ => null
            };

            var addOns = new ObservableCollection<InstalledPackage>();
            var canElevate = await UserHelper.IsAdministratorAsync(cancellationToken);

            var usersTask = this.GetUsers(model, false, cancellationToken);
            var addOnsTask = this.uacElevation.AsHighestAvailable<IAppxPackageQuery>().GetModificationPackages(model.FullName, PackageFindMode.Auto, cancellationToken);

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
                    actualSource = this.uacElevation.AsAdministrator<IAppxPackageQuery>();
                }
                else
                {
                    actualSource = this.uacElevation.AsHighestAvailable<IAppxPackageQuery>();
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
