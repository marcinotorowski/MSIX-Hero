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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary
{
    public class AppxIdentityReader
    {
        public Task<AppxIdentity> GetIdentity(string filePath, CancellationToken cancellationToken = default)
        {
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".xml":
                    if (string.Equals("appxmanifest", Path.GetFileNameWithoutExtension(filePath), StringComparison.OrdinalIgnoreCase))
                    {
                        using var manifestStream = File.OpenRead(filePath);
                        return GetIdentityFromPackageManifest(manifestStream, cancellationToken);
                    }
                    else if (string.Equals("appxbundlemanifest", Path.GetFileNameWithoutExtension(filePath), StringComparison.OrdinalIgnoreCase))
                    {
                        using var manifestStream = File.OpenRead(filePath);
                        return GetIdentityFromBundleManifest(manifestStream, cancellationToken);
                    }
                    else
                    {
                        throw new ArgumentException($"File {Path.GetFileName(filePath)} is not supported.");
                    }
                
                case ".msix":
                case ".appx":
                    return GetIdentityFromPackage(filePath, cancellationToken);
                case ".appxbundle":
                case ".msixbundle":
                    return GetIdentityFromBundle(filePath, cancellationToken);
                default:
                    throw new ArgumentException($"File {Path.GetFileName(filePath)} is not supported.");
            }
        }

        private static async Task<AppxIdentity> GetIdentityFromPackage(string packagePath, CancellationToken cancellationToken = default)
        {
            using IAppxFileReader reader = new ZipArchiveFileReaderAdapter(packagePath);
            return await GetIdentityFromPackageManifest(reader.GetFile("AppxManifest.xml"), cancellationToken).ConfigureAwait(false);
        }

        private static async Task<AppxIdentity> GetIdentityFromBundle(string bundlePath, CancellationToken cancellationToken = default)
        {
            using IAppxFileReader reader = new ZipArchiveFileReaderAdapter(bundlePath);
            return await GetIdentityFromBundleManifest(reader.GetFile("AppxMetadata/AppxBundleManifest.xml"), cancellationToken).ConfigureAwait(false);
        }

        private static async Task<AppxIdentity> GetIdentityFromPackageManifest(Stream packageManifestStream, CancellationToken cancellationToken = default)
        {
            /*
             * <Package
             *    xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
                  <Identity Name="MSIXHero" Version="2.0.46.0" Publisher="CN=abc" ProcessorArchitecture="neutral" />
             */
            
            XNamespace rootNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

            var doc = await XDocument.LoadAsync(packageManifestStream, LoadOptions.None, cancellationToken).ConfigureAwait(false);

            var root = doc.Element(rootNamespace + "Package");
            if (root == null)
            {
                throw new InvalidDataException("Not a valid package manifest. Missing root element <Package />.");
            }
            
            var identity = root.Element(rootNamespace + "Identity");
            if (identity == null)
            {
                throw new InvalidDataException("Not a valid bundle manifest. Missing child element <Identity />.");
            }

            var version = identity.Attribute("Version");
            var publisher = identity.Attribute("Publisher");
            var name = identity.Attribute("Name");
            var architecture = identity.Attribute("ProcessorArchitecture");
            
            var appxIdentity = new AppxIdentity
            {
                Name = name?.Value,
                Publisher = publisher?.Value,
                Version = version?.Value
            };

            if (architecture != null && Enum.TryParse(architecture.Value, true, out AppxPackageArchitecture architectureValue))
            {
                appxIdentity.Architectures = new [] { architectureValue };
            }
            
            return appxIdentity;
        }

        private static async Task<AppxIdentity> GetIdentityFromBundleManifest(Stream bundleManifestStream, CancellationToken cancellationToken = default)
        {
            /*
             *<?xml version="1.0" encoding="UTF-8"?>
             * <Bundle xmlns:b4="http://schemas.microsoft.com/appx/2018/bundle" xmlns="http://schemas.microsoft.com/appx/2013/bundle" 
             */
            XNamespace rootNamespace = "http://schemas.microsoft.com/appx/2013/bundle";

            var doc = await XDocument.LoadAsync(bundleManifestStream, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            var root = doc.Element(rootNamespace + "Bundle");
            if (root == null)
            {
                throw new InvalidDataException("Not a valid bundle manifest. Missing root element <Bundle />.");
            }
            
            var identity = root.Element(rootNamespace + "Identity");
            if (identity == null)
            {
                throw new InvalidDataException("Not a valid bundle manifest. Missing child element <Identity />.");
            }

            var version = identity.Attribute("Version");
            var publisher = identity.Attribute("Publisher");
            var name = identity.Attribute("Name");

            var packages = root.Element(rootNamespace + "Packages")?.Elements(rootNamespace + "Package");

            var appxIdentity = new AppxIdentity
            {
                Name = name?.Value,
                Publisher = publisher?.Value,
                Version = version?.Value
            };
            
            if (packages != null)
            {
                var architectures = new HashSet<AppxPackageArchitecture>();

                foreach (var package in packages)
                {
                    var architecture = package.Attribute("Architecture");
                    if (architecture != null && Enum.TryParse(architecture.Value, true, out AppxPackageArchitecture architectureValue))
                    {
                        architectures.Add(architectureValue);
                    }
                }

                appxIdentity.Architectures = architectures.ToArray();
            }

            return appxIdentity;
        }
    }
}