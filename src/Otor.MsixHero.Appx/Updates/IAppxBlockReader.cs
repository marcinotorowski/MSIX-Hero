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

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities.Blocks;
using Otor.MsixHero.Appx.Updates.Serialization.ComparePackage;

namespace Otor.MsixHero.Appx.Updates
{
    public interface IAppxBlockReader
    {
        Task<IList<Block>> ReadBlocks(IAppxFileReader fileReader, CancellationToken cancellationToken = default);

        Task<IList<Block>> ReadBlocks(FileInfo file, CancellationToken cancellationToken = default);

        void SetBlocks(ICollection<Block> oldBlocks, ICollection<Block> newBlocks, SdkComparePackage comparedPackage);
    }
}
