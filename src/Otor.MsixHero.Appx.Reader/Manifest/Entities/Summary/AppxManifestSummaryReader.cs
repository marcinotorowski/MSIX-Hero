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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapplo.Log;
using Otor.MsixHero.Appx.Common;
using Otor.MsixHero.Appx.Common.Enums;
using Otor.MsixHero.Appx.Reader.File;

namespace Otor.MsixHero.Appx.Reader.Manifest.Entities.Summary
{
    public static class AppxManifestSummaryReader
    {
        private static readonly LogSource Logger = new();        

        public static async Task<AppxManifestSummary> FromMsix(string fullMsixFilePath, ReadMode mode = ReadMode.Minimal)
        {
            Logger.Info().WriteLine("Reading application manifest from {0}…", fullMsixFilePath);

            if (!System.IO.File.Exists(fullMsixFilePath))
            {
                throw new FileNotFoundException("MSIX file does not exist.", fullMsixFilePath);
            }
            
            using var reader = FileReaderFactory.CreateFileReader(fullMsixFilePath);
            var package = reader.GetFile(AppxFileConstants.AppxManifestFile);
            return await FromManifest(package, mode).ConfigureAwait(false);
        }      

        public static async Task<AppxManifestSummary> FromMsix(IAppxFileReader msixFileReader, ReadMode mode = ReadMode.Minimal)
        {
            var package = msixFileReader.GetFile(AppxFileConstants.AppxManifestFile);
            return await FromManifest(package, mode).ConfigureAwait(false);
        }

        public static Task<AppxManifestSummary> FromInstallLocation(string installLocation, ReadMode mode = ReadMode.Minimal)
        {
            Logger.Debug().WriteLine("Reading application manifest from install location {0}…", installLocation);
            if (!Directory.Exists(installLocation))
            {
                throw new DirectoryNotFoundException("Install location " + installLocation + " not found.");
            }

            return FromManifest(Path.Join(installLocation, AppxFileConstants.AppxManifestFile), mode);
        }

        public static async Task<AppxManifestSummary> FromManifest(string fullManifestPath, ReadMode mode = ReadMode.Minimal)
        {
            if (!System.IO.File.Exists(fullManifestPath))
            {
                throw new FileNotFoundException("Manifest file does not exist.", fullManifestPath);
            }

            await using var fs = System.IO.File.OpenRead(fullManifestPath);
            return await FromManifest(fs, mode).ConfigureAwait(false);
        }

        private static async Task<AppxManifestSummary> FromManifest(Stream manifestStream, ReadMode mode)
        {
            var result = new AppxManifestSummary();
            
            Logger.Debug().WriteLine("Loading XML file…");
            var xmlDocument = await XDocument.LoadAsync(manifestStream, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
            
            var identity = AppxIdentityReader.GetIdentityFromPackageManifest(xmlDocument);
            result.Name = identity.Name;
            result.Version = identity.Version;
            result.Publisher = identity.Publisher;
            result.Architectures = identity.Architectures;
            
            XNamespace win10Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace appxNamespace = "http://schemas.microsoft.com/appx/2010/manifest";
            XNamespace uapNamespace = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
            XNamespace uap3Namespace = "http://schemas.microsoft.com/appx/manifest/uap/windows10/3";

            var packageNode = xmlDocument.Element(win10Namespace + "Package") ?? xmlDocument.Element(appxNamespace + "Package");
            if (packageNode == null)
            {
                throw new ArgumentException("The manifest file does not contain a valid root element (<Package />).");
            }
            
            if ((mode & ReadMode.Properties) == ReadMode.Properties)
            {
                var propertiesNode = packageNode.Element(win10Namespace + "Properties") ?? packageNode.Element(appxNamespace + "Properties");
                Logger.Verbose().WriteLine("Executing XQuery /*[local-name()='Package']/*[local-name()='Properties'] for a single node…");
               
                if (propertiesNode != null)
                {
                    foreach (var subNode in propertiesNode.Elements())
                    {
                        switch (subNode.Name.LocalName)
                        {
                            case "DisplayName":
                                result.DisplayName = subNode.Value;
                                break;
                            case "PublisherDisplayName":
                                result.DisplayPublisher = subNode.Value;
                                break;
                            case "Description":
                                result.Description = subNode.Value;
                                break;
                            case "Logo":
                                result.Logo = subNode.Value;
                                break;
                            case "Framework":
                                result.IsFramework = bool.TryParse(subNode.Value, out var parsed) && parsed;
                                break;
                        }
                    }
                }

                var applicationsNode = packageNode.Element(win10Namespace + "Applications") ?? packageNode.Element(appxNamespace + "Applications");
                if (applicationsNode != null)
                {
                    var applicationNode = applicationsNode.Elements(win10Namespace + "Application").Concat(applicationsNode.Elements(appxNamespace + "Application"));
                    var visualElementsNode = applicationNode
                        .SelectMany(e => e.Elements(win10Namespace + "VisualElements")
                        .Concat(e.Elements(appxNamespace + "VisualElements"))
                        .Concat(e.Elements(uap3Namespace + "VisualElements"))
                        .Concat(e.Elements(uapNamespace + "VisualElements")));
                    
                    var background = visualElementsNode.Select(node => node.Attribute("BackgroundColor")).FirstOrDefault(a => a != null);
                    result.AccentColor = background?.Value ?? "Transparent";
                }
                else
                {
                    result.AccentColor = "Transparent";
                }
            }

            if ((mode & ReadMode.Applications) == ReadMode.Applications)
            {
                result.PackageType = result.IsFramework ? MsixApplicationType.Framework : 0;
                
                var applicationsNode = packageNode.Element(win10Namespace + "Applications") ?? packageNode.Element(appxNamespace + "Applications");
                if (applicationsNode != null)
                {
                    var applicationNode = applicationsNode.Elements(win10Namespace + "Application").Concat(applicationsNode.Elements(appxNamespace + "Application"));

                    foreach (var subNode in applicationNode)
                    {
                        var entryPoint = subNode.Attribute("EntryPoint")?.Value;
                        var executable = subNode.Attribute("Executable")?.Value;
                        var startPage = subNode.Attribute("StartPage")?.Value;
                        result.PackageType |= PackageTypeConverter.GetPackageTypeFrom(entryPoint, executable, startPage, result.IsFramework);
                    }
                }
            }

            Logger.Debug().WriteLine("Manifest information parsed.");
            return result;
        }

        [Flags]
        public enum ReadMode
        {
            Applications = 2 << 0,
            Properties = 2 << 1,
            Minimal = Applications | Properties
        }
    }
}