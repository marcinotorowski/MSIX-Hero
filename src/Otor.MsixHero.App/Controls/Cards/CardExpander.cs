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

namespace Otor.MsixHero.App.Controls.Cards;

public class CardExpander : ContentControl, ILoadingCard
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(object), typeof(CardExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(CardExpander), new PropertyMetadata(false));
    
    public static readonly DependencyProperty ContentRightTemplateProperty = DependencyProperty.Register(nameof(ContentRightTemplate), typeof(DataTemplate), typeof(CardExpander), new PropertyMetadata(null));
    
    public static readonly DependencyProperty ContentRightTemplateSelectorProperty = DependencyProperty.Register(nameof(ContentRightTemplateSelector), typeof(DataTemplateSelector), typeof(CardExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty ContentRightProperty = DependencyProperty.Register(nameof(ContentRight), typeof(object), typeof(CardExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty ContentExpandedTemplateProperty = DependencyProperty.Register(nameof(ContentExpandedTemplate), typeof(DataTemplate), typeof(CardExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty ContentExpandedTemplateSelectorProperty = DependencyProperty.Register(nameof(ContentExpandedTemplateSelector), typeof(DataTemplateSelector), typeof(CardExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty ContentExpandedProperty = DependencyProperty.Register(nameof(ContentExpanded), typeof(object), typeof(CardExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty VerticalContentRightAlignmentProperty = DependencyProperty.Register(nameof(VerticalContentRightAlignment), typeof(VerticalAlignment), typeof(CardExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty HorizontalContentRightAlignmentProperty = DependencyProperty.Register(nameof(HorizontalContentRightAlignment), typeof(HorizontalAlignment), typeof(CardExpander), new PropertyMetadata(null));
    
    public static readonly DependencyProperty VerticalContentExpandedAlignmentProperty = DependencyProperty.Register(nameof(VerticalContentExpandedAlignment), typeof(VerticalAlignment), typeof(CardExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty HorizontalContentExpandedAlignmentProperty = DependencyProperty.Register(nameof(HorizontalContentExpandedAlignment), typeof(HorizontalAlignment), typeof(CardExpander), new PropertyMetadata(null));
    
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(CardExpander), new PropertyMetadata(false));

    public static readonly DependencyProperty LoadingContentTemplateProperty = DependencyProperty.Register("LoadingContentTemplate", typeof(DataTemplate), typeof(CardExpander), new PropertyMetadata(null));

    public DataTemplate LoadingContentTemplate
    {
        get => (DataTemplate)GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
    }

    public DataTemplate ContentRightTemplate
    {
        get => (DataTemplate)GetValue(ContentRightTemplateProperty);
        set => SetValue(ContentRightTemplateProperty, value);
    }

    public DataTemplateSelector ContentRightTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ContentRightTemplateSelectorProperty);
        set => SetValue(ContentRightTemplateSelectorProperty, value);
    }

    public DataTemplate ContentExpandedTemplate
    {
        get => (DataTemplate)GetValue(ContentExpandedTemplateProperty);
        set => SetValue(ContentExpandedTemplateProperty, value);
    }

    public DataTemplateSelector ContentExpandedTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ContentExpandedTemplateSelectorProperty);
        set => SetValue(ContentExpandedTemplateSelectorProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public object ContentRight
    {
        get => GetValue(ContentRightProperty);
        set => SetValue(ContentRightProperty, value);
    }

    public object ContentExpanded
    {
        get => GetValue(ContentExpandedProperty);
        set => SetValue(ContentExpandedProperty, value);
    }

    public VerticalAlignment VerticalContentRightAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalContentRightAlignmentProperty);
        set => SetValue(VerticalContentRightAlignmentProperty, value);
    }

    public HorizontalAlignment HorizontalContentRightAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalContentRightAlignmentProperty);
        set => SetValue(HorizontalContentRightAlignmentProperty, value);
    }

    public VerticalAlignment VerticalContentExpandedAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalContentExpandedAlignmentProperty);
        set => SetValue(VerticalContentExpandedAlignmentProperty, value);
    }

    public HorizontalAlignment HorizontalContentExpandedAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalContentExpandedAlignmentProperty);
        set => SetValue(HorizontalContentExpandedAlignmentProperty, value);
    }
}