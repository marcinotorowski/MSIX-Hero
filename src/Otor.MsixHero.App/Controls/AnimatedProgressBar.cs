// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace Otor.MsixHero.App.Controls
{
    public class AnimatedProgressBar : DependencyObject
    {
        public static readonly DependencyProperty NonAnimatedProgressProperty = DependencyProperty.RegisterAttached("NonAnimatedProgress", typeof(int), typeof(AnimatedProgressBar), new PropertyMetadata(0));
        
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.RegisterAttached("Progress", typeof(int), typeof(AnimatedProgressBar), new PropertyMetadata(0, PropertyChangedCallback));
        
        public static int GetProgress(DependencyObject obj)
        {
            return (int)obj.GetValue(ProgressProperty);
        }

        public static void SetProgress(DependencyObject obj, int value)
        {
            obj.SetValue(ProgressProperty, value);
        }

        public static int GetNonAnimatedProgress(DependencyObject obj)
        {
            return (int)obj.GetValue(NonAnimatedProgressProperty);
        }

        public static void SetNonAnimatedProgress(DependencyObject obj, int value)
        {
            obj.SetValue(NonAnimatedProgressProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressBar progressBar)
            {
                var diff = (int)e.NewValue - AnimatedProgressBar.GetNonAnimatedProgress(progressBar);
                AnimatedProgressBar.SetNonAnimatedProgress(progressBar, (int)e.NewValue);

                if (diff < 0)
                {
                    progressBar.BeginAnimation(RangeBase.ValueProperty, null);
                    progressBar.Value = (int)e.NewValue;
                }
                else if (diff < 4)
                {
                    progressBar.BeginAnimation(RangeBase.ValueProperty, null);
                    progressBar.Value = (int)e.NewValue;
                }
                else
                {
                    var story = new Storyboard();
                    var anim = new DoubleAnimation { To = (int)e.NewValue, Duration = TimeSpan.FromMilliseconds(300), AccelerationRatio = 0.5, DecelerationRatio = 0.5 };

                    Storyboard.SetTargetProperty(anim, new PropertyPath(RangeBase.ValueProperty));
                    Storyboard.SetTarget(anim, progressBar);

                    story.Children.Add(anim);
                    story.Freeze();
                    story.Begin();
                }
            }
            else if (d is CircularProgress circularProgress)
            {
                var diff = (int)e.NewValue - AnimatedProgressBar.GetNonAnimatedProgress(circularProgress);
                AnimatedProgressBar.SetNonAnimatedProgress(circularProgress, (int)e.NewValue);

                if (diff < 0)
                {
                    circularProgress.BeginAnimation(RangeBase.ValueProperty, null);
                    circularProgress.Value = (int)e.NewValue;
                }
                else if (diff < 4)
                {
                    circularProgress.BeginAnimation(RangeBase.ValueProperty, null);
                    circularProgress.Value = (int)e.NewValue;
                }
                else
                {
                    var story = new Storyboard();
                    var anim = new DoubleAnimation { To = (int)e.NewValue, Duration = TimeSpan.FromMilliseconds(300), AccelerationRatio = 0.5, DecelerationRatio = 0.5 };

                    Storyboard.SetTargetProperty(anim, new PropertyPath(CircularProgress.ValueProperty));
                    Storyboard.SetTarget(anim, circularProgress);

                    story.Children.Add(anim);
                    story.Freeze();
                    story.Begin();
                }
            }
        }
    }
}
