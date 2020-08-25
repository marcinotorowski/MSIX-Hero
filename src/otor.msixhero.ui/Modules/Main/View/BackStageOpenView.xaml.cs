using System.Windows;
using System.Windows.Controls;
using Fluent;

namespace Otor.MsixHero.Ui.Modules.Main.View
{
    /// <summary>
    /// Interaction logic for BackStageOpenView.xaml
    /// </summary>
    public partial class BackStageOpenView : UserControl
    {
        public BackStageOpenView()
        {
            InitializeComponent();
        }

        private void BackstageButtonClicked(object sender, RoutedEventArgs e)
        {
            PopupService.RaiseDismissPopupEvent(sender, DismissPopupMode.Always);
        }
    }
}
