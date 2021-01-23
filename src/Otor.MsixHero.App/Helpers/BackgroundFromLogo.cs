// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Drawing.Color;

namespace Otor.MsixHero.App.Helpers
{
    public class BackgroundFromLogo : DependencyObject
    {
        public static readonly DependencyProperty LogoFileProperty = DependencyProperty.RegisterAttached("LogoFile", typeof(string), typeof(BackgroundFromLogo), new PropertyMetadata(null, OnLogoChanged));
        public static readonly DependencyProperty LogoBytesProperty = DependencyProperty.RegisterAttached("LogoBytes", typeof(byte[]), typeof(BackgroundFromLogo), new PropertyMetadata(null, OnLogoChanged));
        public static readonly DependencyProperty LogoColorProperty = DependencyProperty.RegisterAttached("LogoColor", typeof(SolidColorBrush), typeof(BackgroundFromLogo), new PropertyMetadata(null, OnLogoColorChanged));
        
        public static void SetLogoColor(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(LogoColorProperty, value);
        }

        public static SolidColorBrush GetLogoColor(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(LogoColorProperty);
        }

        public static void SetLogoFile(DependencyObject obj, string value)
        {
            obj.SetValue(LogoFileProperty, value);
        }

        public static string GetLogoFile(DependencyObject obj)
        {
            return (string)obj.GetValue(LogoFileProperty);
        }

        public static byte[] GetLogoBytes(DependencyObject obj)
        {
            return (byte[])obj.GetValue(LogoBytesProperty);
        }

        public static void SetLogoBytes(DependencyObject obj, byte[] value)
        {
            obj.SetValue(LogoBytesProperty, value);
        }

        private static async void OnLogoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Stream s;

            if (e.NewValue is byte[] newValueByte)
            {
                s = new MemoryStream(newValueByte);
            }
            else if (e.NewValue is string newValueString)
            {
                s = File.OpenRead(newValueString);
            }
            else
            {
                return;
            }

            Color c;

            using (s)
            {
                c = await GetColor(s).ConfigureAwait(true);
            }

            Animate(d, c);
        }

        private static void OnLogoColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = ((SolidColorBrush)e.NewValue).Color;
            Animate(d, Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        private static void Animate(DependencyObject target, Color c)
        {
            System.Windows.Media.Color targetColor;

            if (c.A < 10)
            {
                targetColor = new System.Windows.Media.Color
                {
                    A = 60,
                    R = 1,
                    G = 115,
                    B = 199
                };
            }
            else
            {
                targetColor = new System.Windows.Media.Color
                {
                    A = Math.Min(c.A, (byte)60),
                    R = c.R,
                    G = c.G,
                    B = c.B
                };
            }

            if (!(((Border) target).Background is LinearGradientBrush linearBrush))
            {
                linearBrush = new LinearGradientBrush();
                linearBrush.StartPoint = new System.Windows.Point(0, 0);
                linearBrush.EndPoint = new System.Windows.Point(0, 1.0);
                var gs1 = new GradientStop(Colors.Transparent, 0.0);
                var gs2 = new GradientStop(System.Windows.Media.Color.FromRgb(239, 239, 239), 1.0);

                linearBrush.GradientStops.Add(gs1);
                linearBrush.GradientStops.Add(gs2);
            }

            var colorAnimation = new ColorAnimation(
                targetColor,
                new Duration(TimeSpan.FromMilliseconds(500)))
            {
                AccelerationRatio = 0.5,
                DecelerationRatio = 0.5
            };

            Storyboard.SetTarget(colorAnimation, linearBrush.GradientStops[0]);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath(nameof(GradientStop.Color)));

            ((Border) target).Background = linearBrush;
            var storyboard = new Storyboard();
            storyboard.Children.Add(colorAnimation);
            storyboard.Freeze();
            storyboard.Begin();
        }

        private static Task<Color> GetColor(Stream s)
        {
            return Task.Run(() =>
            {
                using (var b = new Bitmap(s))
                {
                    return b.GetPixel(b.Width / 2, b.Height / 2);
                }
            });
        }
    }
}
