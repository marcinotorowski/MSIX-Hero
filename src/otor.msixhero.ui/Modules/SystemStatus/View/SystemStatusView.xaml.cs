using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

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
    }
}
