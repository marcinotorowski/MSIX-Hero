using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements.Psf;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements
{
    public class AppxApplicationViewModel : NotifyPropertyChanged
    {
        private readonly AppxApplication model;
        private readonly AppxPackage package;

        public AppxApplicationViewModel(AppxApplication model, AppxPackage package)
        {
            this.model = model;
            this.package = package;
            this.Psf = model.Psf == null ? null : new AppxPsfViewModel(package.RootFolder, model.Psf);
            this.Services = model.Extensions == null ? null : new AppxServicesViewModel(model.Extensions);

            var type = PackageTypeConverter.GetPackageTypeFrom(this.model.EntryPoint, this.model.Executable, this.model.StartPage, this.package.IsFramework);
            switch (type)
            {
                case MsixPackageType.BridgePsf: 
                    // we adjust the information to not show PSF here, because we show it elsewhere.
                    this.Type = MsixPackageType.BridgeDirect;
                    this.DisplayType = PackageTypeConverter.GetPackageTypeStringFrom(MsixPackageType.BridgeDirect, true);
                    break;
                default:
                    this.Type = type;
                    this.DisplayType = PackageTypeConverter.GetPackageTypeStringFrom(type, true);
                    break;
            }
        }
        
        public bool Visible => this.model.Visible;

        public bool HasPsf => this.model.Psf != null;
        
        public AppxPsfViewModel Psf { get; }

        public AppxServicesViewModel Services { get; }

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

                        return this.model.Executable;
                }
            }
        }

        public string EntryPoint => this.Type == MsixPackageType.Uwp ? this.model.EntryPoint : null;

        public bool HasEntryPoint => this.Type == MsixPackageType.Uwp;

        public MsixPackageType Type { get; }

        public string DisplayType { get; }
    }
}
