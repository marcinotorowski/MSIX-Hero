// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.ViewModel.Items;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Dialogs;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.ViewModel
{
    public class ChangeVolumeViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IMsixHeroApplication _application;
        private readonly IInteractionService _interactionService;
        private readonly IModuleManager _moduleManager;
        private readonly IDialogService _dialogService;
        private readonly IUacElevation _uacElevation;
        private string _packageInstallLocation;

        public ChangeVolumeViewModel(IMsixHeroApplication application,
            IInteractionService interactionService, 
            IModuleManager moduleManager,
            IDialogService dialogService,
            IUacElevation uacElevation) : base(Resources.Localization.Dialogs_ChangeVolume_Title, interactionService)
        {
            this._application = application;
            this._interactionService = interactionService;
            this._moduleManager = moduleManager;
            this._dialogService = dialogService;
            this._uacElevation = uacElevation;

            // This can be longer…
            var taskForFreeLetters = this.GetAllVolumes();

            taskForFreeLetters.ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                {
                    return;
                }

                var dr = t.Result.LastOrDefault(d => d.Model?.DiskLabel != null);
                if (dr != null)
                {
                    this.TargetVolume.CurrentValue = dr.Model;
                    this.TargetVolume.Commit();
                }

#pragma warning disable 4014
                this.AllVolumes.Load(Task.FromResult(t.Result));
#pragma warning restore 4014
            });

            this.AllVolumes = new AsyncProperty<List<VolumeCandidateViewModel>>();
            this.TargetVolume = new ChangeableProperty<AppxVolume>();
            this.CurrentVolume = new AsyncProperty<VolumeCandidateViewModel>();

            this.AddChildren(this.TargetVolume);
        }
        
        public AsyncProperty<List<VolumeCandidateViewModel>> AllVolumes { get; }

        public AsyncProperty<VolumeCandidateViewModel> CurrentVolume { get; }

        public ChangeableProperty<AppxVolume> TargetVolume { get; }

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

            if (string.Equals(this.CurrentVolume.CurrentValue?.Name, this.TargetVolume.CurrentValue?.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (this.AllVolumes.CurrentValue.Count - 1 == 1)
                {
                    var buttons = new List<string>
                    {
                        Resources.Localization.Dialogs_ChangeVolume_PackageAlreadyOnVolume_NewVolume,
                        Resources.Localization.Button_Cancel
                    };

                    var option = this._interactionService.ShowMessage(Resources.Localization.Dialogs_ChangeVolume_PackageAlreadyOnVolume, buttons);

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
                        string.Format(Resources.Localization.Dialogs_ChangeVolume_PackageAlreadyOnVolume_Use_Format, suggestion?.PackageStorePath),
                        Resources.Localization.Dialogs_ChangeVolume_PackageAlreadyOnVolume_NewVolume,
                        Resources.Localization.Button_Cancel
                    };

                    var option = this._interactionService.ShowMessage(Resources.Localization.Dialogs_ChangeVolume_PackageAlreadyOnVolume_Choice, buttons);

                    if (option == 1)
                    {
                        this.CreateNew();
                        return false;
                    }
                    else if (option == 2)
                    {
                        return false;
                    }

                    this.TargetVolume.CurrentValue = suggestion?.Model;
                }
                else
                {
                    var buttons = new List<string>
                    {
                        Resources.Localization.Dialogs_ChangeVolume_PackageAlreadyOnVolume_Select,
                        Resources.Localization.Dialogs_ChangeVolume_PackageAlreadyOnVolume_NewVolume,
                        Resources.Localization.Button_Cancel
                    };

                    var option = this._interactionService.ShowMessage(Resources.Localization.Dialogs_ChangeVolume_PackageAlreadyOnVolume_Choice, buttons);

                    if (option == 1)
                    {
                        this.CreateNew();
                    }
                    
                    return false;
                }
            }

            progress.Report(new ProgressData(20, Resources.Localization.Dialogs_ChangeVolume_Moving));
            cancellationToken.ThrowIfCancellationRequested();

            var id = Path.GetFileName(this._packageInstallLocation);

            await this._uacElevation.AsAdministrator<IAppxVolumeService>().MovePackageToVolume(this.TargetVolume.CurrentValue, id, cancellationToken, progress).ConfigureAwait(false);

            progress.Report(new ProgressData(100, Resources.Localization.Dialogs_ChangeVolume_ReadingPackages));

            await this._application.CommandExecutor.Invoke<GetVolumesCommand, IList<AppxVolume>>(this, new GetVolumesCommand(), cancellationToken).ConfigureAwait(false);

            return true;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var param = parameters.Count > 0 ? parameters.GetValue<string>(parameters.Keys.First()) : null;
            this._packageInstallLocation = param;

#pragma warning disable 4014
            this.CurrentVolume.Load(this.GetCurrentVolume(param));
#pragma warning restore 4014
        }

        private async Task<List<VolumeCandidateViewModel>> GetAllVolumes()
        {
            var mgr = this._uacElevation.AsCurrentUser<IAppxVolumeService>();
            var disks = await mgr.GetAll().ConfigureAwait(false);
            return disks.Where(d => !d.IsOffline).Select(r => new VolumeCandidateViewModel(r)).Concat(new[] { new VolumeCandidateViewModel(new AppxVolume()) }).ToList();
        }

        private async Task<VolumeCandidateViewModel> GetCurrentVolume(string packagePath)
        {
            var mgr = this._uacElevation.AsCurrentUser<IAppxVolumeService>();
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

            this._moduleManager.LoadModule(ModuleNames.Dialogs.Volumes);
           
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.VolumesNewVolume, new DialogParameters(), result =>
            {
                if (result.Result == ButtonResult.Cancel)
                {
                    return;
                }

                var t1 = this.GetAllVolumes();
                this.AllVolumes.Load(t1).GetAwaiter().GetResult();

                var allVolumes = this.AllVolumes.CurrentValue;
                var newVolume = allVolumes.FirstOrDefault(d => !current.Contains(d.PackageStorePath)) ?? allVolumes.FirstOrDefault();
                this.TargetVolume.CurrentValue = newVolume?.Model;
            });
        }
    }
}
