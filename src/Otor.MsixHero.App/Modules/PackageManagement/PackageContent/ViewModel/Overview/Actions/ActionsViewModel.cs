using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.More;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Open;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Start;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions
{
    public class ActionsViewModel : NotifyPropertyChanged, ILoadPackage, IInstallationAware
    {
        private readonly IAppxPackageQuery _packageQuery;
        private bool _isInstalled;

        public ActionsViewModel(
            IAppxPackageQuery packageQuery,
            IEventAggregator eventAggregator, 
            IConfigurationService configurationService)
        {
            this._packageQuery = packageQuery;
            this.Start = new StartViewModel(eventAggregator, configurationService);
            this.Open = new OpenViewModel();
            this.More = new MoreViewModel();
        }

        public async Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                Start.LoadPackage(model, filePath, cancellationToken),
                Open.LoadPackage(model, filePath, cancellationToken),
                More.LoadPackage(model, filePath, cancellationToken)).ConfigureAwait(false);

            if (filePath.StartsWith("C:\\Program Files\\WindowsApps\\", StringComparison.OrdinalIgnoreCase))
            {
                this.IsInstalled = true;
            }
            else
            {
                var installedPackage = ExceptionGuard.Guard(() => this._packageQuery.GetInstalledPackage(model.FullName, cancellationToken: cancellationToken).GetAwaiter().GetResult());
                this.IsInstalled = installedPackage != null;
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
