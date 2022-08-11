using System;
using System.Linq;
using Otor.MsixHero.App.Converters;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Applications
{
    public class ApplicationViewModel : NotifyPropertyChanged
    {
        private readonly AppxApplication _model;
        private readonly AppxPackage _package;

        public ApplicationViewModel(AppxApplication appxApplication, AppxPackage package)
        {
            this._model = appxApplication;
            this._package = package;
            this.Type = PackageTypeConverter.GetPackageTypeFrom(this._model.EntryPoint, this._model.Executable, this._model.StartPage, this._package.IsFramework, this._model.HostId);
            this.Alias = this._model.ExecutionAlias?.Any() == true ? string.Join(", ", this._model.ExecutionAlias.Distinct(StringComparer.OrdinalIgnoreCase)) : null;
        }

        public bool Visible => this._model.Visible;

        public string Alias { get; }
        
        public string DisplayName => this._model.DisplayName;

        public byte[] Image => this._model.Logo;

        public string Id => this._model.Id;

        public string TileColor => string.Equals(this._model.BackgroundColor, "transparent", StringComparison.OrdinalIgnoreCase) ? null : this._model.BackgroundColor;

        public string Target
        {
            get
            {
                switch (PackageTypeConverter.GetPackageTypeFrom(this._model.EntryPoint, this._model.Executable, this._model.StartPage, this._package.IsFramework))
                {
                    case MsixPackageType.BridgeDirect:
                    case MsixPackageType.BridgePsf:
                        return this._model.Executable;
                    case MsixPackageType.Web:
                        return this._model.StartPage;
                    default:
                        if (string.IsNullOrEmpty(this._model.EntryPoint))
                        {
                            return this._model.Executable;
                        }

                        return this._model.Executable;
                }
            }
        }

        public string ActualTarget
        {
            get
            {
                switch (this.Type)
                {
                    case MsixPackageType.BridgeDirect:
                        return this.Target;

                    case MsixPackageType.BridgePsf:

                        if (this._model.Psf?.Arguments == null)
                        {
                            return this._model.Psf?.Executable ?? this._model.Executable;
                        }

                        return this._model.Psf.Executable + " " + this._model.Psf.Arguments;
                }

                return string.IsNullOrEmpty(this._model.EntryPoint) ? this._model.Executable : this._model.EntryPoint;
            }
        }

        public string OriginalTarget
        {
            get
            {
                switch (this.Type)
                {
                    case MsixPackageType.BridgePsf:
                        if (this._model.Psf?.Executable == null)
                        {
                            return null;
                        }

                        return this._model.Executable;
                }

                return null;
            }
        }

        public string WorkingDirectory
        {
            get
            {
                if (this._model.Psf == null)
                {
                    return null;
                }

                return this._model.Psf.WorkingDirectory;
            }
        }

        public string Arguments => this._model.Psf?.Arguments ?? this._model.Parameters;

        public string HostId => this._model.HostId;

        public string EntryPoint => this.Type == MsixPackageType.Uwp ? this._model.EntryPoint : null;

        public bool HasEntryPoint => this.Type == MsixPackageType.Uwp;
        
        public bool HasHostId => !string.IsNullOrEmpty(this.HostId);

        public bool HasOriginalTarget => !string.IsNullOrEmpty(this.OriginalTarget);

        public bool HasWorkingDirectory => !string.IsNullOrEmpty(this.WorkingDirectory);

        public bool HasArguments => !string.IsNullOrEmpty(this.Arguments);
        
        public bool HasAlias => !string.IsNullOrEmpty(this.Alias);

        public MsixPackageType Type { get; }

        public string DisplayedType => PackageTypeConverter.GetPackageTypeStringFrom(this._model.EntryPoint, this._model.Executable, this._model.StartPage, this._package.IsFramework, this._model.HostId, PackageTypeDisplay.Long);
    }
}
