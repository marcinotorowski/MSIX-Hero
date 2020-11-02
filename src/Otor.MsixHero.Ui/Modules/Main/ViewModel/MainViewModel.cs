using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.Updates;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands.Packages;
using Otor.MsixHero.Ui.Hero.Events;
using Otor.MsixHero.Ui.Hero.Events.Base;
using Otor.MsixHero.Ui.Hero.Executor;
using Otor.MsixHero.Ui.ViewModel;
using Prism.Events;
using Prism.Services.Dialogs;
using Path = System.IO.Path;

namespace Otor.MsixHero.Ui.Modules.Main.ViewModel
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IBusyManager busyManager;
        private readonly UpdateConfigurationManager updateConfigurationManager;
        private bool isLoading;
        private string loadingMessage;
        private int loadingProgress;
        private PackageContext context = PackageContext.CurrentUser;
        private bool isUpdateAvailable;
        
        public MainViewModel(
            IMsixHeroApplication application,
            IUpdateChecker updateChecker,
            IInteractionService interactionService,
            IConfigurationService configurationService, 
            IDialogService dialogService,
            IBusyManager busyManager)
        {
            this.updateConfigurationManager = new UpdateConfigurationManager(configurationService, updateChecker);
            this.application = application;
            this.interactionService = interactionService;
            this.configurationService = configurationService;
            this.dialogService = dialogService;
            this.busyManager = busyManager;
            this.Tools = new ObservableCollection<ToolViewModel>();

            busyManager.StatusChanged += this.BusyManagerOnStatusChanged;

            this.application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
            this.application.EventAggregator.GetEvent<UiFailedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilter);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SelectPackagesCommand>>().Subscribe(this.OnSelectPackages);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageGroupingCommand>>().Subscribe(this.OnSetPackageGrouping);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSortingCommand>>().Subscribe(this.OnSetPackageSorting);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSidebarVisibilityCommand>>().Subscribe(this.OnSetPackageSidebarVisibility);
            this.application.EventAggregator.GetEvent<ToolsChangedEvent>().Subscribe(this.OnToolsChangedEvent, ThreadOption.UIThread);
            this.SetTools();

            this.CheckReleaseNotes();
        }

        private async void CheckReleaseNotes()
        {
            var result = await this.updateConfigurationManager.ShouldShowReleaseNotes().ConfigureAwait(false);
            this.IsUpdateAvailable = result;
        }

        private void OnToolsChangedEvent(IReadOnlyCollection<ToolListConfiguration> toolsCollection)
        {
            this.SetTools(toolsCollection);
        }

        private void SetTools(IEnumerable<ToolListConfiguration> tools = null)
        {
            this.Tools.Clear();

            if (tools == null)
            {
                var config = this.configurationService.GetCurrentConfiguration();
                tools = config?.List?.Tools ?? Enumerable.Empty<ToolListConfiguration>();
            }

            foreach (var item in tools)
            {
                var itemName = item.Name;
                if (string.IsNullOrEmpty(itemName) && !string.IsNullOrEmpty(item.Path))
                {
                    itemName = Path.GetFileNameWithoutExtension(item.Path);
                }

                this.Tools.Add(new ToolViewModel(itemName, item));
            }
        }

        public MainCommandHandler CommandHandler => new MainCommandHandler(this.dialogService);

        public BackstageCommandHandler BackstageCommandHandler => new BackstageCommandHandler(this.dialogService, this.interactionService);

        public ObservableCollection<ToolViewModel> Tools { get; }
        
        public PackageContext Context
        {
            get => this.context;
            set
            {
                if (!this.SetField(ref this.context, value))
                {
                    return;
                }

                switch (value)
                {
                    case PackageContext.CurrentUser:
                        this.LoadContext(PackageFindMode.CurrentUser);
                        break;
                    case PackageContext.AllUsers:
                        this.LoadContext(PackageFindMode.AllUsers);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        private async void LoadContext(PackageFindMode mode)
        {
            var executor = this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.PackageLoading)
                .WithErrorHandling(this.interactionService, true);

            await executor.Invoke(this, new GetPackagesCommand(mode), CancellationToken.None).ConfigureAwait(false);
        }
        
        public bool HasSelection => this.application.ApplicationState.Packages.SelectedPackages.Any();

        public bool HasSingleSelection => this.application.ApplicationState.Packages.SelectedPackages.Count == 1;

        public bool IsUpdateAvailable
        {
            get => this.isUpdateAvailable;
            set => this.SetField(ref this.isUpdateAvailable, value);
        }

        public bool ShowSidebar
        {
            get => this.application.ApplicationState.Packages.ShowSidebar;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageSidebarVisibilityCommand(value));
        }

        public PackageGroup Group
        {
            get => this.application.ApplicationState.Packages.GroupMode;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageGroupingCommand(value));
        }

        public PackageFilter PackageFilter
        {
            get => this.application.ApplicationState.Packages.PackageFilter;
            set
            {
                var state = this.application.ApplicationState.Packages;
                this.application.CommandExecutor
                    .WithErrorHandling(this.interactionService, false)
                    .Invoke(this, new SetPackageFilterCommand(value, state.SearchKey));
            }
        }

        public bool ShowX64
        {
            get => (this.application.ApplicationState.Packages.PackageFilter & PackageFilter.x64) == PackageFilter.x64;
            set => this.SetPackageFilter(PackageFilter.x64, value);
        }

        public bool ShowX86
        {
            get => (this.application.ApplicationState.Packages.PackageFilter & PackageFilter.x86) == PackageFilter.x86;
            set => this.SetPackageFilter(PackageFilter.x86, value);
        }

        public bool ShowNeutral
        {
            get => (this.application.ApplicationState.Packages.PackageFilter & PackageFilter.Neutral) == PackageFilter.Neutral;
            set => this.SetPackageFilter(PackageFilter.Neutral, value);
        }

        public bool ShowArm
        {
            get => (this.application.ApplicationState.Packages.PackageFilter & PackageFilter.Arm) == PackageFilter.Arm;
            set => this.SetPackageFilter(PackageFilter.Arm, value);
        }

        public bool ShowArm64
        {
            get => (this.application.ApplicationState.Packages.PackageFilter & PackageFilter.Arm64) == PackageFilter.Arm64;
            set => this.SetPackageFilter(PackageFilter.Arm64, value);
        }

        private void SetPackageFilter(PackageFilter packageFilter, bool isSet)
        {
            var currentFilter = this.application.ApplicationState.Packages.PackageFilter;
            if (isSet)
            {
                currentFilter |= packageFilter;
            }
            else
            {
                currentFilter &= ~packageFilter;
            }

            var state = this.application.ApplicationState.Packages;
            this.application.CommandExecutor.Invoke(this, new SetPackageFilterCommand(currentFilter, state.SearchKey));
        }

        public bool ShowStoreApps
        {
            get => (this.application.ApplicationState.Packages.PackageFilter & PackageFilter.Store) == PackageFilter.Store;
            set => this.SetPackageFilter(PackageFilter.Store, value);
        }

        public bool ShowSideLoadedApps
        {
            get => (this.application.ApplicationState.Packages.PackageFilter & PackageFilter.Developer) == PackageFilter.Developer;
            set => this.SetPackageFilter(PackageFilter.Developer, value);
        }

        public bool ShowSystemApps
        {
            get => (this.application.ApplicationState.Packages.PackageFilter & PackageFilter.System) == PackageFilter.System;
            set => this.SetPackageFilter(PackageFilter.System, value);
        }

        public PackageFilter AddonsBehavior
        {
            get
            {
                var bitFlag = this.application.ApplicationState.Packages.PackageFilter & PackageFilter.MainAppsAndAddOns;
                return bitFlag == 0 ? PackageFilter.MainAppsAndAddOns : bitFlag;
            }
            set
            {
                var state = this.application.ApplicationState.Packages;
                var executor = this.application.CommandExecutor.WithErrorHandling(this.interactionService, false);

                var newValue = state.PackageFilter &= ~PackageFilter.MainAppsAndAddOns;
                newValue |= value & PackageFilter.MainAppsAndAddOns;
                executor.Invoke(this, new SetPackageFilterCommand(newValue, state.SearchKey));
            }
        }

        public PackageFilter RunningBehavior
        {
            get
            {
                var bitFlag = this.application.ApplicationState.Packages.PackageFilter & PackageFilter.InstalledAndRunning;
                return bitFlag == 0 ? PackageFilter.InstalledAndRunning : bitFlag;
            }
            set
            {
                var state = this.application.ApplicationState.Packages;
                var executor = this.application.CommandExecutor.WithErrorHandling(this.interactionService, false);

                var newValue = state.PackageFilter &= ~PackageFilter.InstalledAndRunning;
                newValue |= value & PackageFilter.InstalledAndRunning;
                executor.Invoke(this, new SetPackageFilterCommand(newValue, state.SearchKey));
            }
        }

        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.SetField(ref this.isLoading, value);
        }

        public int LoadingProgress
        {
            get => this.loadingProgress;
            private set => this.SetField(ref this.loadingProgress, value);
        }

        public string LoadingMessage
        {
            get => this.loadingMessage;
            private set => this.SetField(ref this.loadingMessage, value);
        }
        
        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type < 0)
            {
                return;
            }

            this.IsLoading = e.IsBusy;
            this.LoadingMessage = e.Message;
            this.LoadingProgress = e.Progress;
        }
        
        private void OnSetPackageGrouping(UiExecutedPayload<SetPackageGroupingCommand> obj)
        {
            this.OnPropertyChanged(nameof(Group));
        }

        private void OnSetPackageSorting(UiExecutedPayload<SetPackageSortingCommand> obj)
        {
        }

        private void OnSetPackageSidebarVisibility(UiExecutedPayload<SetPackageSidebarVisibilityCommand> obj)
        {
            this.OnPropertyChanged(nameof(ShowSidebar));
        }

        private void OnSetPackageFilter(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(ShowStoreApps));
            this.OnPropertyChanged(nameof(ShowSideLoadedApps));
            this.OnPropertyChanged(nameof(ShowSystemApps));
            this.OnPropertyChanged(nameof(AddonsBehavior));
            this.OnPropertyChanged(nameof(PackageFilter));
            this.OnPropertyChanged(nameof(ShowX64));
            this.OnPropertyChanged(nameof(ShowX86));
            this.OnPropertyChanged(nameof(ShowArm));
            this.OnPropertyChanged(nameof(ShowArm64));
            this.OnPropertyChanged(nameof(ShowNeutral));
        }

        private void OnSelectPackages(UiExecutedPayload<SelectPackagesCommand> obj)
        {
            this.OnPropertyChanged(nameof(HasSelection));
            this.OnPropertyChanged(nameof(HasSingleSelection));
        }

        private void OnGetPackages(UiFailedPayload<GetPackagesCommand> obj)
        {
            this.context = this.application.ApplicationState.Packages.Mode;
            this.OnPropertyChanged(nameof(Context));
        }

        private void OnGetPackages(UiExecutedPayload<GetPackagesCommand> obj)
        {
            this.context = this.application.ApplicationState.Packages.Mode;
            this.OnPropertyChanged(nameof(Context));
        }
    }
}
