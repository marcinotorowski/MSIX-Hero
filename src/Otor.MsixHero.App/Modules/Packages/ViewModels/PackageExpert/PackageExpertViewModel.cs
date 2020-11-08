using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels.PackageExpert
{
    public class PackageExpertViewModel : NotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PackageExpertViewModel));

        private readonly IInterProcessCommunicationManager interProcessCommunicationManager;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> appxPackageManagerProvider;
        private readonly ISelfElevationProxyProvider<ISigningManager> signManager;
        private readonly IInteractionService interactionService;
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
            this.signManager = signManager;
            this.interactionService = interactionService;
            this.runningDetector = runningDetector;
            this.packagePath = packagePath;
            this.dialogService = dialogService;
        }
        
        public ProgressProperty Progress { get; } = new ProgressProperty();

        public async Task Load()
        {
            using (var cts = new CancellationTokenSource())
            {
                var progress = new Progress<ProgressData>();
                var t = this.LoadPackage(cts.Token, progress);
                this.Progress.MonitorProgress(t, cts, progress);
                await t;
            }
        }

        public PackageExpertPropertiesViewModel Properties { get; private set; }

        private async Task LoadPackage(CancellationToken cancellation = default, IProgress<ProgressData> progress = default)
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
