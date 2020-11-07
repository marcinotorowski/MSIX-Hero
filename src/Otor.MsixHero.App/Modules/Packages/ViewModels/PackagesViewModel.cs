using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.Packages.ViewModels.Commands;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels
{
    public class PackagesViewModel : NotifyPropertyChanged
    {
        public PackagesViewModel(
            IMsixHeroApplication msixHeroApp,
            IInteractionService interactionService,
            IBusyManager busyManager)
        {
            this.CommandHandler = new PackageListCommandHandler(msixHeroApp, busyManager, interactionService);
        }

        public PackageListCommandHandler CommandHandler { get; }
    }
}
