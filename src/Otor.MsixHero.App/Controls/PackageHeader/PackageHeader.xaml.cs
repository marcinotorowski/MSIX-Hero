using System.Windows;
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls.PackageHeader
{
    /// <summary>
    /// Interaction logic for PackageHeader.
    /// </summary>
    public partial class PackageHeader
    {
        public static readonly DependencyProperty LogoProperty = DependencyProperty.Register("Logo", typeof(ImageSource), typeof(PackageHeader), new PropertyMetadata(null));

        public static readonly DependencyProperty PublisherProperty = DependencyProperty.Register("Publisher", typeof(string), typeof(PackageHeader), new PropertyMetadata(null));
        
        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register("Version", typeof(string), typeof(PackageHeader), new PropertyMetadata(null));

        public static readonly DependencyProperty PackageNameProperty = DependencyProperty.Register("PackageName", typeof(string), typeof(PackageHeader), new PropertyMetadata(null));
        
        public static readonly DependencyProperty TileColorProperty = DependencyProperty.Register("TileColor", typeof(Brush), typeof(PackageHeader), new PropertyMetadata(Brushes.DarkGray));

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(PackageHeader), new PropertyMetadata(null));
        
        public static readonly DependencyProperty HeaderForegroundProperty = DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(PackageHeader), new PropertyMetadata(Brushes.White));


        public PackageHeader()
        {
            InitializeComponent();
        }

        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        public Brush HeaderForeground
        {
            get => (Brush)GetValue(HeaderForegroundProperty);
            set => SetValue(HeaderForegroundProperty, value);
        }

        public ImageSource Logo
        {
            get => (ImageSource)GetValue(LogoProperty);
            set => SetValue(LogoProperty, value);
        }

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public Brush TileColor
        {
            get => (Brush)GetValue(TileColorProperty);
            set => SetValue(TileColorProperty, value);
        }

        public string Publisher
        {
            get => (string)GetValue(PublisherProperty);
            set => SetValue(PublisherProperty, value);
        }

        public string PackageName
        {
            get => (string)GetValue(PackageNameProperty);
            set => SetValue(PackageNameProperty, value);
        }


    }
}
