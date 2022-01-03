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

namespace Otor.MsixHero.App.Controls
{
    public class ZeroOrAny : Control
    {
        public static readonly DependencyProperty NoItemsTemplateProperty =  DependencyProperty.Register("NoItemsTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));
        public static readonly DependencyProperty AnyItemTemplateProperty =  DependencyProperty.Register("AnyItemTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));
        public static readonly DependencyProperty OneItemTemplateProperty =  DependencyProperty.Register("OneItemTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));
        public static readonly DependencyProperty ManyItemsTemplateProperty =  DependencyProperty.Register("ManyItemsTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));
        public static readonly DependencyProperty ItemsCountProperty =  DependencyProperty.Register("ItemsCount", typeof(int), typeof(ZeroOrAny), new PropertyMetadata(0, OnItemsChanged));
        public static readonly DependencyProperty ActualTemplateProperty = DependencyProperty.Register("ActualTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));

        public DataTemplate ActualTemplate
        {
            get => (DataTemplate)GetValue(ActualTemplateProperty);
            set => this.SetValue(ActualTemplateProperty, value);
        }

        public int ItemsCount
        {
            get => (int)GetValue(ItemsCountProperty);
            set => this.SetValue(ItemsCountProperty, value);
        }

        public DataTemplate NoItemsTemplate
        {
            get => (DataTemplate)this.GetValue(NoItemsTemplateProperty);
            set => this.SetValue(NoItemsTemplateProperty, value);
        }


        public DataTemplate OneItemTemplate
        {
            get => (DataTemplate)this.GetValue(OneItemTemplateProperty);
            set => this.SetValue(OneItemTemplateProperty, value);
        }

        public DataTemplate ManyItemsTemplate
        {
            get => (DataTemplate)this.GetValue(ManyItemsTemplateProperty);
            set => this.SetValue(ManyItemsTemplateProperty, value);
        }

        public DataTemplate AnyItemTemplate
        {
            get => (DataTemplate)this.GetValue(AnyItemTemplateProperty);
            set => this.SetValue(AnyItemTemplateProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.SetTemplate();
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (ZeroOrAny)d;
            that.SetTemplate();
        }

        private void SetTemplate()
        {
            var colItemsCount = this.ItemsCount;

            if (colItemsCount == 0)
            {
                this.ActualTemplate = this.NoItemsTemplate;
            }
            else if (colItemsCount == 1)
            {
                if (this.OneItemTemplate != null)
                {
                    this.ActualTemplate = this.OneItemTemplate;
                }
                else
                {
                    this.ActualTemplate = this.AnyItemTemplate;
                }
            }
            else
            {
                if (this.ManyItemsTemplate != null)
                {
                    this.ActualTemplate = this.ManyItemsTemplate;
                }
                else
                {
                    this.ActualTemplate = this.AnyItemTemplate;
                }
            }
        }
    }
}
