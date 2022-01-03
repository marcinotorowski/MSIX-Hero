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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Otor.MsixHero.Infrastructure.Extensions
{
    public static class FileExtensions
    {
        public static FileInfo ToFullPath(this FileInfo fileInfo, params string[] basePaths)
        {
            if (fileInfo.Exists)
            {
                return fileInfo;
            }
        
            var paths = new List<string>();
            if (basePaths != null && basePaths.Any())
            {
                paths.AddRange(basePaths);
            }
    
            var result = paths.Distinct(StringComparer.OrdinalIgnoreCase).Select(item => new FileInfo(Path.Combine(item, fileInfo.Name))).FirstOrDefault(item => item.Exists);
            if (result != null)
            {
                return result;
            }
        
            var existingPath = GetAbsolutePath(fileInfo.Name);
            return existingPath != null ? new FileInfo(existingPath) : fileInfo;
        }

        /// <summary>
        /// Gets the paths.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <returns>
        /// The paths.
        /// </returns>
        public static string GetAbsolutePath(string fileName, string workingDirectory = null)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var paths = new List<string>();
            if (workingDirectory != null)
            {
                paths.Add(Path.Combine(workingDirectory, fileName));
            }

            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), fileName));

            var is64Bit = IntPtr.Size == 8;

            if (is64Bit)
            {
                paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), fileName));
            }

            paths.Add(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));

            var path = Environment.GetEnvironmentVariable("PATH");
            if (path != null)
            {
                paths.AddRange(path.Split(';').Select(p => Path.Combine(p.Trim(), fileName)));
            }

            if (is64Bit)
            {
                paths.AddRange(GetAppPaths(fileName, true));
            }

            paths.AddRange(GetAppPaths(fileName, false));

            return paths.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item) && File.Exists(item));
        }

        private static IEnumerable<string> GetAppPaths(string fileName, bool is64Bit)
        {
            const string appPathKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

            try
            {
                using var registry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, is64Bit ? RegistryView.Registry64 : RegistryView.Registry32);
                using var key = registry.OpenSubKey(appPathKey + "\\" + fileName, false);
                
                if (key == null)
                {
                    return Enumerable.Empty<string>();
                }

                var paths = new List<string>();
                var defaultValue = key.GetValue(null) as string;
                if (!string.IsNullOrWhiteSpace(defaultValue))
                {
                    defaultValue = defaultValue.Trim('"');
                    if (!string.IsNullOrWhiteSpace(defaultValue))
                    {
                        paths.Add(defaultValue);
                    }
                }

                var pathValue = key.GetValue("Path") as string;
                if (!string.IsNullOrWhiteSpace(pathValue))
                {
                    paths.AddRange(pathValue.Split(';').Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => Path.Combine(item, fileName)));
                }

                return paths;
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}