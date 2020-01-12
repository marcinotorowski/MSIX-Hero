using Xceed.Wpf.Toolkit;

namespace otor.msixhero.ui.Modules.Dialogs.AppInstaller.View
{
    /// <summary>
    /// Interaction logic for AppInstallerView.
    /// </summary>
    public partial class AppInstallerView
    {
        public AppInstallerView()
        {
            this.InitializeComponent();
        }

        private void Spinner_OnSpin(object sender, SpinEventArgs e)
        {
            var spinner = (ButtonSpinner)sender;
            var content = spinner.Content as string;

            int.TryParse(content ?? "0", out var value);
            if (e.Direction == SpinDirection.Increase)
                value++;
            else
                value--;

            spinner.Content = value.ToString();
        }
    }
}
