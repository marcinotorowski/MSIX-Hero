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
using System.Windows.Controls.Primitives;

namespace Otor.MsixHero.App.Controls.Cards;

public class CardAction : ButtonBase, ILoadingCard
{
    public static readonly DependencyProperty ShowArrowProperty = DependencyProperty.Register(nameof(ShowArrow), typeof(bool), typeof(CardAction), new PropertyMetadata(true));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(CardAction), new PropertyMetadata(null));

    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(CardAction), new PropertyMetadata(false));
    
    public static readonly DependencyProperty LoadingContentTemplateProperty = DependencyProperty.Register("LoadingContentTemplate", typeof(DataTemplate), typeof(CardAction), new PropertyMetadata(null));

    public DataTemplate LoadingContentTemplate
    {
        get => (DataTemplate)GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
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

    public bool ShowArrow
    {
        get => (bool)GetValue(ShowArrowProperty);
        set => SetValue(ShowArrowProperty, value);
    }
}