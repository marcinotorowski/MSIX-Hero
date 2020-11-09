using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels
{
    public class PackageExpertViewModel : NotifyPropertyChanged
    {
        private readonly IInterProcessCommunicationManager interProcessCommunicationManager;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> appxPackageManagerProvider;
        private readonly IRunningDetector runningDetector;
        private readonly IDialogService dialogService;
        private readonly string packagePath;

        public PackageExpertViewModel(
            string packagePath,
            IInterProcessCommunicationManager interProcessCommunicationManager,
            ISelfElevationProxyProvider<IAppxPackageManager> appxPackageManagerProvider,
            ISelfElevationProxyProvider<ISigningManager> signManager,
            IInteractionService interactionService,
            IRunningDetector runningDetector,
            IDialogService dialogService)
        {
            this.interProcessCommunicationManager = interProcessCommunicationManager;
            this.appxPackageManagerProvider = appxPackageManagerProvider;
            this.runningDetector = runningDetector;
            this.packagePath = packagePath;
            this.dialogService = dialogService;

            this.Trust = new TrustViewModel(packagePath, interactionService, signManager);
        }
        
        public ProgressProperty Progress { get; } = new ProgressProperty();

        public async Task Load()
        {
            using (var cts = new CancellationTokenSource())
            {
                var progress = new Progress<ProgressData>();
                var t1 = this.LoadReader(cts.Token);
                var t2 = this.Trust.LoadSignature(cts.Token);

                var allTasks = Task.WhenAll(t1, t2);
                this.Progress.MonitorProgress(allTasks, cts, progress);
                await allTasks;
            }
        }

        public TrustViewModel Trust { get; }

        public PackageExpertPropertiesViewModel Properties { get; private set; }


        private async Task LoadReader(CancellationToken cancellation = default)
        {
            using (var reader = FileReaderFactory.GetFileReader(this.packagePath))
            {
                var manifestReader = new AppxManifestReader();
                var manifest = await manifestReader.Read(reader, cancellation).ConfigureAwait(false);

                this.Properties = new PackageExpertPropertiesViewModel(manifest, this.packagePath);
                this.OnPropertyChanged(nameof(Properties));
            }
        }
    }
}
