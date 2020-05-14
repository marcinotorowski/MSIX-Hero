using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Volumes;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.ChangeVolume.ViewModel.Items;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.ChangeVolume.ViewModel
{
    public class ChangeVolumeViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IDialogService dialogService;
        private readonly ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory;
        private string packageInstallLocation;

        public ChangeVolumeViewModel(
            IInteractionService interactionService, 
            IDialogService dialogService,
            ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory) : base("Change volume", interactionService)
        {
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
            progress.Report(new ProgressData(20, "Moving to the selected volume..."));
            var mgr = await this.volumeManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var id = Path.GetFileName(this.packageInstallLocation);
            await mgr.MovePackageToVolume(this.TargetVolume.CurrentValue, id, cancellationToken, progress).ConfigureAwait(false);
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
            var mgr = await this.volumeManagerFactory.Get().ConfigureAwait(false);
            var disks = await mgr.GetAll().ConfigureAwait(false);
            return disks.Select(r => new VolumeCandidateViewModel(r)).Concat(new[] { new VolumeCandidateViewModel(new AppxVolume()) }).ToList();
        }

        private async Task<VolumeCandidateViewModel> GetCurrentVolume(string packagePath)
        {
            var mgr = await this.volumeManagerFactory.Get().ConfigureAwait(false);
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

            this.dialogService.ShowDialog(Constants.PathNewVolume, new DialogParameters(), result =>
            {
                if (result.Result == ButtonResult.Cancel)
                {
                    return;
                }

                var t = this.GetAllVolumes();
                this.AllVolumes.Load(t).ConfigureAwait(false);

                t.ContinueWith(res =>
                {
                    if (res.IsCanceled || res.IsFaulted || res.Result == null)
                    {
                        return;
                    }

                    var newVolume = res.Result.FirstOrDefault(d => !current.Contains(d.PackageStorePath)) ?? res.Result.FirstOrDefault();
                    this.TargetVolume.CurrentValue = newVolume?.PackageStorePath;
                });
            });
        }
    }
}
