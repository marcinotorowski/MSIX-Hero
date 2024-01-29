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
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels;
using Otor.MsixHero.Appx.Diagnostic.Events.Entities;

namespace Otor.MsixHero.App.Modules.EventViewer.Converters
{
    internal class EventLogIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventViewModel logViewModel)
            {
                switch (logViewModel.Type)
                {
                    case AppxEventType.Warning:
                        return this.IconWarning;
                    case AppxEventType.Error:
                    case AppxEventType.Critical:
                        return this.IconError;
                    case AppxEventType.Verbose:
                        return this.IconVerbose;
                }
            }

            return this.IconInformation;
        }

        public Geometry IconError { get; set; }

        public Geometry IconWarning { get; set; }

        public Geometry IconInformation { get; set; }

        public Geometry IconVerbose { get; set; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
