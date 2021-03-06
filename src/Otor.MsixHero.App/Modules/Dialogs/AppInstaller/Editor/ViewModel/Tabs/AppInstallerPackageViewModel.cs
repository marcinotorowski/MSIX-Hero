using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.AppInstaller.Entities;

namespace Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.ViewModel.Tabs
{
    public class AppInstallerPackageViewModel : AppInstallerBasePackageViewModel
    {
        public AppInstallerPackageViewModel()
        {
            this.Architecture = new ChangeableProperty<AppInstallerPackageArchitecture>(AppInstallerPackageArchitecture.neutral);
            this.AddChildren(this.Architecture);
        }

        public AppInstallerPackageViewModel(AppInstallerPackageEntry baseEntry) : base(baseEntry)
        {
            this.Architecture = new ChangeableProperty<AppInstallerPackageArchitecture>(baseEntry.Architecture);
            this.AddChildren(this.Architecture);
        }

        public override AppInstallerBasePackageViewModelType Type => AppInstallerBasePackageViewModelType.Package;

        public override AppInstallerBaseEntry ToModel()
        {
            return new AppInstallerPackageEntry
            {
                Name = this.Name.CurrentValue,
                Publisher = this.Publisher.CurrentValue,
                Version = this.Version.CurrentValue,
                Architecture = this.Architecture.CurrentValue,
                Uri = this.Uri.CurrentValue
            };
        }
    }
}