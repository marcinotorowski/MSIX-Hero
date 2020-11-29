using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels
{
    public enum ClearFilter
    {
        Architecture,
        Activity,
        Category,
        Type
    }

    public class PackageFilterSortViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;

        public PackageFilterSortViewModel(IMsixHeroApplication application, IInteractionService interactionService)
        {
            this.application = application;
            this.interactionService = interactionService;

            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilter);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSortingCommand>>().Subscribe(this.OnSetPackageSorting);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageGroupingCommand>>().Subscribe(this.OnSetPackageGrouping);

            this.Clear = new DelegateCommand<object>(this.OnClearFilter);
        }

        public ICommand Clear { get; }

        public bool IsDescending
        {
            get => this.application.ApplicationState.Packages.SortDescending;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageSortingCommand(this.Sort, value));
        }

        public PackageSort Sort
        {
            get => this.application.ApplicationState.Packages.SortMode;
            set
            {
                this.application.CommandExecutor.Invoke(this, new SetPackageSortingCommand(value, this.IsDescending));
            }
        }

        public PackageGroup Group
        {
            get => this.application.ApplicationState.Packages.GroupMode;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageGroupingCommand(value));
        }

        public bool FilterStore
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Store);
            set => this.SetPackageFilter(PackageFilter.Store, value);
        }

        public bool FilterAddOn
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Addons);
            set => this.SetPackageFilter(PackageFilter.Addons, value);
        }

        public bool FilterMainApp
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.MainApps);
            set => this.SetPackageFilter(PackageFilter.MainApps, value);
        }

        public bool FilterSideLoaded
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Developer);
            set => this.SetPackageFilter(PackageFilter.Developer, value);
        }

        public bool FilterSystem
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.System);
            set => this.SetPackageFilter(PackageFilter.System, value);
        }

        public bool FilterX64
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.x64);
            set => this.SetPackageFilter(PackageFilter.x64, value);
        }

        public bool FilterX86
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.x86);
            set => this.SetPackageFilter(PackageFilter.x86, value);
        }

        public bool FilterArm
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Arm);
            set => this.SetPackageFilter(PackageFilter.Arm, value);
        }

        public bool FilterArm64
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Arm64);
            set => this.SetPackageFilter(PackageFilter.Arm64, value);
        }

        public bool FilterNeutral
        {
            get => this.application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Neutral);
            set => this.SetPackageFilter(PackageFilter.Neutral, value);
        }

        public string FilterCategoryCaption
        {
            get
            {
                var val = this.application.ApplicationState.Packages.Filter & PackageFilter.AllSources;
                if (val == 0 || val == PackageFilter.AllSources)
                {
                    return "(all)";
                }

                var selected = 0;
                if (this.FilterSideLoaded)
                {
                    selected++;
                }

                if (this.FilterStore)
                {
                    selected++;
                }

                if (this.FilterSystem)
                {
                    selected++;
                }

                return $"({selected}/3)";
            }
        }

        public string FilterTypeCaption
        {
            get
            {
                var val = this.application.ApplicationState.Packages.Filter & PackageFilter.MainAppsAndAddOns;
                if (val == 0 || val == PackageFilter.MainAppsAndAddOns)
                {
                    return "(all)";
                }

                var selected = 0;
                if (this.FilterMainApp)
                {
                    selected++;
                }

                if (this.FilterAddOn)
                {
                    selected++;
                }
                
                return $"({selected}/2)";
            }
        }

        public string FilterArchitectureCaption
        {
            get
            {
                var val = this.application.ApplicationState.Packages.Filter & PackageFilter.AllArchitectures;
                if (val == 0 || val == PackageFilter.AllArchitectures)
                {
                    return "(all)";
                }

                var selected = 0;
                if (this.FilterX64)
                {
                    selected++;
                }

                if (this.FilterX86)
                {
                    selected++;
                }

                if (this.FilterArm)
                {
                    selected++;
                }

                if (this.FilterArm64)
                {
                    selected++;
                }

                if (this.FilterNeutral)
                {
                    selected++;
                }

                return $"({selected}/5)";
            }
        }


        public bool FilterRunning
        {
            get => (this.application.ApplicationState.Packages.Filter & PackageFilter.InstalledAndRunning) == PackageFilter.Running;
            set
            {
                var newValue = this.application.ApplicationState.Packages.Filter & ~PackageFilter.InstalledAndRunning;
                if (value)
                {
                    newValue |= PackageFilter.Running;
                }
                else
                {
                    newValue |= PackageFilter.InstalledAndRunning;
                }

                this.SetPackageFilter(newValue);
            }
        }

        private void OnSetPackageSorting(UiExecutedPayload<SetPackageSortingCommand> obj)
        {
            this.OnPropertyChanged(nameof(IsDescending));
            this.OnPropertyChanged(nameof(Sort));
        }

        private void OnSetPackageGrouping(UiExecutedPayload<SetPackageGroupingCommand> obj)
        {
            this.OnPropertyChanged(nameof(Group));
        }

        private void OnSetPackageFilter(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(FilterSystem));
            this.OnPropertyChanged(nameof(FilterSideLoaded));
            this.OnPropertyChanged(nameof(FilterStore));

            this.OnPropertyChanged(nameof(FilterRunning));

            this.OnPropertyChanged(nameof(FilterX64));
            this.OnPropertyChanged(nameof(FilterX86));
            this.OnPropertyChanged(nameof(FilterArm));
            this.OnPropertyChanged(nameof(FilterArm64));
            this.OnPropertyChanged(nameof(FilterNeutral));

            this.OnPropertyChanged(nameof(FilterMainApp));
            this.OnPropertyChanged(nameof(FilterAddOn));

            this.OnPropertyChanged(nameof(FilterCategoryCaption));
            this.OnPropertyChanged(nameof(FilterArchitectureCaption));
            this.OnPropertyChanged(nameof(FilterTypeCaption));
        }

        private void SetPackageFilter(PackageFilter packageFilter, bool isSet)
        {
            var currentFilter = this.application.ApplicationState.Packages.Filter;
            if (isSet)
            {
                currentFilter |= packageFilter;
            }
            else
            {
                currentFilter &= ~packageFilter;
            }

            this.SetPackageFilter(currentFilter);
        }

        private void SetPackageFilter(PackageFilter packageFilter)
        {
            var state = this.application.ApplicationState.Packages;
            this.application.CommandExecutor.WithErrorHandling(this.interactionService, false).Invoke(this, new SetPackageFilterCommand(packageFilter, state.SearchKey));
        }
        
        private void OnClearFilter(object objectFilterToClear)
        {
            if (!(objectFilterToClear is ClearFilter filterToClear))
            {
                return;
            }

            switch (filterToClear)
            {
                case ClearFilter.Architecture:
                    this.SetPackageFilter(PackageFilter.AllArchitectures, true);
                    break;
                case ClearFilter.Activity:
                    this.SetPackageFilter(PackageFilter.InstalledAndRunning, true);
                    break;
                case ClearFilter.Category:
                    this.SetPackageFilter(PackageFilter.AllSources, true);
                    break;
                case ClearFilter.Type:
                    this.SetPackageFilter(PackageFilter.MainAppsAndAddOns, true);
                    break;
            }
        }
    }
}
