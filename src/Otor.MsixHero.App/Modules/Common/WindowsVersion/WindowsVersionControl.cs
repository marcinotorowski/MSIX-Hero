using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Otor.MsixHero.Appx.Common.WindowsVersioning;

namespace Otor.MsixHero.App.Modules.Common.WindowsVersion
{
    public class WindowsVersionControl : Control
    {
        public static readonly DependencyProperty Windows11ImageSourceProperty = DependencyProperty.Register(nameof(Windows11ImageSource), typeof(ImageSource), typeof(WindowsVersionControl), new PropertyMetadata(null, CalculateProperties));
        public static readonly DependencyProperty Windows10ImageSourceProperty = DependencyProperty.Register(nameof(Windows10ImageSource), typeof(ImageSource), typeof(WindowsVersionControl), new PropertyMetadata(null, CalculateProperties));
        public static readonly DependencyProperty Windows7ImageSourceProperty = DependencyProperty.Register(nameof(Windows7ImageSource), typeof(ImageSource), typeof(WindowsVersionControl), new PropertyMetadata(null, CalculateProperties));
        public static readonly DependencyProperty Windows8ImageSourceProperty = DependencyProperty.Register(nameof(Windows8ImageSource), typeof(ImageSource), typeof(WindowsVersionControl), new PropertyMetadata(null, CalculateProperties));
        public static readonly DependencyProperty UnknownImageSourceProperty = DependencyProperty.Register(nameof(UnknownImageSource), typeof(ImageSource), typeof(WindowsVersionControl), new PropertyMetadata(null, CalculateProperties));
        public static readonly DependencyPropertyKey ActualImageSourcePropertyKey = DependencyProperty.RegisterReadOnly(nameof(ActualImageSource), typeof(ImageSource), typeof(WindowsVersionControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ActualImageSourceProperty = ActualImageSourcePropertyKey.DependencyProperty;
        public static readonly DependencyPropertyKey ActualVersionPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ActualVersion), typeof(Appx.Common.WindowsVersioning.WindowsVersion), typeof(WindowsVersionControl), new PropertyMetadata(Appx.Common.WindowsVersioning.WindowsVersion.Unspecified));
        public static readonly DependencyProperty ActualVersionProperty = ActualVersionPropertyKey.DependencyProperty;
        public static readonly DependencyProperty WindowsVersionProperty = DependencyProperty.Register(nameof(WindowsVersion), typeof(string), typeof(WindowsVersionControl), new PropertyMetadata(null, CalculateProperties));

        public ImageSource Windows10ImageSource
        {
            get => (ImageSource)GetValue(Windows10ImageSourceProperty);
            set => SetValue(Windows10ImageSourceProperty, value);
        }

        public ImageSource Windows11ImageSource
        {
            get => (ImageSource)GetValue(Windows11ImageSourceProperty);
            set => SetValue(Windows11ImageSourceProperty, value);
        }

        public ImageSource Windows8ImageSource
        {
            get => (ImageSource)GetValue(Windows8ImageSourceProperty);
            set => SetValue(Windows8ImageSourceProperty, value);
        }

        public ImageSource Windows7ImageSource
        {
            get => (ImageSource)GetValue(Windows7ImageSourceProperty);
            set => SetValue(Windows7ImageSourceProperty, value);
        }

        public ImageSource UnknownImageSource
        {
            get => (ImageSource)GetValue(UnknownImageSourceProperty);
            set => SetValue(UnknownImageSourceProperty, value);
        }

        public ImageSource ActualImageSource
        {
            get => (ImageSource)GetValue(ActualImageSourceProperty);
            private set => SetValue(ActualImageSourcePropertyKey, value);
        }
        
        public Appx.Common.WindowsVersioning.WindowsVersion ActualVersion
        {
            get => (Appx.Common.WindowsVersioning.WindowsVersion)GetValue(ActualVersionProperty);
            private set => SetValue(ActualVersionPropertyKey, value);
        }
        
        public string WindowsVersion
        {
            get => (string)GetValue(WindowsVersionProperty);
            set => SetValue(WindowsVersionProperty, value);
        }
        
        private static void CalculateProperties(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == WindowsVersionControl.WindowsVersionProperty)
            {
                // just assign the correct image
                var newVersion = (string)e.NewValue == null 
                    ? Appx.Common.WindowsVersioning.WindowsVersion.Unspecified 
                    : WindowsNames.GetWindowsDesktop((string)e.NewValue).WindowsVersion;

                var currentVersion = ((WindowsVersionControl)d).ActualVersion;
                if (newVersion == currentVersion)
                {
                    return;
                }

                ((WindowsVersionControl)d).ActualVersion = newVersion;
            }

            switch (((WindowsVersionControl)d).ActualVersion)
            {
                case Appx.Common.WindowsVersioning.WindowsVersion.Win10:
                    ((WindowsVersionControl)d).ActualImageSource = ((WindowsVersionControl)d).Windows10ImageSource;
                    break;
                case Appx.Common.WindowsVersioning.WindowsVersion.Win11:
                    ((WindowsVersionControl)d).ActualImageSource = ((WindowsVersionControl)d).Windows11ImageSource;
                    break;
                case Appx.Common.WindowsVersioning.WindowsVersion.Win8:
                case Appx.Common.WindowsVersioning.WindowsVersion.Win81:
                    ((WindowsVersionControl)d).ActualImageSource = ((WindowsVersionControl)d).Windows8ImageSource;
                    break;
                case Appx.Common.WindowsVersioning.WindowsVersion.Win7:
                    ((WindowsVersionControl)d).ActualImageSource = ((WindowsVersionControl)d).Windows7ImageSource;
                    break;
                default:
                    ((WindowsVersionControl)d).ActualImageSource = ((WindowsVersionControl)d).UnknownImageSource;
                    break;
            }
        }
    }
}
