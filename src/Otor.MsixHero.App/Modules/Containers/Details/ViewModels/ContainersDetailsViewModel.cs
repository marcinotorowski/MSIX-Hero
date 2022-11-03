// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.Containers.Details.ViewModels.Items;
using Otor.MsixHero.App.Modules.Containers.List.ViewModels;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Containers.Details.ViewModels
{
    public class ContainersDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IAppxPackageQueryService _appQueryService;
        private readonly PrismServices _prismServices;
        private readonly IInteractionService _interactionService;
        private SharedPackageContainerViewModel _selectedContainer;

        public ContainersDetailsViewModel(
            IAppxPackageQueryService appQueryService,
            PrismServices prismServices,
            IEventAggregator eventAggregator,
            IInteractionService interactionService)
        {
            this._appQueryService = appQueryService;
            this._prismServices = prismServices;
            this._interactionService = interactionService;

            this.OpenPackage = new DelegateCommand<object>(this.OnOpen, this.CanOpen);
            eventAggregator.GetEvent<UiExecutedEvent<GetSharedPackageContainersCommand, IList<SharedPackageContainer>>>().Subscribe(this.OnGetSharedPackageContainers, ThreadOption.UIThread);
        }

        private void OnOpen(object commandParameter)
        {
            if (commandParameter is not string familyName)
            {
                return;
            }

            try
            {
                var pkg = _appQueryService.GetInstalledPackageByFamilyName(familyName).GetAwaiter().GetResult();

                var dialogOpener = new DialogOpener(this._prismServices, this._interactionService);
                dialogOpener.OpenMsix(new FileInfo(pkg.ManifestPath));
            }
            catch (Exception e)
            {
                this._interactionService.ShowError(Resources.Localization.Containers_Error_PackageOpen + " " + e.GetBaseException().Message);
            }
        }

        private bool CanOpen(object commandParameter)
        {
            return commandParameter is string familyName && !string.IsNullOrWhiteSpace(familyName);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var newSelection = GetSharedAppContainerFromRegionContext(navigationContext);

            if (newSelection == null)
            {
                this.SelectedContainer = null;
            }
            else if (this.SelectedContainer?.Model != newSelection)
            {
                this.SelectedContainer = new SharedPackageContainerViewModel(newSelection);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public SharedPackageContainerViewModel SelectedContainer
        {
            get => this._selectedContainer;
            private set
            {
                if (!this.SetField(ref this._selectedContainer, value))
                {
                    return;
                }

                var _ = this.Packages.Load(this.GetPackages(value.Model));
            }
        }

        private void OnGetSharedPackageContainers(UiExecutedPayload<GetSharedPackageContainersCommand, IList<SharedPackageContainer>> obj)
        {
            if (this.SelectedContainer == null || obj.Result.All(r => r.Name != this.SelectedContainer.Name))
            {
                return;
            }

            this.SelectedContainer = new SharedPackageContainerViewModel(obj.Result.FirstOrDefault(r => r.Name == this.SelectedContainer.Name));
        }

        private async Task<ObservableCollection<ContainerContentViewModel>> GetPackages(SharedPackageContainer valueModel)
        {
            var col = new List<Task<ContainerContentViewModel>>(valueModel.PackageFamilies.Count);

            const int maxTasksAtOnce = 5;

            var progressMonitoring = new Progress();

            // ReSharper disable once ConvertToLocalFunction
            EventHandler<ProgressData> handler = (_, data) =>
            {
                this.Packages.Progress.Message = data.Message;
                this.Packages.Progress.Progress = data.Progress;
            };
            
            progressMonitoring.ProgressChanged += handler;

            try
            {
                progressMonitoring.ProgressChanged += handler;

                using var progress = new WrappedProgress(progressMonitoring);

                var progressObjects = new Dictionary<string, IProgress<ProgressData>>(StringComparer.OrdinalIgnoreCase);
                foreach (var familyName in valueModel.PackageFamilies.Select(p => p.FamilyName))
                {
                    progressObjects[familyName] = progress.GetChildProgress();
                }

                foreach (var familyName in valueModel.PackageFamilies.Select(p => p.FamilyName))
                {
                    while (col.Count >= maxTasksAtOnce)
                    {
                        var finished = await Task.WhenAny(col).ConfigureAwait(false);
                        col.Remove(finished);
                    }

                    var singleProgress = progressObjects[familyName];
                    col.Add(this.GetContainerContentFromFamilyName(familyName, singleProgress));
                }

                await Task.WhenAll(col).ConfigureAwait(false);
                return new ObservableCollection<ContainerContentViewModel>(col.Select(t => t.Result));
            }
            finally
            {
                progressMonitoring.ProgressChanged -= handler;
            }
        }

        private async Task<ContainerContentViewModel> GetContainerContentFromFamilyName(string familyName, IProgress<ProgressData> progress)
        {
            try
            {
                progress.Report(new ProgressData(0, Resources.Localization.Containers_GettingApps));

                var packageEntry = await this._appQueryService.GetInstalledPackageByFamilyName(familyName).ConfigureAwait(false);
                if (packageEntry == null)
                {
                    return new FamilyNameViewModel(familyName);
                }

                return new PackageContainerContentViewModel(packageEntry);
            }
            finally
            {
                progress.Report(new ProgressData(100, Resources.Localization.Containers_GettingApps));
            }
        }

        public ICommand OpenPackage { get; }

        public AsyncProperty<ObservableCollection<ContainerContentViewModel>> Packages { get; } = new();

        private static SharedPackageContainer GetSharedAppContainerFromRegionContext(NavigationContext context)
        {
            var key = context.Parameters.Keys.FirstOrDefault(k => context.Parameters[k] is SharedPackageContainer);
            if (key == null)
            {
                return null;
            }

            return (SharedPackageContainer) context.Parameters[key];
        }
    }
}

