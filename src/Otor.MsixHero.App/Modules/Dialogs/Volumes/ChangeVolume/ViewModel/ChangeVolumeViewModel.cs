// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.ViewModel.Items;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.ViewModel
{
    public class ChangeVolumeViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly IModuleManager moduleManager;
        private readonly IDialogService dialogService;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerFactory;
        private string packageInstallLocation;

        public ChangeVolumeViewModel(
            IMsixHeroApplication application,
            IInteractionService interactionService, 
            IModuleManager moduleManager,
            IDialogService dialogService,
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerFactory) : base("Change volume", interactionService)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.moduleManager = moduleManager;
            this.dialogService = dialogService;
            this.volumeManagerFactory = volumeManagerFactory;

            // This can be longer...
            var taskForFreeLetters = this.GetAllVolumes();

            taskForFreeLetters.ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                {
                    return;
                }

                var dr = t.Result.FirstOrDefault();
                if (dr != null)
                {
                    this.TargetVolume.CurrentValue = dr.Name;
                    this.TargetVolume.Commit();
                }

#pragma warning disable 4014
                this.AllVolumes.Load(Task.FromResult(t.Result));
#pragma warning restore 4014
            });

            this.AllVolumes = new AsyncProperty<List<VolumeCandidateViewModel>>();
            this.TargetVolume = new ChangeableProperty<string>();
            this.CurrentVolume = new AsyncProperty<VolumeCandidateViewModel>();

            this.AddChildren(this.TargetVolume);
        }
        
        public AsyncProperty<List<VolumeCandidateViewModel>> AllVolumes { get; }

        public AsyncProperty<VolumeCandidateViewModel> CurrentVolume { get; }

        public ChangeableProperty<string> TargetVolume { get; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.CurrentVolume.HasValue)
            {
                return false;
            }

            if (!this.AllVolumes.HasValue)
            {
                return false;
            }

            if (string.Equals(this.CurrentVolume.CurrentValue?.Name, this.TargetVolume.CurrentValue, StringComparison.OrdinalIgnoreCase))
            {
                if (this.AllVolumes.CurrentValue.Count - 1 == 1)
                {
                    var buttons = new List<string>
                    {
                        "Create a new volume",
                        "Go back"
                    };

                    var option = this.interactionService.ShowMessage("The selected package is already available on the required volume. Currently, there is only a single volume available, did you mean to create a new one first?", buttons);

                    if (option == 0)
                    {
                        this.CreateNew();
                    }

                    return false;
                }

                if (this.AllVolumes.CurrentValue.Count - 1 == 2)
                {
                    var suggestion = this.AllVolumes.CurrentValue.FirstOrDefault(v => v?.Name != this.CurrentVolume.CurrentValue?.Name);
                    var buttons = new List<string>
                    {
                        "Use " + suggestion?.PackageStorePath,
                        "Create a new volume",
                        "Cancel"
                    };

                    var option = this.interactionService.ShowMessage("The selected package is already available on the required volume. Did you mean another volume?", buttons);

                    if (option == 1)
                    {
                        this.CreateNew();
                        return false;
                    }
                    else if (option == 2)
                    {
                        return false;
                    }

                    this.TargetVolume.CurrentValue = suggestion?.Name;
                }
                else
                {
                    var buttons = new List<string>
                    {
                        "Select another volume",
                        "Create a new volume",
                        "Cancel"
                    };

                    var option = this.interactionService.ShowMessage("The selected package is already available on the required volume. Did you mean another volume?", buttons);

                    if (option == 1)
                    {
                        this.CreateNew();
                    }
                    
                    return false;
                }
            }

            progress.Report(new ProgressData(20, "Moving to the selected volume..."));
            var mgr = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var id = Path.GetFileName(this.packageInstallLocation);
            await mgr.MovePackageToVolume(this.TargetVolume.CurrentValue, id, cancellationToken, progress).ConfigureAwait(false);

            progress.Report(new ProgressData(100, "Reading packages..."));

            await this.application.CommandExecutor.Invoke(this, new GetVolumesCommand(), cancellationToken).ConfigureAwait(false);

            return true;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var param = parameters.Count > 0 ? parameters.GetValue<string>(parameters.Keys.First()) : null;
            this.packageInstallLocation = param;

#pragma warning disable 4014
            this.CurrentVolume.Load(this.GetCurrentVolume(param));
#pragma warning restore 4014
        }

        private async Task<List<VolumeCandidateViewModel>> GetAllVolumes()
        {
            var mgr = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker).ConfigureAwait(false);
            var disks = await mgr.GetAll().ConfigureAwait(false);
            return disks.Where(d => !d.IsOffline).Select(r => new VolumeCandidateViewModel(r)).Concat(new[] { new VolumeCandidateViewModel(new AppxVolume()) }).ToList();
        }

        private async Task<VolumeCandidateViewModel> GetCurrentVolume(string packagePath)
        {
            var mgr = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker).ConfigureAwait(false);
            var disk = await mgr.GetVolumeForPath(packagePath).ConfigureAwait(false);
            if (disk == null)
            {
                return null;
            }

            return new VolumeCandidateViewModel(disk);
        }

        public void CreateNew()
        {
            var current = this.AllVolumes.CurrentValue.Select(c => c.PackageStorePath).ToList();

            this.moduleManager.LoadModule(ModuleNames.Dialogs.Volumes);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.VolumesNewVolume, new DialogParameters(), async result =>
            {
                if (result.Result == ButtonResult.Cancel)
                {
                    return;
                }

                var t1 = this.GetAllVolumes();
                await this.AllVolumes.Load(t1).ConfigureAwait(false);

                var allVolumes = this.AllVolumes.CurrentValue;
                var newVolume = allVolumes.FirstOrDefault(d => !current.Contains(d.PackageStorePath)) ?? allVolumes.FirstOrDefault();
                this.TargetVolume.CurrentValue = newVolume?.Name;
            });
        }
    }
}
