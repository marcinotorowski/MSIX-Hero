using System;
using System.Windows;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageList.Views
{
    public partial class SelectablePackage : IDisposable
    {
        public SelectablePackage()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;
        }

        public void Dispose()
        {
            this.Loaded -= OnLoaded;
            this.Unloaded -= OnUnloaded;
            this.SizeChanged -= OnSizeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ((SelectablePackage)sender).Dispose();
        }
        
        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((SelectablePackage)sender).Loaded -= OnLoaded;
            ((SelectablePackage)sender).SetVisibility();
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((SelectablePackage)sender).SetVisibility();
        }

        private void SetVisibility()
        {
            // these values seem to work, so that we have a reasonable compromise
            this.Logo.Visibility = this.ActualWidth > 220 ? Visibility.Visible : Visibility.Collapsed;
            this.Column2.Visibility = this.ActualWidth > 304 ? Visibility.Visible : Visibility.Collapsed;
            this.Star.Visibility = this.ActualWidth > 280 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
