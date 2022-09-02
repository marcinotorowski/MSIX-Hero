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
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Helpers.Bitness;

public class FileBitnessReader
{
    public async Task<FileBitness> GetBitness(Stream fileStream, CancellationToken cancellationToken = default)
    {
        const int firstBytesToReadHeader = 4096;
        const int machineOffset = 4;
        const int portableExecutablePointerOffset = 60;

        var readFirstBytes = new byte[firstBytesToReadHeader];
        var actualReadBytes = await fileStream.ReadAsync(readFirstBytes, 0, readFirstBytes.Length, cancellationToken).ConfigureAwait(false);

        if (portableExecutablePointerOffset + sizeof(int) > actualReadBytes)
        {
            throw new ArgumentException("No PE header found.");
        }

        var portableExecutableHeaderAddress = BitConverter.ToInt32(readFirstBytes, portableExecutablePointerOffset);
        if (portableExecutableHeaderAddress + machineOffset + sizeof(short) > actualReadBytes)
        {
            throw new ArgumentException("No PE header found.");
        }

        var machineMagic = BitConverter.ToUInt16(readFirstBytes, portableExecutableHeaderAddress + machineOffset);
        return (FileBitness)machineMagic;
    }

    public async Task<FileBitness> GetBitness(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await this.GetBitness(stream, cancellationToken).ConfigureAwait(false);
    }
}