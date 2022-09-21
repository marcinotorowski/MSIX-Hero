using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.More;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Open;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Start;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions
{
    public class ActionsViewModel : NotifyPropertyChanged, ILoadPackage, IInstallationAware
    {
        private readonly IAppxPackageQueryService _packageQueryService;
        private bool _isInstalled;

        public ActionsViewModel(
            IAppxPackageQueryService packageQueryService,
            IEventAggregator eventAggregator, 
            IConfigurationService configurationService)
        {
            this._packageQueryService = packageQueryService;
            this.Start = new StartViewModel(eventAggregator, configurationService);
            this.Open = new OpenViewModel();
            this.More = new MoreViewModel();
        }

        public async Task LoadPackage(AppxPackage model, PackageEntry installationEntry, string filePath, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                Start.LoadPackage(model, installationEntry, filePath, cancellationToken),
                Open.LoadPackage(model, installationEntry, filePath, cancellationToken),
                More.LoadPackage(model, installationEntry, filePath, cancellationToken)).ConfigureAwait(false);

            this.IsInstalled = installationEntry?.InstallDate != null;
        }

        public StartViewModel Start { get; }

        public OpenViewModel Open { get; }

        public MoreViewModel More { get; }

        public bool IsInstalled
        {
            get => this._isInstalled;
            set
            {
                if (this.SetField(ref this._isInstalled, value))
                {
                    this.Start.IsInstalled = value;
                    this.More.IsInstalled = value;
                }
            }
        }
    }
}
