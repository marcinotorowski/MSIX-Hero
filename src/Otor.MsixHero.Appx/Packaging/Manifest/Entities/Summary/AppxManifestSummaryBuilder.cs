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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary
{
    public class AppxManifestSummaryBuilder
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        
        public static async Task<AppxManifestSummary> FromMsix(string fullMsixFilePath, AppxManifestSummaryBuilderMode mode = AppxManifestSummaryBuilderMode.Minimal)
        {
            Logger.Info("Reading application manifest {0}...", fullMsixFilePath);

            if (!File.Exists(fullMsixFilePath))
            {
                throw new FileNotFoundException("MSIX file does not exist.", fullMsixFilePath);
            }

            try
            {
                Logger.Debug("Opening file as ZIP...");
                using var zipFile = ZipFile.OpenRead(fullMsixFilePath);

                Logger.Debug("Getting entry {0}...", FileConstants.AppxManifestFile);
                var entry = zipFile.GetEntry(FileConstants.AppxManifestFile);
                if (entry == null)
                {
                    throw new FileNotFoundException("Manifest file not found.");
                }

                await using var stream = entry.Open();
                return await FromManifest(stream, mode).ConfigureAwait(false);
            }
            catch (InvalidDataException e)
            {
                throw new InvalidOperationException("File " + fullMsixFilePath + " does not seem to be a valid MSIX package.", e);
            }
        }

        public static Task<AppxManifestSummary> FromInstallLocation(string installLocation, AppxManifestSummaryBuilderMode mode = AppxManifestSummaryBuilderMode.Minimal)
        {
            Logger.Debug("Reading application manifest from install location {0}...", installLocation);
            if (!Directory.Exists(installLocation))
            {
                throw new DirectoryNotFoundException("Install location " + installLocation + " not found.");
            }

            return FromManifest(Path.Join(installLocation, FileConstants.AppxManifestFile), mode);
        }

        public static async Task<AppxManifestSummary> FromManifest(string fullManifestPath, AppxManifestSummaryBuilderMode mode = AppxManifestSummaryBuilderMode.Minimal)
        {
            if (!File.Exists(fullManifestPath))
            {
                throw new FileNotFoundException("Manifest file does not exist.", fullManifestPath);
            }

            using (var fs = File.OpenRead(fullManifestPath))
            {
                return await FromManifest(fs, mode).ConfigureAwait(false);
            }
        }

        private static async Task<AppxManifestSummary> FromManifest(Stream manifestStream, AppxManifestSummaryBuilderMode mode)
        {
            var result = new AppxManifestSummary();
            var xmlDocument = new XmlDocument();
            var streamReader = new StreamReader(manifestStream);

            Logger.Debug("Loading XML file...");
            xmlDocument.LoadXml(await streamReader.ReadToEndAsync().ConfigureAwait(false));

            if ((mode & AppxManifestSummaryBuilderMode.Identity) == AppxManifestSummaryBuilderMode.Identity)
            {
                Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Identity'] for a single node...");
                var identity = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']");
            
                result.Name = identity?.Attributes?["Name"]?.Value;
                result.Version = identity?.Attributes?["Version"]?.Value;
                result.Publisher = identity?.Attributes?["Publisher"]?.Value;
                result.ProcessorArchitecture = identity?.Attributes?["ProcessorArchitecture"]?.Value;
            }

            if ((mode & AppxManifestSummaryBuilderMode.Properties) == AppxManifestSummaryBuilderMode.Properties)
            {
                Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Properties'] for a single node...");
                var node = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Properties']");

                if (node != null)
                {
                    foreach (var subNode in node.ChildNodes.OfType<XmlNode>())
                    {
                        switch (subNode.LocalName)
                        {
                            case "DisplayName":
                                result.DisplayName = subNode.InnerText;
                                break;
                            case "PublisherDisplayName":
                                result.DisplayPublisher = subNode.InnerText;
                                break;
                            case "Description":
                                result.Description = subNode.InnerText;
                                break;
                            case "Logo":
                                result.Logo = subNode.InnerText;
                                break;
                            case "Framework":
                                result.IsFramework = bool.TryParse(subNode.InnerText, out var parsed) && parsed;
                                break;
                        }
                    }
                }

                Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']/*[local-name()='VisualElements'] for a single node...");
                node = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']/*[local-name()='VisualElements']");
                result.AccentColor = node?.Attributes?["BackgroundColor"]?.Value ?? "Transparent";
            }

            if ((mode & AppxManifestSummaryBuilderMode.Applications) == AppxManifestSummaryBuilderMode.Applications)
            {
                result.PackageType = result.IsFramework ? MsixPackageType.Framework : 0;

                Logger.Trace("Executing XQuery /*[local-name()='Package']");
                var package = xmlDocument.SelectSingleNode("/*[local-name()='Package']");

                if (package != null)
                {
                    Logger.Trace("Executing XQuery /*[local-name()='Applications']/*[local-name()='Application']...");
                    var applications = package.SelectNodes("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']");

                    if (applications != null)
                    {
                        foreach (var subNode in applications.OfType<XmlNode>())
                        {
                            var entryPoint = subNode.Attributes?["EntryPoint"]?.Value;
                            var executable = subNode.Attributes?["Executable"]?.Value;
                            var startPage = subNode.Attributes?["StartPage"]?.Value;
                            result.PackageType |= PackageTypeConverter.GetPackageTypeFrom(entryPoint, executable, startPage, result.IsFramework);
                        }
                    }
                }
            }

            Logger.Debug("Manifest information parsed.");
            return result;
        }
    }
}