using System;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class AppxApplicationViewModel : NotifyPropertyChanged
    {
        private readonly AppxApplication model;
        
        public AppxApplicationViewModel(AppxApplication model)
        {
            this.model = model;
            this.Psf = model.Psf == null ? null : new AppxPsfViewModel(model.Psf);
        }

        public bool HasPsf => this.model.Psf != null;
        
        public AppxPsfViewModel Psf { get; }

        public string DisplayName => this.model.DisplayName;

        public string Image
        {
            get
            {

                if (this.model.Square30x30Logo != null)
                {
                    return this.model.Square30x30Logo;
                }

                if (this.model.SmallLogo != null)
                {
                    return this.model.SmallLogo;
                }

                if (this.model.Logo != null)
                {
                    return this.model.Logo;
                }

                return null;
            }
        }

        public string TileColor => this.model.BackgroundColor;

        public string Start
        {
            get
            {
                switch (PackageTypeConverter.GetPackageTypeFrom(this.model.EntryPoint, this.model.Executable, this.model.StartPage))
                {
                    case PackageType.Uwp:
                        if (string.IsNullOrEmpty(this.model.EntryPoint))
                        {
                            return this.model.Executable;
                        }

                        return this.model.Executable + " > " + this.model.EntryPoint;
                    case PackageType.BridgeDirect:
                    case PackageType.BridgePsf:
                        return this.model.Executable;
                    case PackageType.Web:
                        return this.model.StartPage;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public PackageType Type => PackageTypeConverter.GetPackageTypeFrom(this.model.EntryPoint, this.model.Executable, this.model.StartPage);

        public string DisplayType => PackageTypeConverter.GetPackageTypeStringFrom(this.model.EntryPoint, this.model.Executable, this.model.StartPage);
    }
}
