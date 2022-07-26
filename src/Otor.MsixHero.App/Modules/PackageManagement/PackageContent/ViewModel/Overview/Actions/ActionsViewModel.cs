using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.More;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Open;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Start;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions
{
    public class ActionsViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public ActionsViewModel(IEventAggregator eventAggregator, IConfigurationService configurationService)
        {
            Start = new StartViewModel(eventAggregator, configurationService);
            Open = new OpenViewModel();
            More = new MoreViewModel();
        }

        public async Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                Start.LoadPackage(model, filePath, cancellationToken),
                Open.LoadPackage(model, filePath, cancellationToken),
                More.LoadPackage(model, filePath, cancellationToken)).ConfigureAwait(false);
        }

        public StartViewModel Start { get; }

        public OpenViewModel Open { get; }

        public MoreViewModel More { get; }
    }
}
