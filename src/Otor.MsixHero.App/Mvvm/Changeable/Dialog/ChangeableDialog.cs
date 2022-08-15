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
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;

namespace Otor.MsixHero.App.Mvvm.Changeable.Dialog
{
    [TemplatePart(Name = "PART_CopyCommandLine")]
    [TemplatePart(Name = "PART_CopyCommandLineFinishScreen")]
    public class ChangeableDialog : Control
    {
        static ChangeableDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChangeableDialog),  new FrameworkPropertyMetadata(typeof(ChangeableDialog)));
        }

        public static readonly DependencyProperty OkButtonVisibilityProperty =  DependencyProperty.Register("OkButtonVisibility", typeof(Visibility), typeof(ChangeableDialog), new PropertyMetadata(Visibility.Visible));
        
        public static readonly DependencyProperty SuccessContentTemplateProperty = DependencyProperty.Register("SuccessContentTemplate", typeof(DataTemplate), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty SuccessContentTemplateSelectorProperty = DependencyProperty.Register("SuccessContentTemplateSelector", typeof(DataTemplateSelector), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty SuccessContentProperty = DependencyProperty.Register("SuccessContent", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentTemplateProperty = DependencyProperty.Register("DialogContentTemplate", typeof(DataTemplate), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentTemplateSelectorProperty = DependencyProperty.Register("DialogContentTemplateSelector", typeof(DataTemplateSelector), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentProperty = DependencyProperty.Register("DialogContent", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Geometry), typeof(ChangeableDialog), new PropertyMetadata(Geometry.Empty));

        public static readonly DependencyProperty OkButtonLabelProperty = DependencyProperty.Register("OkButtonLabel", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));
        
        public static readonly DependencyProperty CloseButtonLabelProperty = DependencyProperty.Register("CloseButtonLabel", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogProperty = DependencyProperty.Register("Dialog", typeof(ChangeableDialogViewModel), typeof(ChangeableDialog), new PropertyMetadata(null));
        
        public static readonly DependencyProperty FooterProperty =  DependencyProperty.Register("Footer", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowShieldProperty = DependencyProperty.Register("ShowShield", typeof(bool), typeof(ChangeableDialog), new PropertyMetadata(false));

        public static readonly DependencyProperty SupportsCommandLineBuildingProperty = DependencyProperty.Register("SupportsCommandLineBuilding", typeof(bool), typeof(ChangeableDialog), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowHeaderProperty = DependencyProperty.Register("ShowHeader", typeof(bool), typeof(ChangeableDialog), new PropertyMetadata(false));

        public static readonly DependencyProperty SilentCommandLineProperty = DependencyProperty.Register("SilentCommandLine", typeof(string), typeof(ChangeableDialog), new PropertyMetadata(null));
        
        public static readonly DependencyProperty ShowSilentCommandLineProperty = DependencyProperty.Register("ShowSilentCommandLine", typeof(bool), typeof(ChangeableDialog), new PropertyMetadata(false));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.GetTemplateChild("PART_CopyCommandLine") is ButtonBase button1)
            {
                button1.Click += (_, _) =>
                {
                    Clipboard.SetText(this.SilentCommandLine);
                };
            }

            if (this.GetTemplateChild("PART_CopyCommandLineFinishScreen") is ButtonBase button2)
            {
                button2.Click += (_, _) =>
                {
                    Clipboard.SetText(this.SilentCommandLine);
                };
            }
        }

        public bool SupportsCommandLineBuilding
        {
            get => (bool)GetValue(SupportsCommandLineBuildingProperty);
            set => SetValue(SupportsCommandLineBuildingProperty, value);
        }

        public bool ShowSilentCommandLine
        {
            get => (bool)GetValue(ShowSilentCommandLineProperty);
            set => SetValue(ShowSilentCommandLineProperty, value);
        }

        public string SilentCommandLine
        {
            get => (string)GetValue(SilentCommandLineProperty);
            set => SetValue(SilentCommandLineProperty, value);
        }

        public ChangeableDialogViewModel Dialog
        {
            get => (ChangeableDialogViewModel)this.GetValue(DialogProperty);
            set => this.SetValue(DialogProperty, value);
        }

        public T FindDialogTemplateName<T>(string name)
        {
            if (this.DialogContentTemplate == null)
            {
                return default;
            }

            var contentControl = this.GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
            if (contentControl == null)
            {
                return default;
            }

            return (T)this.DialogContentTemplate.FindName(name, contentControl);
        }
        
        public bool ShowShield
        {
            get => (bool)this.GetValue(ShowShieldProperty);
            set => this.SetValue(ShowShieldProperty, value);
        }

        public object Footer
        {
            get => this.GetValue(FooterProperty);
            set => this.SetValue(FooterProperty, value);
        }

        public Visibility OkButtonVisibility
        {
            get => (Visibility)this.GetValue(OkButtonVisibilityProperty);
            set => this.SetValue(OkButtonVisibilityProperty, value);
        }

        public Geometry Icon
        {
            get => (Geometry)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public bool ShowHeader
        {
            get => (bool)this.GetValue(ShowHeaderProperty);
            set => this.SetValue(ShowHeaderProperty, value);
        }

        public object OkButtonLabel
        {
            get => this.GetValue(OkButtonLabelProperty);
            set => this.SetValue(OkButtonLabelProperty, value);
        }

        public object CloseButtonLabel
        {
            get => this.GetValue(CloseButtonLabelProperty);
            set => this.SetValue(CloseButtonLabelProperty, value);
        }

        public DataTemplate SuccessContentTemplate
        {
            get => (DataTemplate)this.GetValue(SuccessContentTemplateProperty);
            set => this.SetValue(SuccessContentTemplateProperty, value);
        }

        public DataTemplateSelector SuccessContentTemplateSelector
        {
            get => (DataTemplateSelector)this.GetValue(SuccessContentTemplateSelectorProperty);
            set => this.SetValue(SuccessContentTemplateSelectorProperty, value);
        }

        public DataTemplate DialogContentTemplate
        {
            get => (DataTemplate)this.GetValue(DialogContentTemplateProperty);
            set => this.SetValue(DialogContentTemplateProperty, value);
        }

        public DataTemplateSelector DialogContentTemplateSelector
        {
            get => (DataTemplateSelector)this.GetValue(DialogContentTemplateSelectorProperty);
            set => this.SetValue(DialogContentTemplateSelectorProperty, value);
        }
    }
}