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

using System.IO;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

public abstract class MakeAppxOptions
{
    protected MakeAppxOptions(FileSystemInfo source, FileSystemInfo target, bool validate = false)
    {
        this.Source = source;
        this.Target = target;
        this.Validate = validate;
    }

    protected MakeAppxOptions()
    {
    }

    public FileSystemInfo Source { get; set; }

    public FileSystemInfo Target { get; set; }

    public bool Validate { get; set; }

    public bool Verbose { get; set; } = true;

    public bool Overwrite { get; set; } = true;
}