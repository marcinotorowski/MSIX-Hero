using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items
{
    public class TrustViewModel : NotifyPropertyChanged
    {
        private readonly string packagePath;
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<ISigningManager> signManagerProvider;
        private bool isTrusting;
        private DelegateCommand trustMe;
        private DelegateCommand showPropertiesCommand;
        private string certificateFile;

        public TrustViewModel(
            string packagePath,
            IInteractionService interactionService,
            ISelfElevationProxyProvider<ISigningManager> signManagerProvider)
        {
            this.packagePath = packagePath;
            this.interactionService = interactionService;
            this.signManagerProvider = signManagerProvider;
        }

        public bool IsTrusting
        {
            get => this.isTrusting;
            set => this.SetField(ref this.isTrusting, value);
        }

        public ICommand TrustMeCommand
        {
            get
            {
                return this.trustMe ??= new DelegateCommand(async () =>
                    {
                        if (this.interactionService.Confirm("Are you sure you want to add this publisher to the list of trusted publishers (machine-wide)?", type: InteractionType.Question, buttons: InteractionButton.YesNo) != InteractionResult.Yes)
                        {
                            return;
                        }

                        try
                        {
                            this.IsTrusting = true;

                            var manager = await this.signManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator).ConfigureAwait(false);
                            await manager.Trust(this.certificateFile).ConfigureAwait(false);
                            await this.TrustStatus.Load(this.LoadSignature(CancellationToken.None)).ConfigureAwait(false);
                        }
                        finally
                        {
                            this.IsTrusting = false;
                        }
                    },
                    () => this.certificateFile != null);
            }
        }

        public ICommand ShowPropertiesCommand
        {
            get
            {
                return this.showPropertiesCommand ??= new DelegateCommand(() =>
                {
                    WindowsExplorerCertificateHelper.ShowFileSecurityProperties(this.certificateFile, IntPtr.Zero);
                }, () => this.certificateFile != null);
            }
        }

        public AsyncProperty<TrustStatus> TrustStatus { get; } = new AsyncProperty<TrustStatus>();

        public async Task<TrustStatus> LoadSignature(CancellationToken cancellationToken)
        {
            using (var source = FileReaderFactory.GetFileReader(this.packagePath))
            {
                if (source is ZipArchiveFileReaderAdapter zipFileReader)
                {
                    this.certificateFile = zipFileReader.PackagePath;
                    var manager = await this.signManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                    var signTask = manager.IsTrusted(zipFileReader.PackagePath, cancellationToken);
                    await this.TrustStatus.Load(signTask);
                    return await signTask.ConfigureAwait(false);
                }

                if (source is IAppxDiskFileReader fileReader)
                {
                    var file = new FileInfo(Path.Combine(fileReader.RootDirectory, "AppxSignature.p7x"));
                    this.certificateFile = file.FullName;
                    if (file.Exists)
                    {
                        var manager = await this.signManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                        var signTask = manager.IsTrusted(file.FullName, cancellationToken);
                        await this.TrustStatus.Load(signTask);
                        return await signTask.ConfigureAwait(false);
                    }
                }

                return null;
            }
        }

    }
}
