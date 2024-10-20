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
using System.IO;
using Otor.MsixHero.Appx.Common;
using Otor.MsixHero.Appx.Reader.File.Adapters;
using Otor.MsixHero.Appx.Reader.Helpers;

namespace Otor.MsixHero.Appx.Reader.File
{
    public class FileReaderFactory
    {
        public static IAppxFileReader CreateFileReader(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!System.IO.File.Exists(path))
            {
                if (!Directory.Exists(path))
                {
                    throw new FileNotFoundException(Resources.Localization.Packages_Error_FileNotFound, path);
                }

                return new DirectoryInfoFileReaderAdapter(path);
            }

            var fileName = Path.GetFileName(path);
            if (string.Equals(AppxFileConstants.AppxManifestFile, fileName, StringComparison.OrdinalIgnoreCase))
            {
                return new FileInfoFileReaderAdapter(path);
            }

            var ext = Path.GetExtension(path);
            switch (ext.ToLowerInvariant())
            {
                case ".msix":
                case ".appx":
                    if (ZipFileChecker.IsZipFile(path))
                    {
                        return new ZipArchiveFileReaderAdapter(path);
                    }

                    throw new InvalidOperationException("Not an MSIX/APPX file.");

                default:
                    if (ZipFileChecker.IsZipFile(path))
                    {
                        return new ZipArchiveFileReaderAdapter(path);
                    }

                    return new FileInfoFileReaderAdapter(path);
            }
        }
    }
}
