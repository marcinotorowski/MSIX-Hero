using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.Ui.Modules.Dialogs.NewSelfSigned.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.NewSelfSigned.View
{
    /// <summary>
    /// Interaction logic for NewSelfSignedView.
    /// </summary>
    public partial class NewSelfSignedView
    {
        public NewSelfSignedView()
        {
            this.InitializeComponent();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", ((NewSelfSignedViewModel)this.DataContext).OutputPath.CurrentValue);
        }

        private void HyperlinkImportWizard_OnClick(object sender, RoutedEventArgs e)
        {
            ((NewSelfSignedViewModel)this.DataContext).ImportNewCertificate.Execute(null);
        }
    }
}
