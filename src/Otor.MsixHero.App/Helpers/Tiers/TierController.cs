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

namespace Otor.MsixHero.App.Helpers.Tiers
{
    public class TierController : DependencyObject
    {
        private static int _currentTier;
        
        private TierController()
        {
        }

        public static TierController Instance { get; } = new TierController();

        private static readonly DependencyPropertyKey CurrentTierPropertyKey = DependencyProperty.RegisterReadOnly("CurrentTier", typeof(int), typeof(TierController), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty CurrentTierProperty = CurrentTierPropertyKey.DependencyProperty;
        
        public int CurrentTier
        {
            get => (int) GetValue(CurrentTierProperty);
            private set => SetValue(CurrentTierPropertyKey, value);
        }
        
        public static int GetCurrentTier()
        {
            return _currentTier;
        }

        // Important to call somewhere at the beginning so that listening and initial tier is set-up.
        public static void ListenForSystemChanges()
        {
            System.Windows.Media.RenderCapability.TierChanged -= RenderCapabilityOnTierChanged;
            System.Windows.Media.RenderCapability.TierChanged += RenderCapabilityOnTierChanged;
        }

        private static void RenderCapabilityOnTierChanged(object sender, EventArgs e)
        {
            SetCurrentTier(_currentTier, false);
        }

        public static void SetCurrentTier(int tier, bool ignoreSystemChanges = true)
        {
            _currentTier = tier;
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Instance.CurrentTier = _currentTier;
                TierBasedVisibility.Reevaluate();
                TierBasedTemplating.Reevaluate();
                TierBasedStyling.Reevaluate();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Instance.CurrentTier = _currentTier;
                    TierBasedVisibility.Reevaluate();
                    TierBasedTemplating.Reevaluate();
                    TierBasedStyling.Reevaluate();
                });
            }
        }

        public static void SetSystemTier()
        {
            _currentTier = System.Windows.Media.RenderCapability.Tier >> 16;

            if (Application.Current?.Dispatcher == null || Application.Current.Dispatcher.CheckAccess())
            {
                Instance.CurrentTier = _currentTier;
                TierBasedVisibility.Reevaluate();
                TierBasedTemplating.Reevaluate();
                TierBasedStyling.Reevaluate();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Instance.CurrentTier = _currentTier;
                    TierBasedVisibility.Reevaluate();
                    TierBasedTemplating.Reevaluate();
                    TierBasedStyling.Reevaluate();
                });
            }
            
            ListenForSystemChanges();
        }
    }
}