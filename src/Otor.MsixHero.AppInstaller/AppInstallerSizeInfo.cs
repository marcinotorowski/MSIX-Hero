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
using System.Text;
using System.Threading.Tasks;
using Otor.MsixHero.AppInstaller.Entities;
using Otor.MsixHero.Appx.Packaging;

namespace Otor.MsixHero.AppInstaller;

public class AppInstallerSizeInfo
{
    private readonly FileInfo _fileInfo;

    public AppInstallerSizeInfo(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }

    public long FileSize => _fileInfo.Length;

    public static async Task<int> GetActualSize(AppInstallerConfig config)
    {
        var builder = new AppInstallerBuilder(config);
        var built = builder.Build(PackageType.Bundle);
        var text = await builder.Create(built).ConfigureAwait(false);
        var bytesOccupied = Encoding.UTF8.GetBytes(text).Length;
        return bytesOccupied;
    }

    /// <summary>
    /// This method takes an instance of AppInstaller config and returns a value, which includes extra padding at the end.
    /// It then returns a new size in bytes, which should be used as a padded size for a file in order to workaround an MSIX delivery bug.
    /// See https://github.com/MicrosoftDocs/msix-docs/issues/188#issuecomment-947
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="extraPadding">The extra padding to be added at the end of the file.</param>
    /// <param name="roundUp">Round the value to the nearest unit of this number.</param>
    /// <returns>The expected padded size.</returns>
    public static async Task<int> GetSuggestedPaddedSize(AppInstallerConfig config, int extraPadding = 20000, int roundUp = 1000)
    {
        var bytesOccupied = await GetActualSize(config).ConfigureAwait(false);
        return GetSuggestedPaddedSize(bytesOccupied, extraPadding, roundUp);
    }
    
    public static int GetSuggestedPaddedSize(int appInstallerSize, int extraPadding = 5000, int roundUp = 1000)
    {
        if (extraPadding > 0)
        {
            appInstallerSize += extraPadding;
        }

        if (roundUp < 2)
        {
            return appInstallerSize;
        }

        var excess = appInstallerSize % roundUp;
        if (excess == 0)
        {
            return appInstallerSize;
        }

        return appInstallerSize - excess + roundUp;
    }

    public async Task Pad(long size)
    {
        await using var stream = File.Open(_fileInfo.FullName, FileMode.Append);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);

        if (stream.Length < size)
        {
            await writer.WriteAsync(Environment.NewLine);
        }

        while (stream.Length < size)
        {
            await writer.WriteAsync(' ').ConfigureAwait(false);
        }

        await writer.FlushAsync().ConfigureAwait(false);
    }

    public async Task<bool> IsPadded()
    {
        var content = await File.ReadAllTextAsync(_fileInfo.FullName).ConfigureAwait(false);
        if (content.Length == 0)
        {
            return false;
        }

        var lastPositionOfClosing = content.LastIndexOf("</AppInstaller>", StringComparison.OrdinalIgnoreCase);
        if (lastPositionOfClosing + 1 == content.Length)
        {
            return false;
        }

        content = content.Remove(0, lastPositionOfClosing + "</AppInstaller>".Length);

        // Let's assume a single ending line is not an indication of padding. There must be at least some space, or more than one new line.
        if (content.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            content = content.Substring(0, content.LastIndexOf(Environment.NewLine, StringComparison.Ordinal));
        }

        return content.Length > 0;
    }
}