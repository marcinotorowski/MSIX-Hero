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

using System;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;
using Otor.MsixHero.Infrastructure.Localization;

namespace Otor.MsixHero.App.Localization;

public class LocExtension : MarkupExtension
{
    public string StringName { get; set; }

    public LocExtension(string stringName)
    {
        this.StringName = stringName;
    }

    public LocExtension()
    {
    }

    private static ResourceManager GetResourceManager(object control)
    {
        if (control is not DependencyObject dependencyObject)
        {
            return null;
        }

        var localValue = dependencyObject.ReadLocalValue(Translation.ResourceManagerProperty);

        // does this control have a "Translation.ResourceManager" attached property with a set value?
        if (localValue == DependencyProperty.UnsetValue || localValue is not ResourceManager resourceManager)
        {
            return null;
        }

        MsixHeroTranslation.Instance.AddResourceManager(resourceManager);
        return resourceManager;

    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        // targetObject is the control that is using the LocExtension
        object targetObject = (serviceProvider as IProvideValueTarget)?.TargetObject;

        if (targetObject?.GetType().Name == "SharedDp") // is extension used in a control template?
            return targetObject; // required for template re-binding

        string baseName = GetResourceManager(targetObject)?.BaseName ?? string.Empty;

        if (string.IsNullOrEmpty(baseName))
        {
            // rootObject is the root control of the visual tree (the top parent of targetObject)
            object rootObject = (serviceProvider as IRootObjectProvider)?.RootObject;
            baseName = GetResourceManager(rootObject)?.BaseName ?? string.Empty;
        }

        if (string.IsNullOrEmpty(baseName)) // template re-binding
        {
            if (targetObject is FrameworkElement frameworkElement)
            {
                baseName = GetResourceManager(frameworkElement.TemplatedParent)?.BaseName ?? string.Empty;
            }
        }

        var binding = new Binding
        {
            Mode = BindingMode.OneWay,
            Path = new PropertyPath($"[{baseName}.{StringName}]"),
            Source = MsixHeroTranslation.Instance,
            FallbackValue = StringName
        };

        return binding.ProvideValue(serviceProvider);
    }
}