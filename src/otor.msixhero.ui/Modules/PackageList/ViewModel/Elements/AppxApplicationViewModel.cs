using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel.Elements
{
    public class AppxApplicationViewModel : NotifyPropertyChanged
    {
        private readonly AppxApplication model;
        private readonly AppxPackage package;

        public AppxApplicationViewModel(AppxApplication model, AppxPackage package)
        {
            this.model = model;
            this.package = package;
            this.Psf = model.Psf == null ? null : new AppxPsfViewModel(model.Psf);
        }

        public bool HasPsf => this.model.Psf != null;
        
        public AppxPsfViewModel Psf { get; }

        public string DisplayName => this.model.DisplayName;

        public byte[] Image => this.model.Logo;

        public string TileColor => this.model.BackgroundColor;

        public string Start
        {
            get
            {
                switch (PackageTypeConverter.GetPackageTypeFrom(this.model.EntryPoint, this.model.Executable, this.model.StartPage, this.package.IsFramework))
                {
                    case MsixPackageType.BridgeDirect:
                    case MsixPackageType.BridgePsf:
                        return this.model.Executable;
                    case MsixPackageType.Web:
                        return this.model.StartPage;
                    case MsixPackageType.Uwp:
                    default:
                        if (string.IsNullOrEmpty(this.model.EntryPoint))
                        {
                            return this.model.Executable;
                        }

                        return this.model.Executable + " > " + this.model.EntryPoint;
                }
            }
        }
        
        public MsixPackageType Type => PackageTypeConverter.GetPackageTypeFrom(this.model.EntryPoint, this.model.Executable, this.model.StartPage, this.package.IsFramework);

        public string DisplayType => PackageTypeConverter.GetPackageTypeStringFrom(this.model.EntryPoint, this.model.Executable, this.model.StartPage, this.package.IsFramework);
    }
}
