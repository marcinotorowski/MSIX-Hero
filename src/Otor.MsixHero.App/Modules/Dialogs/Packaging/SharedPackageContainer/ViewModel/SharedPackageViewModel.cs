using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.Manifest;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.ViewModel
{
    public class SharedPackageViewModel : ChangeableContainer
    {
        private bool _isEditing;

        public SharedPackageViewModel()
        { 
            this.AddChildren
            (
                this.FamilyName = new(Resources.Localization.Dialogs_SharedContainer_FamilyName, true, ValidatorFactory.ValidateNotEmptyField()),
                this.Type = new ChangeableProperty<SharedPackageItemType>(),
                this.FullName = new ChangeableProperty<string>(),
                this.FilePath = new ChangeableProperty<string>()
            );
        }

        public static SharedPackageViewModel FromFamilyName(IAppxPackageQueryService queryService, string familyName)
        {
            var newObj = new SharedPackageViewModel();
            newObj.SetFromFamilyName(queryService, familyName, CancellationToken.None).GetAwaiter().GetResult();
            newObj.Type.CurrentValue = SharedPackageItemType.FamilyName;
            newObj.Commit();

            return newObj;
        }
        
        public static SharedPackageViewModel FromInstalledPackage(PackageEntry packageEntry)
        {
            var newObj = new SharedPackageViewModel();
            newObj.FamilyName.CurrentValue = packageEntry.PackageFamilyName;
            newObj.DisplayName = packageEntry.DisplayName;
            newObj.FilePath.CurrentValue = packageEntry.ManifestPath;
            newObj.Type.CurrentValue = SharedPackageItemType.FilePath;
            newObj.Color = packageEntry.TileColor;
            newObj.PublisherDisplayName = packageEntry.DisplayName;
            newObj.Version = packageEntry.DisplayName;
            newObj.LogoPath = packageEntry.ImagePath;
            newObj.Logo = packageEntry.ImageContent;
            newObj.Type.CurrentValue = SharedPackageItemType.Installed;
            newObj.FullName.CurrentValue = packageEntry.PackageFullName;
            newObj.Commit();

            return newObj;
        }
        
        public static SharedPackageViewModel FromInstalledPackage(InstalledPackageViewModel installedPackage)
        {
            return FromInstalledPackage(installedPackage.Model);
        }

        public bool IsEditing
        {
            get => this._isEditing;
            set => this.SetField(ref this._isEditing, value);
        }

        public ChangeableProperty<SharedPackageItemType> Type { get; }

        public ChangeableProperty<string> FullName { get; }

        public ChangeableProperty<string> FilePath { get; }

        public ValidatedChangeableProperty<string> FamilyName { get; }

        public string DisplayName { get; private set; }

        public string Version { get; private set; }
        
        public byte[] Logo { get; private set; }
        
        public string LogoPath { get; private set; }

        public string Color { get; private set; }

        public string PublisherDisplayName { get; private set; }

        public async Task<bool> SetFromFamilyName(IAppxPackageQueryService packageQueryService, string familyName, CancellationToken cancellationToken)
        {
            if (this.FamilyName.CurrentValue == familyName || string.IsNullOrEmpty(familyName))
            {
                return false;
            }

            try
            {
                var installedPackage = await packageQueryService.GetInstalledPackageByFamilyName(familyName, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (installedPackage != null)
                {
                    var ip = await this.SetFromFilePath(installedPackage.ManifestPath, cancellationToken).ConfigureAwait(false);
                    this.Type.CurrentValue = SharedPackageItemType.FamilyName;
                    return ip;
                }
            }
            catch
            {
                // can happen, ignore it
            }

            this.Type.CurrentValue = SharedPackageItemType.FamilyName;
            this.FamilyName.CurrentValue = familyName;
            this.FilePath.CurrentValue = null;
            this.FullName.CurrentValue = null;
            this.Color = null;
            this.DisplayName = null;
            this.PublisherDisplayName = null;
            this.Logo = null;
            this.LogoPath = null;
            this.Version = null;
            this.OnPropertyChanged(null);

            return false;
        }

        public async Task<bool> SetFromFilePath(string filePath, CancellationToken cancellationToken)
        {
            this.Type.CurrentValue = SharedPackageItemType.FilePath;

            try
            {
                using var reader = FileReaderFactory.CreateFileReader(filePath);
                var manifestReader = new AppxManifestReader();
                var pkg = await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);

                this.Color = pkg.Applications.Select(b => b.BackgroundColor).FirstOrDefault(b => b != null);
                this.DisplayName = pkg.DisplayName;
                this.FilePath.CurrentValue = filePath;
                this.PublisherDisplayName = pkg.PublisherDisplayName;
                this.Logo = pkg.Logo;
                this.Version = pkg.Version;
                this.FamilyName.CurrentValue = pkg.FamilyName;
                this.FullName.CurrentValue = pkg.FullName;

                this.OnPropertyChanged(null);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
