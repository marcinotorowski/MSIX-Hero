using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.NewVolume.ViewModel.Items;

namespace otor.msixhero.ui.Modules.Dialogs.NewVolume.ViewModel
{
    public class NewVolumeViewModel : ChangeableDialogViewModel
    {
        private readonly IApplicationStateManager manager;
        private readonly IAppxVolumeManager volumeManager;

        public NewVolumeViewModel(
            IApplicationStateManager manager,
            IInteractionService interactionService, 
            IAppxVolumeManager volumeManager) : base("New volume", interactionService)
        {
            this.manager = manager;
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
            var volume = await this.manager.CommandExecutor.GetExecuteAsync(new AddVolume { DrivePath = drivePath }, cancellationToken, progress).ConfigureAwait(false);
            if (volume == null)
            {
                return false;
            }

            if (this.SetAsDefault.CurrentValue)
            {
                progress.Report(new ProgressData(80, "Changing the default volume..."));
                await this.manager.CommandExecutor.ExecuteAsync(new SetDefaultVolume { DrivePath = volume.PackageStorePath }, cancellationToken).ConfigureAwait(false);
            }

            return true;
        }

        private async Task<List<VolumeCandidateViewModel>> GetLetters()
        {
            var disks = await this.volumeManager.GetAvailableDrivesForAppxVolume(true).ConfigureAwait(false);
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
