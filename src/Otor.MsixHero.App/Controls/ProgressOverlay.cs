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
using System.Windows.Controls;
using System.Windows.Input;

namespace Otor.MsixHero.App.Controls
{
    public class ProgressOverlay : ContentControl
    {
        static ProgressOverlay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressOverlay), new FrameworkPropertyMetadata(typeof(ProgressOverlay)));
        }

        public static readonly DependencyProperty ProgressProperty =  DependencyProperty.Register("Progress", typeof(double), typeof(ProgressOverlay), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MessageProperty =  DependencyProperty.Register("Message", typeof(string), typeof(ProgressOverlay), new PropertyMetadata(MsixHero.App.Resources.Localization.Loading_PleaseWait));

        public static readonly DependencyProperty IsShownProperty =  DependencyProperty.Register("IsShown", typeof(bool), typeof(ProgressOverlay), new PropertyMetadata(false));
        
        public static readonly DependencyProperty HideContentWhileLoadingProperty =  DependencyProperty.Register("HideContentWhileLoading", typeof(bool), typeof(ProgressOverlay), new PropertyMetadata(false));

        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(ProgressOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty SupportsCancellingProperty =  DependencyProperty.Register("SupportsCancelling", typeof(bool), typeof(ProgressOverlay), new PropertyMetadata(false));
        
        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public bool SupportsCancelling
        {
            get => (bool)GetValue(SupportsCancellingProperty);
            set => SetValue(SupportsCancellingProperty, value);
        }

        public double Progress
        {
            get => (double)this.GetValue(ProgressProperty);
            set => this.SetValue(ProgressProperty, value);
        }
        
        public string Message
        {
            get => (string)this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }


        public bool IsShown
        {
            get => (bool)this.GetValue(IsShownProperty);
            set => this.SetValue(IsShownProperty, value);
        }


        public bool HideContentWhileLoading
        {
            get => (bool)this.GetValue(HideContentWhileLoadingProperty);
            set => this.SetValue(HideContentWhileLoadingProperty, value);
        }
    }
}
