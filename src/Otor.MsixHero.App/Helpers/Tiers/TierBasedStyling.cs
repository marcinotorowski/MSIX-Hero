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
    public class TierBasedStyling : Behavior<FrameworkElement>, ISupportInitialize
    {
        private static readonly ReaderWriterLockSlim LockObject = new ReaderWriterLockSlim();

        private bool initializing;
        private static readonly IList<WeakReference<FrameworkElement>> AllSubscribedElements = new List<WeakReference<FrameworkElement>>();
        
        public static readonly DependencyProperty Tier2Property = DependencyProperty.Register("Tier2", typeof(Style), typeof(TierBasedStyling), new PropertyMetadata(default(Style), TierStyleChanged));
        public static readonly DependencyProperty Tier1Property = DependencyProperty.Register("Tier1", typeof(Style), typeof(TierBasedStyling), new PropertyMetadata(default(Style), TierStyleChanged));
        public static readonly DependencyProperty Tier0Property = DependencyProperty.Register("Tier0", typeof(Style), typeof(TierBasedStyling), new PropertyMetadata(default(Style), TierStyleChanged));

        public Style Tier2
        {
            get => (Style)GetValue(Tier2Property);
            set => SetValue(Tier2Property, value);
        }

        public Style Tier1
        {
            get => (Style)GetValue(Tier1Property);
            set => SetValue(Tier1Property, value);
        }

        public Style Tier0
        {
            get => (Style)GetValue(Tier0Property);
            set => SetValue(Tier0Property, value);
        }

        public static void Reevaluate()
        {
            SetStyleOnAllElements();
        }

        void ISupportInitialize.BeginInit()
        {
            this.initializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            this.initializing = false;
            SetBestStyle(this.AssociatedObject);
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
            SetBestStyle(this);
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

        private static void SetBestStyle(DependencyObject frameworkElement)
        {
            if (frameworkElement == null)
            {
                return;
            }
            
            var behavior = InteractionEx.GetBehaviors(frameworkElement).OfType<TierBasedStyling>().FirstOrDefault();
            if (behavior == null)
            {
                // This should not happen, but let's be 100% sure of not throwing exceptions
                return;
            }

            SetBestStyle(behavior);
        }

        private static void SetBestStyle(TierBasedStyling behavior)
        {
            var associatedObject = behavior.AssociatedObject;

            var tier = TierController.GetCurrentTier();
            if (tier == 2)
            {
                if (behavior.Tier2 != null)
                {
                    associatedObject.Style = behavior.Tier2;
                    return;
                }

                tier = 1;
            }

            if (tier == 1)
            {
                if (behavior.Tier1 != null)
                {
                    associatedObject.Style = behavior.Tier1;
                    return;
                }
            }
            
            if (behavior.Tier0 != null)
            {
                associatedObject.Style = behavior.Tier0;
            }
        }

        private static void SetStyleOnAllElements()
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

                    SetBestStyle(obj);
                }
            }
            finally
            {
                LockObject.ExitUpgradeableReadLock();
            }
        }

        private static void TierStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((TierBasedStyling)d).initializing)
            {
                return;
            }

            SetBestStyle((TierBasedStyling)d);
        }
    }
}
