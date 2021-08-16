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

namespace Otor.MsixHero.Appx.Packaging.ManifestCreator
{
    public readonly struct CreatedItem
    {
        public CreatedItem(string sourcePath, string packageRelativePath, ItemType type)
        {
            this.SourcePath = sourcePath;
            this.PackageRelativePath = packageRelativePath;
            this.Type = type;
        }

        public string SourcePath { get; }

        public string PackageRelativePath { get; }

        public ItemType Type { get; }

        public static CreatedItem CreateManifest(string sourcePath)
        {
            return new CreatedItem(sourcePath, FileConstants.AppxManifestFile, ItemType.Manifest);
        }

        public static CreatedItem CreateRegistry(string sourcePath, string relativePath)
        {
            return new CreatedItem(sourcePath, relativePath, ItemType.Registry);
        }

        public static CreatedItem CreateAsset(string sourcePath, string relativePath)
        {
            return new CreatedItem(sourcePath, relativePath, ItemType.Asset);
        }

        public enum ItemType
        {
            Manifest,
            Asset,
            Registry
        }
    }
}