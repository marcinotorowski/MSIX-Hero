using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Modules.Common.PackageContent.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PackageExpert.ViewModel
{
    public class PackageExpertViewModel : ChangeableDialogViewModel, IDialogAware
    {
        public PackageExpertViewModel(
            IInteractionService interactionService, 
            IApplicationStateManager stateManager,
            ISelfElevationManagerFactory<ISigningManager> signManager,
            IConfigurationService configurationService) : base("MSIX Hero - Package Expert", interactionService)
        {
            this.Content = new PackageContentViewModel(stateManager, signManager, interactionService, configurationService);
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {   
            var parsedParams = new PackageExpertSelection(parameters);
#pragma warning disable 4014
            var task = this.Content.LoadPackage(parsedParams.Source, CancellationToken.None);
            this.Content.SelectedPackageManifestInfo.Load(task);

            task.ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    return;
                }

                this.Content.SelectedPackageJsonInfo.Load(this.Content.LoadPackageJson(parsedParams.Source, task.Result.Model, CancellationToken.None));
            });
#pragma warning restore 4014
        }

        public PackageContentViewModel Content { get; }

        protected override Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return Task.FromResult(true);
        }
    }
}
