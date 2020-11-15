using System.Windows;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.Main.Sidebar.Views
{
    /// <summary>
    /// Interaction logic for SidebarView.xaml
    /// </summary>
    public partial class SidebarView
    {
        private readonly IBusyManager busyManager;

        public SidebarView(IBusyManager busyManager)
        {
            this.busyManager = busyManager;
            this.InitializeComponent();

            this.busyManager.StatusChanged += this.OnBusyManagerStatusChanged;
        }

        private void OnBusyManagerStatusChanged(object sender, IBusyStatusChange e)
        {
            if (Application.Current.CheckAccess())
            {
                this.IsEnabled = !e.IsBusy;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => this.IsEnabled = !e.IsBusy);
            }
        }
    }
}
