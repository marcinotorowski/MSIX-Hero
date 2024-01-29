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

using System.Resources;
using System.Windows;

namespace Otor.MsixHero.App.Localization;

public class Translation : DependencyObject
{
    public static readonly DependencyProperty ResourceManagerProperty =
        DependencyProperty.RegisterAttached("ResourceManager", typeof(ResourceManager), typeof(Translation));

    public static ResourceManager GetResourceManager(DependencyObject dependencyObject)
    {
        return (ResourceManager)dependencyObject.GetValue(ResourceManagerProperty);
    }

    public static void SetResourceManager(DependencyObject dependencyObject, ResourceManager value)
    {
        dependencyObject.SetValue(ResourceManagerProperty, value);
    }
}