using System.Threading;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels
{
    public class PackagesSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;
        private bool isAllUsers;

        public PackagesSearchViewModel(
            IEventAggregator eventAggregator,
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.application = application;
            this.busyManager = busyManager;
            this.interactionService = interactionService;
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilterCommand);
            this.isAllUsers = application.ApplicationState.Packages.Mode == PackageContext.AllUsers;

            eventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
            eventAggregator.GetEvent<UiFailedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
            eventAggregator.GetEvent<UiCancelledEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
        }

        private void OnSetPackageFilterCommand(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
        }


        public string SearchKey
        {
            get => this.application.ApplicationState.Packages.SearchKey;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageFilterCommand(this.application.CommandExecutor.ApplicationState.Packages.Filter, value));
        }

        public bool IsAllUsers
        {
            get => this.isAllUsers;
            set
            {
                if (!this.SetField(ref this.isAllUsers, value))
                {
                    return;
                }

                this.LoadContext(value ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser);
            }
        }


        private void OnGetPackages(UiFailedPayload<GetPackagesCommand> obj)
        {
            this.isAllUsers = this.application.ApplicationState.Packages.Mode == PackageContext.AllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));
        }

        private void OnGetPackages(UiExecutedPayload<GetPackagesCommand> obj)
        {
            this.isAllUsers = this.application.ApplicationState.Packages.Mode == PackageContext.AllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));
        }

        private void OnGetPackages(UiCancelledPayload<GetPackagesCommand> obj)
        {
            this.isAllUsers = this.application.ApplicationState.Packages.Mode == PackageContext.AllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));
        }

        private async void LoadContext(PackageFindMode mode)
        {
            var executor = this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.PackageLoading)
                .WithErrorHandling(this.interactionService, true);

            await executor.Invoke(this, new GetPackagesCommand(mode), CancellationToken.None).ConfigureAwait(false);
        }
    }
}
