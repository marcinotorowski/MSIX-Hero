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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using Otor.MsixHero.App.Helpers.Behaviors;

namespace Otor.MsixHero.App.Helpers.Tiers
{
    public class TierBasedVisibility : Behavior<FrameworkElement>, ISupportInitialize
    {
        private static readonly ReaderWriterLockSlim LockObject = new ReaderWriterLockSlim();

        private bool initializing;

        private static readonly IList<WeakReference<FrameworkElement>> AllSubscribedElements = new List<WeakReference<FrameworkElement>>();

        public static readonly DependencyProperty Tier2Property = DependencyProperty.Register("Tier2", typeof(Visibility), typeof(TierBasedVisibility), new PropertyMetadata(Visibility.Visible, TierVisibilityChanged));
        public static readonly DependencyProperty Tier1Property = DependencyProperty.Register("Tier1", typeof(Visibility), typeof(TierBasedVisibility), new PropertyMetadata(Visibility.Visible, TierVisibilityChanged));
        public static readonly DependencyProperty Tier0Property = DependencyProperty.Register("Tier0", typeof(Visibility), typeof(TierBasedVisibility), new PropertyMetadata(Visibility.Visible, TierVisibilityChanged));

        public Visibility Tier2
        {
            get => (Visibility)GetValue(Tier2Property);
            set => SetValue(Tier2Property, value);
        }

        public Visibility Tier1
        {
            get => (Visibility)GetValue(Tier1Property);
            set => SetValue(Tier1Property, value);
        }

        public Visibility Tier0
        {
            get => (Visibility)GetValue(Tier0Property);
            set => SetValue(Tier0Property, value);
        }

        public static void Reevaluate()
        {
            SetVisibilityOnAllElements();
        }

        void ISupportInitialize.BeginInit()
        {
            this.initializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            this.initializing = false;
            SetBestVisibility(this.AssociatedObject);
        }

        protected override void OnAttached()
        {
            LockObject.EnterWriteLock();

            try
            {
                for (var i = AllSubscribedElements.Count - 1; i >= 0; i--)
                {
                    if (!AllSubscribedElements[i].TryGetTarget(out _))
                    {
                        AllSubscribedElements.RemoveAt(i);
                    }
                }
            }
            finally
            {
                LockObject.ExitWriteLock();
            }

            AllSubscribedElements.Add(new WeakReference<FrameworkElement>(this.AssociatedObject));
            base.OnAttached();
            SetBestVisibility(this);
        }

        protected override void OnDetaching()
        {
            LockObject.EnterWriteLock();

            try
            {
                for (var i = AllSubscribedElements.Count - 1; i >= 0; i--)
                {
                    var currentElement = AllSubscribedElements[i];
                    if (!currentElement.TryGetTarget(out var obj))
                    {
                        AllSubscribedElements.RemoveAt(i);
                        continue;
                    }

                    if (obj == this.AssociatedObject)
                    {
                        AllSubscribedElements.RemoveAt(i);
                        break;
                    }
                }
            }
            finally
            {
                LockObject.ExitWriteLock();
            }
            
            base.OnDetaching();
        }

        private static void SetBestVisibility(DependencyObject frameworkElement)
        {
            if (frameworkElement == null)
            {
                return;
            }
            
            var behavior = InteractionEx.GetBehaviors(frameworkElement).OfType<TierBasedVisibility>().FirstOrDefault();
            if (behavior == null)
            {
                // This should not happen, but let's be 100% sure of not throwing exceptions
                return;
            }

            SetBestVisibility(behavior);
        }

        private static void SetBestVisibility(TierBasedVisibility behavior)
        {
            var associatedObject = behavior.AssociatedObject;

            var tier = TierController.GetCurrentTier();

            switch (tier)
            {
                case 2:
                    associatedObject.Visibility = behavior.Tier2;
                    break;
                case 1:
                    associatedObject.Visibility = behavior.Tier1;
                    break;
                default:
                    associatedObject.Visibility = behavior.Tier0;
                    break;
            }
        }

        private static void SetVisibilityOnAllElements()
        {
            LockObject.EnterUpgradeableReadLock();
            try
            {
                for (var i = AllSubscribedElements.Count - 1; i >= 0; i--)
                {
                    if (!AllSubscribedElements[i].TryGetTarget(out var obj))
                    {
                        LockObject.EnterWriteLock();
                        try
                        {
                            AllSubscribedElements.RemoveAt(i);
                            continue;
                        }
                        finally
                        {
                            LockObject.ExitWriteLock();
                        }
                    }

                    SetBestVisibility(obj);
                }
            }
            finally
            {
                LockObject.ExitUpgradeableReadLock();
            }
        }

        private static void TierVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((TierBasedVisibility)d).initializing)
            {
                return;
            }

            SetBestVisibility((TierBasedVisibility)d);
        }
    }
}
