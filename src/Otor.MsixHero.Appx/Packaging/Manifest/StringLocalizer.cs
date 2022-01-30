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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PriFormat;

namespace Otor.MsixHero.Appx.Packaging.Manifest
{
    public static class StringLocalizer
    {
        public static string Localize(string priFile, string appId, string packageFullName, string resourceId)
        {
            if (resourceId == null || !resourceId.StartsWith("ms-resource:"))
            {
                return resourceId;
            }

            if (!File.Exists(priFile))
            {
                return resourceId;
            }

            var outBuff = new StringBuilder(1024);


            resourceId = resourceId.Remove(0, "ms-resource:".Length);
            if (string.IsNullOrEmpty(resourceId))
            {
                return resourceId;
            }

            string fullString;

            if (resourceId.StartsWith("//"))
            {
                fullString = "@{" + priFile + "?ms-resource:" + resourceId.TrimEnd('/') + "}";
                if (SHLoadIndirectString(fullString, outBuff, outBuff.Capacity, IntPtr.Zero) == 0)
                {
                    return outBuff.ToString();
                }
            }

            string msResource;
            if (resourceId[0] == '/')
            {
                var split = resourceId.Split('/');
                var newResourceId = string.Join('/', split.Take(2)) + "/" + string.Join('.', split.Skip(2));
                msResource = "ms-resource://" + appId + newResourceId.TrimEnd('/');
            }
            else if (resourceId.IndexOf('/') != -1)
            {
                var split = resourceId.Split('/');
                var newResourceId = string.Join('/', split.Take(1)) + "/" + string.Join('.', split.Skip(1));
                msResource = "ms-resource://" + appId + "/" + newResourceId.TrimEnd('/');
            }
            else
            {
                var split = resourceId.Split('/');
                var newResourceId = string.Join('/', split.Take(1)) + "/" + string.Join('.', split.Skip(1));
                msResource = "ms-resource://" + appId + "/resources/" + newResourceId.TrimEnd('/') ;
            }

            fullString = "@{" + priFile + "?" + msResource + "}";

            if (SHLoadIndirectString(fullString, outBuff, outBuff.Capacity, IntPtr.Zero) == 0)
            {
                return outBuff.ToString();
            }

            fullString = "@{" + packageFullName + "?" + msResource + "}";
            if (SHLoadIndirectString(fullString, outBuff, outBuff.Capacity, IntPtr.Zero) == 0)
            {
                return outBuff.ToString();
            }

            using var s = File.OpenRead(priFile);
            var pri= PriFile.Parse(s);
            var name = pri.Sections.OfType<HierarchicalSchemaSection>().FirstOrDefault(s => s.UniqueName != null);
            if (name != null)
            {
                msResource = "ms-resource://" + name.UniqueName.Substring(name.UniqueName.IndexOf("://", StringComparison.OrdinalIgnoreCase) + 3) + "Resources/" + resourceId.TrimEnd('/');
                fullString = "@{" + priFile + "?" + msResource + "}";

                if (SHLoadIndirectString(fullString, outBuff, outBuff.Capacity, IntPtr.Zero) == 0)
                {
                    return outBuff.ToString();
                }

                msResource = "ms-resource://" + name.UniqueName.Substring(name.UniqueName.IndexOf("://", StringComparison.OrdinalIgnoreCase) + 3) + resourceId.TrimEnd('/');
                fullString = "@{" + priFile + "?" + msResource + "}";

                if (SHLoadIndirectString(fullString, outBuff, outBuff.Capacity, IntPtr.Zero) == 0)
                {
                    return outBuff.ToString();
                }
            }

            return resourceId;
        }
        
        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);
    }
}
