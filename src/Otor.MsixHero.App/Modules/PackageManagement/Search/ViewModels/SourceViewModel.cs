using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Services;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels
{
    public class SourceViewModel(PackagesSearchViewModel parent, PackageQuerySource sourceType, string displayName = null) : NotifyPropertyChanged
    {
        private PackageQuerySource _sourceType = sourceType;
        
        private string _displayName = displayName ?? ConvertToDisplayName(sourceType);
        private string _displayPath = ConvertToDisplayPath(sourceType);
        
        public PackageQuerySource SourceType
        {
            get => _sourceType;
            internal set
            {
                if (!this.SetField(ref this._sourceType, value))
                {
                    return;
                }

                this.DisplayName = ConvertToDisplayName(value);
                this.DisplayPath = ConvertToDisplayPath(value);
            }
        }

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

        public string DisplayPath
        {
            get => _displayPath;
            private set
            {
                if (!this.SetField(ref this._displayPath, value))
                {
                    return;
                }
            }
        }

        public bool IsSelected
        {
            get => parent.SelectedSource == this;
            set
            {
                var wasSelected = parent.SelectedSource == this;

                if (value)
                {
                    parent.SelectedSource = this;
                }

                if (!wasSelected && value || wasSelected && !value)
                {
                    this.OnPropertyChanged();
                }
            }
        }

        private static string ConvertToDisplayPath(PackageQuerySource mode)
        {
            if (mode.Path == null || mode.Type != PackageQuerySourceType.Directory)
            {
                return mode.Path;
            }

            if (mode.Path.EndsWith("/*") || mode.Path.EndsWith("\\*"))
            {
                return System.IO.Path.GetDirectoryName(mode.Path);
            }

            return mode.Path;
        }

        private static string ConvertToDisplayName(PackageQuerySource mode)
        {
            if (mode.Path == null || mode.Type != PackageQuerySourceType.Directory)
            {
                return null;
            }

            if (mode.Path.EndsWith("/*") || mode.Path.EndsWith("\\*"))
            {
                return System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(mode.Path));
            }

            return System.IO.Path.GetFileName(mode.Path);
        }
    }
}
