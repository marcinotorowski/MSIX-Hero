using System.Diagnostics;
using System.Windows;
using Otor.MsixHero.Ui.Modules.Dialogs.AppAttach.ViewModel;
using Xceed.Wpf.Toolkit;

namespace Otor.MsixHero.Ui.Modules.Dialogs.AppAttach.View
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
    }
}
