using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedAppContainer.ViewModel
{
    public class SharedAppViewModel : ChangeableContainer
    {
        private bool _isEditing;

        public static SharedAppViewModel Create(IAppxPackageQuery query, string familyName)
        {
            var newObj = new SharedAppViewModel();
            newObj.SetFromFamilyName(query, familyName, CancellationToken.None).GetAwaiter().GetResult();
            newObj.FamilyName.Commit();
            return newObj;
        }

        public bool IsEditing
        {
            get => this._isEditing;
            set => this.SetField(ref this._isEditing, value);
        }

        public ValidatedChangeableProperty<string> FamilyName { get; } = new("Family name", true, ValidatorFactory.ValidateNotEmptyField());

        public string DisplayName { get; private set; }
        
        public string Version { get; private set; }
        
        public byte[] Logo { get; private set; }

        public string Color { get; private set; }

        public string PublisherDisplayName { get; private set; }

        public async Task<bool> SetFromFamilyName(IAppxPackageQuery packageQuery, string familyName, CancellationToken cancellationToken)
        {
            this.FamilyName.CurrentValue = familyName;

            try
            {
                var installedPackage = await packageQuery.GetInstalledPackageByFamilyName(this.FamilyName.CurrentValue, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (installedPackage != null)
                {
                    return await this.SetFromFilePath(installedPackage.ManifestLocation, cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                // can happen, ignore it
            }

            this.Color = null;
            this.DisplayName = null;
            this.PublisherDisplayName = null;
            this.Logo = null;
            this.Version = null;
            this.OnPropertyChanged(null);

            return false;
        }

        public async Task<bool> SetFromFilePath(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                using var reader = FileReaderFactory.CreateFileReader(filePath);
                var manifestReader = new AppxManifestReader();
                var pkg = await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);

                this.Color = pkg.Applications.Select(b => b.BackgroundColor).FirstOrDefault(b => b != null);
                this.DisplayName = pkg.DisplayName;
                this.PublisherDisplayName = pkg.PublisherDisplayName;
                this.Logo = pkg.Logo;
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
