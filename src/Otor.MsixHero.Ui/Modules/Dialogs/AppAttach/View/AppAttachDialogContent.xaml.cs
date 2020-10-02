using System.Diagnostics;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Otor.MsixHero.Ui.Modules.Dialogs.AppAttach.View
{
    public partial class AppAttachDialogContent
    {
        public AppAttachDialogContent()
        {
            InitializeComponent();
        }
        private void HyperlinkMsdn_OnClick(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo("https://msixhero.net/redirect/msix-app-attach/prepare-ps");
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void Spinner_OnSpin(object sender, SpinEventArgs e)
        {
            var spinner = (ButtonSpinner)sender;
            var content = spinner.Content as string;

            int.TryParse(content ?? "0", out var value);
            if (e.Direction == SpinDirection.Increase)
                value += 10;
            else
                value -= 10;

            spinner.Content = value.ToString();
        }
    }
}
