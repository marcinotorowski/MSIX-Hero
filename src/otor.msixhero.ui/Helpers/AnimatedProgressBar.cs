using System;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace otor.msixhero.ui.Helpers
{
    public class AnimatedProgressBar : DependencyObject
    {
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.RegisterAttached("Progress", typeof(int), typeof(AnimatedProgressBar), new PropertyMetadata(0, PropertyChangedCallback));
        
        public static int GetProgress(DependencyObject obj)
        {
            return (int)obj.GetValue(ProgressProperty);
        }

        public static void SetProgress(DependencyObject obj, int value)
        {
            obj.SetValue(ProgressProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var objectParent = (ProgressBar)d;

            var diff = (int) e.NewValue - objectParent.Value;
            if (diff < 0)
            {
                objectParent.BeginAnimation(RangeBase.ValueProperty, null);
                objectParent.Value = (int)e.NewValue;
            }
            else if (Math.Abs(diff) < 5)
            {
                // objectParent.BeginAnimation(RangeBase.ValueProperty, null);
                // objectParent.Value = (int) e.NewValue;
            }
            else
            {
                var story = new Storyboard();
                var anim = new DoubleAnimation { To = (int)e.NewValue, Duration = TimeSpan.FromMilliseconds(300), AccelerationRatio = 0.5, DecelerationRatio = 0.5 };

                Storyboard.SetTargetProperty(anim, new PropertyPath(RangeBase.ValueProperty));
                Storyboard.SetTarget(anim, objectParent);

                story.Children.Add(anim);
                story.Freeze();
                story.Begin();
            }
        }
    }
}
