using System;
using System.Windows;
using System.Windows.Controls;

namespace Otor.MsixHero.Ui.Controls.Volume
{
    public class VolumeControl : Control
    {
        private static readonly DependencyPropertyKey CaptionPropertyKey = DependencyProperty.RegisterReadOnly("Caption", typeof(string), typeof(VolumeControl), new PropertyMetadata(null));
        private static readonly DependencyPropertyKey DisplayNamePropertyKey = DependencyProperty.RegisterReadOnly("DisplayName", typeof(string), typeof(VolumeControl), new PropertyMetadata(null));
        private static readonly DependencyPropertyKey PercentPropertyKey = DependencyProperty.RegisterReadOnly("Percent", typeof(double), typeof(VolumeControl), new PropertyMetadata(0.0));
        private static readonly DependencyPropertyKey ThresholdReachedPropertyKey = DependencyProperty.RegisterReadOnly("ThresholdReached", typeof(bool), typeof(VolumeControl), new PropertyMetadata(false));
        
        public static readonly DependencyProperty CaptionProperty = CaptionPropertyKey.DependencyProperty;
        public static readonly DependencyProperty DisplayNameProperty = DisplayNamePropertyKey.DependencyProperty;
        public static readonly DependencyProperty PercentProperty = PercentPropertyKey.DependencyProperty;
        public static readonly DependencyProperty ThresholdReachedProperty = ThresholdReachedPropertyKey.DependencyProperty;

        public static readonly DependencyProperty UseShortPathsProperty = DependencyProperty.Register("UseShortPaths", typeof(bool), typeof(VolumeControl), new PropertyMetadata(false, OnDisplayNameComponentChanged));
        public static readonly DependencyProperty TotalSizeProperty = DependencyProperty.Register("TotalSize", typeof(long), typeof(VolumeControl), new PropertyMetadata(0L, OnSizeComponentChanged));
        public static readonly DependencyProperty SearchKeyProperty = DependencyProperty.Register("SearchKey", typeof(string), typeof(VolumeControl), new PropertyMetadata(null));
        public static readonly DependencyProperty OccupiedSizeProperty = DependencyProperty.Register("OccupiedSize", typeof(long), typeof(VolumeControl), new PropertyMetadata(0L, OnSizeComponentChanged));
        public static readonly DependencyProperty IsDefaultProperty = DependencyProperty.Register("IsDefault", typeof(bool), typeof(VolumeControl), new PropertyMetadata(false));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(VolumeControl), new PropertyMetadata(null, OnDisplayNameComponentChanged));
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(VolumeControl), new PropertyMetadata(null, OnDisplayNameComponentChanged));
        public static readonly DependencyProperty IsOfflineProperty = DependencyProperty.Register("IsOffline", typeof(bool), typeof(VolumeControl), new PropertyMetadata(false));

        static VolumeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VolumeControl), new FrameworkPropertyMetadata(typeof(VolumeControl)));
        }

        public string SearchKey
        {
            get => (string)GetValue(SearchKeyProperty);
            set => SetValue(SearchKeyProperty, value);
        }
        
        public bool IsOffline
        {
            get => (bool)GetValue(IsOfflineProperty);
            set => SetValue(IsOfflineProperty, value);
        }

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            private set => SetValue(CaptionPropertyKey, value);
        }

        public bool ThresholdReached
        {
            get => (bool)GetValue(ThresholdReachedProperty);
            private set => SetValue(ThresholdReachedPropertyKey, value);
        }

        public double Percent
        {
            get => (double)GetValue(PercentProperty);
            private set => SetValue(PercentPropertyKey, value);
        }

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public long TotalSize
        {
            get => (long)GetValue(TotalSizeProperty);
            set => SetValue(TotalSizeProperty, value);
        }
        
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public bool UseShortPaths
        {
            get => (bool)GetValue(UseShortPathsProperty);
            set => SetValue(UseShortPathsProperty, value);
        }

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            private set => SetValue(DisplayNamePropertyKey, value);
        }
        
        public bool IsDefault
        {
            get => (bool)GetValue(IsDefaultProperty);
            set => SetValue(IsDefaultProperty, value);
        }

        public long OccupiedSize
        {
            get => (long)GetValue(OccupiedSizeProperty);
            set => SetValue(OccupiedSizeProperty, value);
        }

        public override void EndInit()
        {
            base.EndInit();
            this.CalculateSizeComponents();
            this.CalculateDisplayName();
        }

        private static void OnSizeComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (VolumeControl)d;
            if (!that.IsInitialized)
            {
                return;
            }

            that.CalculateSizeComponents();
        }

        private static void OnDisplayNameComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (VolumeControl)d;
            if (!that.IsInitialized)
            {
                return;
            }

            that.CalculateDisplayName();
        }

        private void CalculateDisplayName()
        {
            var path = this.Path;

            if (this.UseShortPaths)
            {
                if (path.Length == 1 || (path.Length > 1 && path[1] == ':'))
                {
                    path = char.ToUpper(path[0]) + ":\\";
                }
                else if (path.Length > 4 && path.StartsWith("\\\\", StringComparison.Ordinal))
                {
                    var nextSlash = path.IndexOf('\\', 2);
                    if (nextSlash != -1)
                    {
                        nextSlash = path.IndexOf('\\', nextSlash);
                        if (nextSlash != -1)
                        {
                            path = path.Substring(0, nextSlash).TrimEnd('\\');
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(this.Label))
            {
                this.DisplayName = path;
            }
            else
            {
                this.DisplayName = $"[{this.Label}] {path}";
            }
        }

        private void CalculateSizeComponents()
        {
            if (this.TotalSize == 0)
            {
                this.Percent = double.NaN;
                this.Caption = "Unknown size";
                this.ThresholdReached = false;
            }

            var sizeFree = FormatSize(this.TotalSize - this.OccupiedSize);
            var sizeTotal = FormatSize(this.TotalSize);
            this.Caption = $"{sizeFree} free of {sizeTotal}";
            this.Percent = 100.0 * OccupiedSize / TotalSize;
            this.ThresholdReached = this.Percent >= 90.0;
        }

        private static string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes < 1000)
            {
                return sizeInBytes + " B";
            }

            var units = new[] { "TB", "GB", "MB", "KB" };

            double size = sizeInBytes;
            for (var i = units.Length - 1; i >= 0; i--)
            {
                size /= 1024.0;

                if (size < 1024)
                {
                    return $"{size:0} {units[i]}";
                }

                if (size < 10 * 1024 && i > 0)
                {
                    i--;
                    size = Math.Floor(100.0 * size / 1024) / 100;
                    return $"{size:0.00} {units[i]}";
                }
            }

            return $"{size:0} {units[0]}";
        }
    }
}
