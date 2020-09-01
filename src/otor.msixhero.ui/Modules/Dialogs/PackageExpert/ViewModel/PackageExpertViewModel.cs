using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.ViewModel
{
    public class PackageExpertViewModel : ChangeableDialogViewModel, IDialogAware
    {
        public PackageExpertViewModel(
            IInterProcessCommunicationManager interProcessCommunicationManager,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            IInteractionService interactionService, 
            ISelfElevationProxyProvider<ISigningManager> signManager,
            IConfigurationService configurationService) : base("MSIX Hero - Package Expert", interactionService)
        {
            this.Content = new PackageContentViewModel(interProcessCommunicationManager, packageManagerProvider, signManager, interactionService, configurationService);
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
