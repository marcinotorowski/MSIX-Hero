// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public class ZipArchiveFileReaderAdapter : IAppxFileReader
    {
        private readonly string msixPackagePath;
        private ZipArchive msixPackage;
        private IDisposable[] disposableStreams;

        public ZipArchiveFileReaderAdapter(ZipArchive msixPackage, string msixPackagePath = null)
        {
            this.PackagePath = msixPackagePath;
            this.msixPackage = msixPackage;
        }

        public ZipArchiveFileReaderAdapter(string msixPackagePath)
        {
            this.PackagePath = msixPackagePath;
            this.msixPackagePath = msixPackagePath;
        }

        public string PackagePath { get; private set; }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.EnsureInitialized();

            var entry = msixPackage.GetEntry(filePath.Replace("\\", "/"));
            if (entry == null)
            {
                entry = msixPackage.Entries.FirstOrDefault(e => string.Equals(e.FullName, filePath.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                {
                    throw new FileNotFoundException($"File {filePath} not found in MSIX package.");
                }
            }

            return entry.Open();
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            this.EnsureInitialized();

            if (this.FileExists(resourceFilePath))
            {
                return this.GetFile(resourceFilePath);
            }

            var resourceDir = Path.GetDirectoryName(resourceFilePath) + "/";
            var resourceFileName = Path.GetFileName(resourceFilePath);

            foreach (var item in this.msixPackage.Entries)
            {
                var currentName = item.FullName;
                if (resourceDir.Length > 1)
                {
                    if (!currentName.StartsWith(resourceDir, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    currentName = currentName.Remove(0, resourceDir.Length);
                }

                // 1) Remove quantified folder names
                currentName = Regex.Replace(currentName, @"[^\.\-]+-[^\.\-]+[\\/]", string.Empty);
                
                // 2) Remove quantified file names
                currentName = Regex.Replace(currentName, @"\.[^\.\-]+-[^\.\-]+", string.Empty);

                if (string.Equals(currentName, resourceFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Open();
                }
            }

            return null;
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            this.EnsureInitialized();

            filePath = filePath.Replace("\\", "/");
            var entry = this.msixPackage.GetEntry(filePath);
            if (entry != null)
            {
                return true;
            }

            return this.msixPackage.Entries.Any(e => string.Equals(filePath, e.FullName, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            if (this.disposableStreams == null)
            {
                return;
            }

            foreach (var item in this.disposableStreams)
            {
                item.Dispose();
            }
        }
        private void EnsureInitialized()
        {
            if (this.msixPackage != null)
            {
                return;
            }

            if (!File.Exists(this.msixPackagePath))
            {
                throw new ArgumentException($"File { this.msixPackagePath} does not exist.");
            }

            var fileStream = File.OpenRead(msixPackagePath);

            try
            {
                this.msixPackage = new ZipArchive(fileStream);
                this.disposableStreams = new IDisposable[] {this.msixPackage, fileStream};
            }
            catch (InvalidDataException e)
            {
                this.disposableStreams = new IDisposable[] { fileStream };
                throw new InvalidDataException("This file is not an MSIX/APPX package, or the content of the package is damaged.", e);
            }
            catch (Exception)
            {
                this.disposableStreams = new IDisposable[] { fileStream };
                throw;
            }
        }
    }
}