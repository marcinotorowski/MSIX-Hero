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

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

public class MakeAppxPackOptions : MakeAppxOptions
{
    public MakeAppxPackOptions(FileSystemInfo source, FileSystemInfo target, bool validate = false, bool compress = true) : base(source, target, validate)
    {
        this.Compress = compress;
    }

    public MakeAppxPackOptions(FileSystemInfo source, FileSystemInfo target) : base(source, target)
    {
    }

    public MakeAppxPackOptions()
    {
    }
    
    public string PublisherBridge { get; set; }

    public bool Compress { get; set; }

    public static MakeAppxPackOptions CreateFromMapping(FileInfo sourceMappingFile, FileInfo targetPackagePath, bool compress = true, bool validate = false)
    {
        if (sourceMappingFile == null)
        {
            throw new ArgumentNullException(nameof(sourceMappingFile));
        }

        if (targetPackagePath == null)
        {
            throw new ArgumentNullException(nameof(targetPackagePath));
        }

        return new MakeAppxPackOptions(sourceMappingFile, targetPackagePath, validate)
        {
            Compress = compress
        };
    }

    public static MakeAppxPackOptions CreateFromMapping(FileInfo sourceMappingFile, string targetPackagePath, bool compress = true, bool validate = false)
    {
        return CreateFromMapping(sourceMappingFile, new FileInfo(targetPackagePath), validate, compress);
    }

    public static MakeAppxPackOptions CreateFromMapping(string sourceMappingFile, string targetPackagePath, bool compress = true, bool validate = false)
    {
        if (sourceMappingFile == null)
        {
            throw new ArgumentNullException(nameof(sourceMappingFile));
        }

        if (targetPackagePath == null)
        {
            throw new ArgumentNullException(nameof(targetPackagePath));
        }

        return CreateFromMapping(new FileInfo(sourceMappingFile), new FileInfo(targetPackagePath), validate, compress);
    }

    public static MakeAppxPackOptions CreateFromDirectory(DirectoryInfo sourceDirectory, FileInfo targetPackagePath, bool compress = true, bool validate = false)
    {
        if (sourceDirectory == null)
        {
            throw new ArgumentNullException(nameof(sourceDirectory));
        }

        if (targetPackagePath == null)
        {
            throw new ArgumentNullException(nameof(targetPackagePath));
        }

        return new MakeAppxPackOptions(sourceDirectory, targetPackagePath, validate)
        {
            Compress = compress
        };
    }

    public static MakeAppxPackOptions CreateFromDirectory(DirectoryInfo sourceDirectory, string targetPackagePath, bool compress = true, bool validate = false)
    {
        if (sourceDirectory == null)
        {
            throw new ArgumentNullException(nameof(sourceDirectory));
        }

        if (targetPackagePath == null)
        {
            throw new ArgumentNullException(nameof(targetPackagePath));
        }

        return CreateFromDirectory(sourceDirectory, new FileInfo(targetPackagePath), validate, compress);
    }

    public static MakeAppxPackOptions CreateFromDirectory(string sourceDirectory, string targetPackagePath, bool compress = true, bool validate = false)
    {
        if (sourceDirectory == null)
        {
            throw new ArgumentNullException(nameof(sourceDirectory));
        }

        if (targetPackagePath == null)
        {
            throw new ArgumentNullException(nameof(targetPackagePath));
        }

        return CreateFromDirectory(new DirectoryInfo(sourceDirectory), new FileInfo(targetPackagePath), validate, compress);
    }
}