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
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Packer.Enums;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.Packer
{
    public interface IAppxPacker
    {
        Task Pack(string directory, string packagePath, AppxPackerOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
        
        Task PackFiles(string directory, string packagePath, AppxPackerOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
       
        Task Unpack(string packagePath, string directory, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
    }
}
