using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.More;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Open;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Start;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions
{
    public class ActionsViewModel : NotifyPropertyChanged, ILoadPackage, IInstallationAware
    {
        private bool _isInstalled;
        private bool _isLoading;

        public ActionsViewModel(IEventAggregator eventAggregator, 
            IConfigurationService configurationService)
        {
            this.Start = new StartViewModel(eventAggregator, configurationService);
            this.Open = new OpenViewModel();
            this.More = new MoreViewModel();
        }

        public bool IsLoading
        {
            get => this._isLoading;
            private set => this.SetField(ref this._isLoading, value);
        }

        public async Task LoadPackage(AppxPackage model, PackageEntry installationEntry, string filePath, CancellationToken cancellationToken)
        {
            try
            {
                this.IsLoading = true;

                await Task.WhenAll(
                    Start.LoadPackage(model, installationEntry, filePath, cancellationToken),
                    Open.LoadPackage(model, installationEntry, filePath, cancellationToken),
                    More.LoadPackage(model, installationEntry, filePath, cancellationToken)).ConfigureAwait(false);

                this.IsInstalled = installationEntry?.InstallDate != null;
            }
            finally
            {
                this.IsLoading = false;
            }
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
