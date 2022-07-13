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

        public async Task LoadPackage(AppxPackage model, string filePath)
        {
            await Task.WhenAll(
                Start.LoadPackage(model, filePath),
                Open.LoadPackage(model, filePath),
                More.LoadPackage(model, filePath)).ConfigureAwait(false);
        }

        public StartViewModel Start { get; }
        public OpenViewModel Open { get; }
        public MoreViewModel More { get; }
    }
}
