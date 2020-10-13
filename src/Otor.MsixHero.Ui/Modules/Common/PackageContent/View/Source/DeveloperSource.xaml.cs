using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Otor.MsixHero.Ui.Modules.Common.PackageContent.View.Source
{
    /// <summary>
    /// Interaction logic for DeveloperSource.
    /// </summary>
    public partial class DeveloperSource
    {
        public static readonly DependencyProperty ManifestFilePathProperty =  DependencyProperty.Register("ManifestFilePath", typeof(string), typeof(DeveloperSource), new PropertyMetadata(null));

        public DeveloperSource()
        {
            InitializeComponent();
        }
        
        public string ManifestFilePath
        {
            get => (string)GetValue(ManifestFilePathProperty);
            set => SetValue(ManifestFilePathProperty, value);
        }

        private void LinkClicked(object sender, RoutedEventArgs e)
        {
            var path = (string) ((Hyperlink) sender).Tag;
            var psi = new ProcessStartInfo("explorer.exe", "/select," + path)
            {
                UseShellExecute = true
            };

            Process.Start(psi);
        }
    }
}
