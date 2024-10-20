using JetBrains.Annotations;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Services;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels
{
    public class SourceViewModel(PackageQuerySource sourceType, [CanBeNull] string displayName = null) : NotifyPropertyChanged
    {
        private PackageQuerySource _sourceType = sourceType;
        [CanBeNull] private string _displayName = displayName;

        public PackageQuerySource SourceType
        {
            get => _sourceType;
            internal set
            {
                if (!this.SetField(ref this._sourceType, value))
                {
                    return;
                }

                if (value.Type == PackageQuerySourceType.Directory)
                {
                    this.DisplayName = System.IO.Path.GetFileName(value.Path);
                }
                else
                {
                    this.DisplayName = null;
                }
            }
        }

        [CanBeNull]
        public string DisplayName
        {
            get => _displayName;
            private set
            {
                if (!this.SetField(ref this._displayName, value))
                {
                    return;
                }
            }
        }
    }
}
