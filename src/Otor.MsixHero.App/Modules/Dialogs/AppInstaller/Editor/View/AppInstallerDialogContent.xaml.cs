using Xceed.Wpf.Toolkit;

namespace Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.View
{
    public partial class AppInstallerDialogContent
    {
        public AppInstallerDialogContent()
        {
            InitializeComponent();
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
