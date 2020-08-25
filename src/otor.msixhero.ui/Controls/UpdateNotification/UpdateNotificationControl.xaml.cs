using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Ui.Controls.UpdateNotification
{
    /// <summary>
    /// Interaction logic for UpdateNotificationControl.xaml
    /// </summary>
    public partial class UpdateNotificationControl : UserControl
    {
        public static readonly DependencyProperty IsUpdateAvailableProperty = DependencyProperty.Register("IsUpdateAvailable", typeof(bool), typeof(UpdateNotificationControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty CurrentVersionProperty =
            // ReSharper disable once PossibleNullReferenceException
            DependencyProperty.Register("CurrentVersion", typeof(string), typeof(UpdateNotificationControl), new PropertyMetadata(typeof(UpdateNotificationControl).Assembly.GetName().Version.ToString(3)));

        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateNotificationControl));

        public UpdateNotificationControl()
        {
            InitializeComponent();
        }

        public string CurrentVersion
        {
            get => (string)this.GetValue(CurrentVersionProperty);
            set => this.SetValue(CurrentVersionProperty, value);
        }

        public bool IsUpdateAvailable
        {
            get => (bool)this.GetValue(IsUpdateAvailableProperty);
            set => this.SetValue(IsUpdateAvailableProperty, value);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.IsUpdateAvailable = false;
        }

        private void OnShowReleaseNotesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo("https://msixhero.net/redirect/release-notes/" + this.CurrentVersion)
                {
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception exception)
            {
                Logger.Warn(exception);
            }
        }
    }
}
