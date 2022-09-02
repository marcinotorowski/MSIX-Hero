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
using System.IO;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public static class SdkPathHelper
    {
        public static string GetSdkPath(string localName, string baseDirectory = null)
        {
            return GetSdkPath(localName, IntPtr.Size == 4, baseDirectory);
        }

        public static string GetSdkPath(string localName, bool use32Bit, string baseDirectory = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var baseDir = baseDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "sdk");
            var path = Path.Combine(baseDir, use32Bit ? "x86" : "x64", localName);
            if (!File.Exists(path))
            {
                path = Path.Combine(baseDir, localName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_MissingSdk_Format, path), path);
                }
            }

            return path;
        }
        public static string GetPsfDirectory(string baseDirectory = null)
        {
            return GetPsfDirectory(IntPtr.Size == 4, baseDirectory);
        }

        public static string GetPsfDirectory(bool use32Bit, string baseDirectory = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var baseDir = baseDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "psf");
            var path = Path.Combine(baseDir, use32Bit ? "x86" : "x64");
            if (!Directory.Exists(path))
            {
                path = Path.Combine(baseDir);
                if (!Directory.Exists(path))
                {
                    throw new DirectoryNotFoundException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_MissingSdk_Format, path));
                }
            }

            return path;
        }
    }
}