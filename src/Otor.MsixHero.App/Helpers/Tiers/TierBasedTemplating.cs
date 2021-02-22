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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using Otor.MsixHero.App.Helpers.Behaviors;

namespace Otor.MsixHero.App.Helpers.Tiers
{
    public class TierBasedTemplating : Behavior<Control>, ISupportInitialize
    {
        private static readonly ReaderWriterLockSlim LockObject = new ReaderWriterLockSlim();

        private bool initializing;
        private static readonly IList<WeakReference<Control>> AllSubscribedElements = new List<WeakReference<Control>>();

        public static readonly DependencyProperty Tier2Property = DependencyProperty.Register("Tier2", typeof(ControlTemplate), typeof(TierBasedTemplating), new PropertyMetadata(default(ControlTemplate), TierTemplateChanged));
        public static readonly DependencyProperty Tier1Property = DependencyProperty.Register("Tier1", typeof(ControlTemplate), typeof(TierBasedTemplating), new PropertyMetadata(default(ControlTemplate), TierTemplateChanged));
        public static readonly DependencyProperty Tier0Property = DependencyProperty.Register("Tier0", typeof(ControlTemplate), typeof(TierBasedTemplating), new PropertyMetadata(default(ControlTemplate), TierTemplateChanged));


        public ControlTemplate Tier2
        {
            get => (ControlTemplate)GetValue(Tier2Property);
            set => SetValue(Tier2Property, value);
        }

        public ControlTemplate Tier1
        {
            get => (ControlTemplate)GetValue(Tier1Property);
            set => SetValue(Tier1Property, value);
        }

        public ControlTemplate Tier0
        {
            get => (ControlTemplate)GetValue(Tier0Property);
            set => SetValue(Tier0Property, value);
        }
        
        void ISupportInitialize.BeginInit()
        {
            this.initializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            this.initializing = false;
            SetBestTemplate(this.AssociatedObject);
        }

        public static void Reevaluate()
        {
            SetTemplateOnAllElements();
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

            AllSubscribedElements.Add(new WeakReference<Control>(this.AssociatedObject));
            base.OnAttached();
            SetBestTemplate(this);
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
        
        private static void SetBestTemplate(DependencyObject frameworkElement)
        {
            if (frameworkElement == null)
            {
                return;
            }

            var behavior = InteractionEx.GetBehaviors(frameworkElement).OfType<TierBasedTemplating>().FirstOrDefault();
            if (behavior == null)
            {
                // This should not happen, but let's be 100% sure of not throwing exceptions
                return;
            }

            SetBestTemplate(behavior);
        }

        private static void SetBestTemplate(TierBasedTemplating behavior)
        {
            var associatedObject = behavior.AssociatedObject;

            var tier = TierController.GetCurrentTier();
            if (tier == 2)
            {
                if (behavior.Tier2 != null)
                {
                    associatedObject.Template = behavior.Tier2;
                    return;
                }

                tier = 1;
            }

            if (tier == 1)
            {
                if (behavior.Tier1 != null)
                {
                    associatedObject.Template = behavior.Tier1;
                    return;
                }
            }

            if (behavior.Tier0 != null)
            {
                associatedObject.Template = behavior.Tier0;
            }
        }

        private static void SetTemplateOnAllElements()
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

                    SetBestTemplate(obj);
                }
            }
            finally
            {
                LockObject.ExitUpgradeableReadLock();
            }
        }
        
        private static void TierTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((TierBasedTemplating)d).initializing)
            {
                return;
            }

            SetBestTemplate((TierBasedTemplating)d);
        }
    }
}