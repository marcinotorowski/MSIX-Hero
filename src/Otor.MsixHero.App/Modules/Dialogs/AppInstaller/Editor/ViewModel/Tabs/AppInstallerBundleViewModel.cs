using Otor.MsixHero.AppInstaller.Entities;

namespace Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.ViewModel.Tabs
{
    public class AppInstallerBundleViewModel : AppInstallerBasePackageViewModel
    {
        public AppInstallerBundleViewModel()
        {
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public AppInstallerBundleViewModel(AppInstallerBundleEntry baseEntry) : base(baseEntry)
        {
        }

        public override AppInstallerBasePackageViewModelType Type => AppInstallerBasePackageViewModelType.Bundle;

        public override AppInstallerBaseEntry ToModel()
        {
            return new AppInstallerBundleEntry
            {
                Name = this.Name.CurrentValue,
                Publisher = this.Publisher.CurrentValue,
                Version = this.Version.CurrentValue,
                Uri = this.Uri.CurrentValue
            };
        }
    }
}