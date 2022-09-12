using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.ViewModel
{
    public enum SharedPackageItemType
    {
        New,
        FilePath,
        FamilyName
    }

    public class SharedPackageViewModel : ChangeableContainer
    {
        private bool _isEditing;

        public static SharedPackageViewModel Create(IAppxPackageQuery query, string familyName)
        {
            var newObj = new SharedPackageViewModel();
            newObj.SetFromFamilyName(query, familyName, CancellationToken.None).GetAwaiter().GetResult();
            newObj.FamilyName.Commit();
            return newObj;
        }

        public static SharedPackageViewModel Create(InstalledPackage installedPackage)
        {
            var newObj = new SharedPackageViewModel();
            newObj.FamilyName.CurrentValue = installedPackage.PackageFamilyName;
            newObj.DisplayName = installedPackage.DisplayName;
            newObj.FilePath = installedPackage.ManifestLocation;
            newObj.Type = SharedPackageItemType.FilePath;
            newObj.Color = installedPackage.TileColor;
            newObj.PublisherDisplayName = installedPackage.DisplayName;
            newObj.Version = installedPackage.DisplayName;
            newObj.LogoPath = installedPackage.Image;

            newObj.FamilyName.Commit();
            return newObj;
        }

        public bool IsEditing
        {
            get => this._isEditing;
            set => this.SetField(ref this._isEditing, value);
        }

        public SharedPackageItemType Type { get; set; } = SharedPackageItemType.New;

        public ValidatedChangeableProperty<string> FamilyName { get; } = new("Family name", true, ValidatorFactory.ValidateNotEmptyField());

        public string DisplayName { get; private set; }
        
        public string FilePath { get; private set; }

        public string Version { get; private set; }
        
        public byte[] Logo { get; private set; }
        
        public string LogoPath { get; private set; }

        public string Color { get; private set; }

        public string PublisherDisplayName { get; private set; }

        public async Task<bool> SetFromFamilyName(IAppxPackageQuery packageQuery, string familyName, CancellationToken cancellationToken)
        {
            this.Type = SharedPackageItemType.FamilyName;

            if (this.FamilyName.CurrentValue == familyName || string.IsNullOrEmpty(familyName))
            {
                return false;
            }

            try
            {
                var installedPackage = await packageQuery.GetInstalledPackageByFamilyName(familyName, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (installedPackage != null)
                {
                    return await this.SetFromFilePath(installedPackage.ManifestLocation, cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                // can happen, ignore it
            }

            this.FamilyName.CurrentValue = familyName;
            this.FilePath = null;
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
            this.Type = SharedPackageItemType.FilePath;

            try
            {
                using var reader = FileReaderFactory.CreateFileReader(filePath);
                var manifestReader = new AppxManifestReader();
                var pkg = await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);

                this.Color = pkg.Applications.Select(b => b.BackgroundColor).FirstOrDefault(b => b != null);
                this.DisplayName = pkg.DisplayName;
                this.FilePath = filePath;
                this.PublisherDisplayName = pkg.PublisherDisplayName;
                this.Logo = pkg.Logo;
                this.LogoPath = null;
                this.Version = pkg.Version;
                this.FamilyName.CurrentValue = pkg.FamilyName;

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
