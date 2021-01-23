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

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Mvvm.Changeable;

namespace Otor.MsixHero.App.Helpers.Validation
{
    public class ValidationBehavior : BehaviorBase<TabItem>
    {
        public static readonly DependencyProperty ValidatedChangeableProperty = DependencyProperty.Register("ValidatedChangeable", typeof(IValidatedChangeable), typeof(ValidationBehavior), new PropertyMetadata(null, ChangeableChanged));

        public IValidatedChangeable ValidatedChangeable
        {
            get => (IValidatedChangeable)GetValue(ValidatedChangeableProperty);
            set => SetValue(ValidatedChangeableProperty, value);
        }

        private static void ChangeableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (ValidationBehavior)d;
            me.HandleChange((IValidatedChangeable) e.OldValue, (IValidatedChangeable) e.NewValue);
        }

        private void HandleChange(IValidatedChangeable oldValue, IValidatedChangeable newValue)
        {
            if (oldValue != null)
            {
                oldValue.ValidationStatusChanged -= this.OnValidationStatusChanged;
                oldValue.PropertyChanged -= this.OnPropertyChanged;

                if (newValue == null)
                {
                    ValidatedTabItem.SetIsValid(this.AssociatedObject, true);
                    ValidatedTabItem.SetValidationMessage(this.AssociatedObject, null);
                }
            }

            if (newValue != null)
            {
                newValue.ValidationStatusChanged -= this.OnValidationStatusChanged;
                newValue.ValidationStatusChanged += this.OnValidationStatusChanged;

                newValue.PropertyChanged -= this.OnPropertyChanged;
                newValue.PropertyChanged += this.OnPropertyChanged;

                this.SetValues(newValue);
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null || e.PropertyName == nameof(IDataErrorInfo.Error) || e.PropertyName == nameof(IValidatedChangeable.DisplayValidationErrors))
            {
                this.SetValues((IValidatedChangeable)sender);
            }
        }

        private void OnValidationStatusChanged(object sender, ValueChangedEventArgs<string> e)
        {
            var obj = (IValidatedChangeable)sender;
            this.SetValues(obj);
        }

        private void SetValues(IValidatedChangeable src)
        {
            if (this.AssociatedObject == null)
            {
                return;
            }

            if (src.IsValidated && src.DisplayValidationErrors)
            {
                ValidatedTabItem.SetIsValid(this.AssociatedObject, string.IsNullOrEmpty(src.ValidationMessage));
                ValidatedTabItem.SetValidationMessage(this.AssociatedObject, src.ValidationMessage);
            }
            else
            {
                ValidatedTabItem.SetIsValid(this.AssociatedObject, true);
                ValidatedTabItem.SetValidationMessage(this.AssociatedObject, null);
            }
        }
    }
}