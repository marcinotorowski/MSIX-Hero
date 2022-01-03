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

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public class FileReaderFactory
    {
        public static IAppxFileReader CreateFileReader(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                if (!Directory.Exists(path))
                {
                    throw new FileNotFoundException("File not found.", path);
                }

                return new DirectoryInfoFileReaderAdapter(path);
            }

            var fileName = Path.GetFileName(path);
            if (string.Equals(FileConstants.AppxManifestFile, fileName, StringComparison.OrdinalIgnoreCase))
            {
                return new FileInfoFileReaderAdapter(path);
            }

            var ext = Path.GetExtension(path);
            switch (ext.ToLowerInvariant())
            {
                case FileConstants.MsixExtension:
                case FileConstants.AppxExtension:
                    return new ZipArchiveFileReaderAdapter(path);
                default:
                    return new FileInfoFileReaderAdapter(path);
            }
        }
    }
}
