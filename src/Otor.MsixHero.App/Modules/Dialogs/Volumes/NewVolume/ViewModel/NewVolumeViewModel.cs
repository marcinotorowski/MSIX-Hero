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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.NewVolume.ViewModel.Items;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.Dialogs.Volumes.NewVolume.ViewModel
{
    public class NewVolumeViewModel : ChangeableDialogViewModel
    {
        private readonly IMsixHeroApplication application;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManager;

        public NewVolumeViewModel(
            IMsixHeroApplication application,
            IInteractionService interactionService, 
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManager) : base("New volume", interactionService)
        {
            this.application = application;
            this.volumeManager = volumeManager;
            this.Path = new ChangeableProperty<string>("WindowsApps");

            // This can be longer...
            var taskForFreeLetters = this.GetLetters();

            taskForFreeLetters.ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                {
                    return;
                }

                var dr = t.Result.FirstOrDefault();
                if (dr != null)
                {
                    this.SelectedLetter.CurrentValue = dr.PackageStorePath;
                    this.SelectedLetter.Commit();
                }

#pragma warning disable 4014
                this.Letters.Load(Task.FromResult(t.Result));
#pragma warning restore 4014
            });

            this.Letters = new AsyncProperty<List<VolumeCandidateViewModel>>();
            this.SelectedLetter = new ChangeableProperty<string>();
            this.PathType = new ChangeableProperty<NewVolumePathType>();
            this.SetAsDefault = new ChangeableProperty<bool>();
            this.PathType.ValueChanged += this.PathTypeOnValueChanged;

            this.AddChildren(this.Path, this.PathType, this.SetAsDefault);
        }

        public ChangeableProperty<NewVolumePathType> PathType { get; }

        public ChangeableProperty<string> Path { get; }

        public AsyncProperty<List<VolumeCandidateViewModel>> Letters { get; }

        public ChangeableProperty<bool> SetAsDefault { get; }

        public ChangeableProperty<string> SelectedLetter { get; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            var drivePath = System.IO.Path.Combine(this.SelectedLetter.CurrentValue ?? "C:\\", this.Path.CurrentValue ?? string.Empty);

            progress.Report(new ProgressData(20, "Adding a volume..."));

            var mgr = await this.volumeManager.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var volume = await mgr.Add(drivePath, cancellationToken, progress).ConfigureAwait(false);
            if (volume == null)
            {
                return false;
            }

            if (this.SetAsDefault.CurrentValue)
            {
                progress.Report(new ProgressData(80, "Changing the default volume..."));
                await mgr.SetDefault(drivePath, cancellationToken).ConfigureAwait(false);
            }

            progress.Report(new ProgressData(90, "Reading volumes..."));
            
            await this.application.CommandExecutor.Invoke<GetVolumesCommand, IList<AppxVolume>>(this, new GetVolumesCommand(), cancellationToken).ConfigureAwait(false);
            return true;
        }

        private async Task<List<VolumeCandidateViewModel>> GetLetters()
        {
            var mgr = await this.volumeManager.GetProxyFor(SelfElevationLevel.AsInvoker).ConfigureAwait(false);
            var disks = await mgr.GetAvailableDrivesForAppxVolume(true).ConfigureAwait(false);
            return disks.Select(r => new VolumeCandidateViewModel(r)).ToList();
        }

        private void PathTypeOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if ((NewVolumePathType)e.NewValue == NewVolumePathType.Default)
            {
                this.Path.CurrentValue = "WindowsApps";
            }
        }
    }
}
