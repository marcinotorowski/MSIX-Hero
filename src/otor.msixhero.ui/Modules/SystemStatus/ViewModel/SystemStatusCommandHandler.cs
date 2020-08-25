using System.Diagnostics;
using System.Windows.Input;
using Otor.MsixHero.Ui.Commands.RoutedCommand;

namespace Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel
{
    public class SystemStatusCommandHandler
    {
        public SystemStatusCommandHandler()
        {
            this.OpenAppsFeatures = new DelegateCommand(param => this.OpenAppsFeaturesExecute());
            this.OpenDevSettings = new DelegateCommand(param => this.OpenDevSettingsExecute());
            this.OpenServices = new DelegateCommand(param => this.OpenServicesExecute());
        }

        public ICommand OpenAppsFeatures { get; }

        public ICommand OpenDevSettings { get; }

        public ICommand OpenServices { get; }

        private void OpenAppsFeaturesExecute()
        {
            var process = new ProcessStartInfo("ms-settings:appsfeatures") { UseShellExecute = true };
            Process.Start(process);
        }

        private void OpenServicesExecute()
        {
            var process = new ProcessStartInfo("services.msc") { UseShellExecute = true };
            Process.Start(process);
        }

        private void OpenDevSettingsExecute()
        {
            var process = new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true };
            Process.Start(process);
        }
    }
}
