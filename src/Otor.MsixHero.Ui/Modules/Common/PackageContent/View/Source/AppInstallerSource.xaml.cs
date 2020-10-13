using System;
using System.Windows;

namespace Otor.MsixHero.Ui.Modules.Common.PackageContent.View.Source
{
    /// <summary>
    /// Interaction logic for AppInstallerSource.
    /// </summary>
    public partial class AppInstallerSource 
    {
        public static readonly DependencyProperty AppInstallerUriProperty = DependencyProperty.Register("AppInstallerUri", typeof(Uri), typeof(AppInstallerSource), new PropertyMetadata(null));

        public AppInstallerSource()
        {
            InitializeComponent();
        }

        public Uri AppInstallerUri
        {
            get => (Uri)GetValue(AppInstallerUriProperty);
            set => SetValue(AppInstallerUriProperty, value);
        }
    }
}
