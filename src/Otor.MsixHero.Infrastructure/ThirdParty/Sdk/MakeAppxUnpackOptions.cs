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

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

public class MakeAppxUnpackOptions : MakeAppxOptions
{
    public MakeAppxUnpackOptions(FileInfo sourcePackage, DirectoryInfo targetDirectory, bool validate = false) : base(sourcePackage, targetDirectory, validate)
    {
    }

    public MakeAppxUnpackOptions()
    {
    }

    public static MakeAppxUnpackOptions Create(FileInfo sourcePackage, DirectoryInfo targetDirectory, bool validate = false)
    {
        if (sourcePackage == null)
        {
            throw new ArgumentNullException(nameof(sourcePackage));
        }

        if (targetDirectory == null)
        {
            throw new ArgumentNullException(nameof(targetDirectory));
        }

        return new MakeAppxUnpackOptions(sourcePackage, targetDirectory, validate);
    }

    public static MakeAppxUnpackOptions Create(string sourcePackage, string targetDirectory, bool validate = false)
    {
        if (sourcePackage == null)
        {
            throw new ArgumentNullException(nameof(sourcePackage));
        }

        if (targetDirectory == null)
        {
            throw new ArgumentNullException(nameof(targetDirectory));
        }

        return new MakeAppxUnpackOptions(new FileInfo(sourcePackage), new DirectoryInfo(targetDirectory), validate);
    }
}