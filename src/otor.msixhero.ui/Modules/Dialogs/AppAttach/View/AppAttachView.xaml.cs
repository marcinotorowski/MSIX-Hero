using System.Diagnostics;
using System.Windows;
using otor.msixhero.ui.Modules.Dialogs.AppAttach.ViewModel;
using Xceed.Wpf.Toolkit;

namespace otor.msixhero.ui.Modules.Dialogs.AppAttach.View
{
    /// <summary>
    /// Interaction logic for App Attach View.
    /// </summary>
    public partial class AppAttachView
    {
        public AppAttachView()
        {
            this.InitializeComponent();
        }
        
        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var pathOutput = ((AppAttachViewModel)this.DataContext).OutputPath;
            Process.Start("explorer.exe", "/select," + pathOutput);
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
