using System;
using System.Windows;
using System.Windows.Media;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.View
{
    /// <summary>
    /// Interaction logic for RelativeIndicator.
    /// </summary>
    public partial class RelativeIndicator
    {
        public static readonly DependencyProperty OldValueProperty = DependencyProperty.Register("OldValue", typeof(double), typeof(RelativeIndicator), new PropertyMetadata(0.0, PropertyChangedCallback));
        
        public static readonly DependencyProperty NewValueProperty = DependencyProperty.Register("NewValue", typeof(double), typeof(RelativeIndicator), new PropertyMetadata(0.0, PropertyChangedCallback));

        public static readonly DependencyProperty IsReversedProperty = DependencyProperty.Register("IsReversed", typeof(bool), typeof(RelativeIndicator), new PropertyMetadata(false, PropertyChangedCallback));

        public RelativeIndicator()
        {
            this.InitializeComponent();
        }

        public bool IsReversed
        {
            get => (bool)GetValue(IsReversedProperty);
            set => SetValue(IsReversedProperty, value);
        }

        public double OldValue
        {
            get => (double)GetValue(OldValueProperty);
            set => SetValue(OldValueProperty, value);
        }

        public double NewValue
        {
            get => (double)GetValue(NewValueProperty);
            set => SetValue(NewValueProperty, value);
        }
        
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (RelativeIndicator) d;
            if (Math.Abs(that.OldValue) < 0.1)
            {
                that.PART_Text.Text = "no difference";
                that.PART_Icon.Visibility = Visibility.Collapsed;
                return;
            }

            if (that.OldValue < that.NewValue)
            {
                that.PART_Text.Text = $"+{Math.Round(100.0 * (that.NewValue - that.OldValue) / that.OldValue, 2):0.00}%";
                that.PART_Icon.Visibility = Visibility.Visible;
                that.PART_Icon.Fill = that.IsReversed ? Brushes.Red : Brushes.Green;
                ((RotateTransform)that.PART_Icon.RenderTransform).Angle = 0;
                return;
            }

            that.PART_Text.Text = $"-{Math.Round(100.0 * (that.OldValue - that.NewValue) / that.OldValue, 2):0.00}%";
            that.PART_Icon.Visibility = Visibility.Visible;
            that.PART_Icon.Fill = that.IsReversed ? Brushes.Green : Brushes.Red;
            ((RotateTransform) that.PART_Icon.RenderTransform).Angle = 180;
        }
    }
}
