using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Otor.MsixHero.App.Helpers
{
    internal class ImageExtension : DependencyObject
    {
        public static readonly DependencyProperty SourceImagePathProperty = DependencyProperty.RegisterAttached("SourceImagePath", typeof(string), typeof(ImageExtension), new PropertyMetadata(null, PropertyChangedCallback));
        
        public static string GetSourceImagePath(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceImagePathProperty);
        }

        public static void SetSourceImagePath(DependencyObject obj, string value)
        {
            obj.SetValue(SourceImagePathProperty, value);
        }
        
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Image image)
            {
                throw new NotSupportedException();
            }

            if (e.NewValue == DependencyProperty.UnsetValue)
            {
                image.Source = null;
                return;
            }

            if (e.NewValue is BitmapImage bitmapImage)
            {
                image.Source = bitmapImage;
            }
            else if (e.NewValue is byte[] byteArray)
            {
                var memStream = new MemoryStream(byteArray);

                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = memStream;
                bi.EndInit();
                bi.Freeze();

                image.Source = bi;
            }
            else if(e.NewValue is string newValueAsString && !string.IsNullOrEmpty(newValueAsString))
            {
                if (!File.Exists(newValueAsString))
                {
                    image.Source = null;
                    return;
                }

                var fileStream = File.OpenRead(newValueAsString);
                var memStream = new MemoryStream();
                fileStream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = memStream;
                bi.EndInit();
                bi.Freeze();

                image.Source = bi;
            }
            else
            {
                image.Source = null;
            }
        }
    }
}
