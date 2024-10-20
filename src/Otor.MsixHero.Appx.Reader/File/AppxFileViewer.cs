﻿// MSIX Hero
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
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Appx.Reader.File
{
    public class AppxFileViewer : IAppxFileViewer
    {
        private DirectoryInfo _tempDirectory;

        public async Task<string> GetDiskPath(IAppxFileReader fileReader, string filePath)
        {
            if (!fileReader.FileExists(filePath))
            {
                throw new FileNotFoundException();
            }

            await using var fileStream = fileReader.GetFile(filePath);
            if (fileStream is FileStream fs)
            {
                // we can assume the file is directly accessible
                return fs.Name;
            }

            var index = 0;
            string fullPath;

            _tempDirectory ??= new DirectoryInfo(Path.Combine(Path.GetTempPath(), "MSIX-Hero", Guid.NewGuid().ToString("N").Substring(10)));

            while (true)
            {
                fullPath = Path.Combine(_tempDirectory.FullName, index.ToString("0"), Path.GetFileName(filePath));

                if (!System.IO.File.Exists(fullPath))
                {
                    break;
                }

                index++;
            }

            var fileInfo = new FileInfo(fullPath);
            if (fileInfo.Directory?.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            await using var targetStream = System.IO.File.OpenWrite(fileInfo.FullName);
            await fileStream.CopyToAsync(targetStream).ConfigureAwait(false);
            return fileInfo.FullName;
        }

        ~AppxFileViewer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ReSharper disable once UnusedParameter.Local
        private void Dispose(bool disposing)
        {
            if (_tempDirectory?.Exists == true)
            {
                ExceptionGuard.Guard(() => _tempDirectory.Delete(true));
                _tempDirectory = null;
            }
        }
    }
}