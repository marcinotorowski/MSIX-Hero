// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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

using System.Windows;

namespace Otor.MsixHero.App.Helpers.Behaviors
{
    public class InteractionEx : DependencyObject
    {
        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("Behaviors", typeof(BehaviorCollectionEx), typeof(InteractionEx), new PropertyMetadata(null, OnBehaviorsChanged));

        public static BehaviorCollectionEx GetBehaviors(DependencyObject obj)
        {
            var behaviorCollection = (BehaviorCollectionEx)obj.GetValue(BehaviorsProperty);
            return behaviorCollection;
        }
        
        public static void SetBehaviors(DependencyObject obj, BehaviorCollectionEx value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }

        private static void OnBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldBehaviorCollection = (BehaviorCollectionEx)e.OldValue;
            var newBehaviorCollection = (BehaviorCollectionEx)e.NewValue;

            if (oldBehaviorCollection == newBehaviorCollection)
            {
                return;
            }

            oldBehaviorCollection?.DetachAll();
            newBehaviorCollection?.SetParent((FrameworkElement)d);
        }
    }
}
