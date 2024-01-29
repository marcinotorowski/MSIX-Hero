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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Otor.MsixHero.Appx.Psf.Entities.Interpreter;
using Otor.MsixHero.Appx.Psf.Entities.Interpreter.Redirection;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.View.Psf;

public class InterpretedRegexCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IList<InterpretedRegex> values)
        {
            return Binding.DoNothing;
        }

        var stringBuilder = new StringBuilder();

        for (var i = 0; i < values.Count; i++)
        {
            var regex = values[i];

            if (i > 0)
            {
                stringBuilder.Append(i + 1 < values.Count ? ", " : " and ");
            }

            switch (regex.Comparison)
            {
                case RegexInterpretationResult.Any:
                    stringBuilder.AppendFormat(Resources.Localization.Psf_Regex_AllFiles, regex.Target);
                    break;
                case RegexInterpretationResult.Extension:
                    if (i > 0 && values[i - 1].Comparison == RegexInterpretationResult.Extension)
                    {
                        stringBuilder.AppendFormat("**{0}**", regex.Target);
                    }
                    else
                    {
                        stringBuilder.AppendFormat(Resources.Localization.Psf_Regex_FileWithExtension, regex.Target);
                    }
                    break;
                case RegexInterpretationResult.Name:
                    stringBuilder.AppendFormat(Resources.Localization.Psf_Regex_File, regex.Target);
                    break;
                case RegexInterpretationResult.Custom:
                    stringBuilder.AppendFormat(Resources.Localization.Psf_Regex_FilesMatching, regex.Target);
                    break;
                case RegexInterpretationResult.StartsWith:
                    stringBuilder.AppendFormat(Resources.Localization.Psf_Regex_FilesStartingWith, regex.Target);
                    break;
                case RegexInterpretationResult.EndsWith:
                    stringBuilder.AppendFormat(Resources.Localization.Psf_Regex_FilesEndingWith, regex.Target);
                    break;
                default:
                    continue;
            }
        }

        return stringBuilder.ToString().Replace("**", "");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}