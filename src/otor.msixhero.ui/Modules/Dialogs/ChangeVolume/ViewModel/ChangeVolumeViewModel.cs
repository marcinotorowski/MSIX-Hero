using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Volumes;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
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
        private readonly IApplicationStateManager appStateManager;
        private readonly IInteractionService interactionService;
        private readonly IDialogService dialogService;
        private readonly ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory;
        private string packageInstallLocation;

        public ChangeVolumeViewModel(
            IApplicationStateManager appStateManager,
            IInteractionService interactionService, 
            IDialogService dialogService,
            ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory) : base("Change volume", interactionService)
        {
            this.appStateManager = appStateManager;
            this.interactionService = interactionService;
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
            var mgr = await this.volumeManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var id = Path.GetFileName(this.packageInstallLocation);
            await mgr.MovePackageToVolume(this.TargetVolume.CurrentValue, id, cancellationToken, progress).ConfigureAwait(false);

            progress.Report(new ProgressData(100, "Reading packages..."));
            await this.appStateManager.CommandExecutor.ExecuteAsync(new GetPackages(this.appStateManager.CurrentState.Packages.Context), cancellationToken).ConfigureAwait(false);

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

                var t1 = this.GetAllVolumes();
                var t2 = this.AllVolumes.Load(t1);

                t2.ContinueWith(res =>
                {
                    if (res.IsCanceled || res.IsFaulted)
                    {
                        return;
                    }

                    var result = this.AllVolumes.CurrentValue;
                    var newVolume = result.FirstOrDefault(d => !current.Contains(d.PackageStorePath)) ?? result.FirstOrDefault();
                    this.TargetVolume.CurrentValue = newVolume?.Name;
                });
            });
        }
    }
}
