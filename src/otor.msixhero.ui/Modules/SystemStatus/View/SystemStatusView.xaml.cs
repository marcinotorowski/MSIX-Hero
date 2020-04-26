using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.DeveloperMode;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.Repackaging;

namespace otor.msixhero.ui.Modules.SystemStatus.View
{
    /// <summary>
    /// Interaction logic for System Status View.
    /// </summary>
    public partial class SystemStatusView
    {
        public SystemStatusView()
        {
            this.InitializeComponent();
        }

        private void OpenLink(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo((string) ((Hyperlink) sender).Tag)
            {
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private void WindowsSettingsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var sourceViewModel = (BaseRecommendationViewModel)e.Parameter;
            
            if (sourceViewModel is DeveloperAndSideloadingRecommendationViewModel)
            {
                var process = new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true };
                Process.Start(process);
            }
            else if (sourceViewModel is RepackagingRecommendationViewModel)
            {
                var process = new ProcessStartInfo("services.msc") { UseShellExecute = true };
                Process.Start(process);
            }
        }

        private void RefreshExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((SystemStatusViewModel)this.DataContext).Refresh();
        }

        private void StartExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var serviceItem = (ServiceRecommendationViewModel)e.Parameter;
            serviceItem.Start();
        }

        private void StopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var serviceItem = (ServiceRecommendationViewModel)e.Parameter;
            serviceItem.Stop();
        }
    }
}
