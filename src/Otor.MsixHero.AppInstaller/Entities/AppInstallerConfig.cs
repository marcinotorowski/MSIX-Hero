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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    public class AppInstallerConfig
    {
        public AppInstallerConfig()
        {
        }

        public AppInstallerConfig(AppInstallerConfig config)
        {
            this.MainPackage = config.MainPackage;
            this.MainBundle = config.MainBundle;
            this.Version = config.Version;
            this.Dependencies = config.Dependencies;
            this.Uri = config.Uri;
            this.Optional = config.Optional;
            this.Related = config.Related;
            this.UpdateSettings = config.UpdateSettings;
        }
        
        [XmlElement("MainPackage")]
        public AppInstallerPackageEntry MainPackage { get; set; }

        [XmlElement("MainBundle")]
        public AppInstallerBundleEntry MainBundle { get; set; }

        [XmlAttribute]
        public string Uri { get; set; }

        [XmlAttribute]
        public string Version { get; set; }
            
        /// <summary>
        /// Defines the optional packages that will be installed along with the main package.
        /// </summary>
        [XmlArray("OptionalPackages")]
        [XmlArrayItem("Bundle", typeof(AppInstallerBundleEntry))]
        [XmlArrayItem("Package", typeof(AppInstallerPackageEntry))]
        public List<AppInstallerBaseEntry> Optional { get; set; }
        
        [XmlArray("RelatedPackages")]
        [XmlArrayItem("Bundle", typeof(AppInstallerBundleEntry))]
        [XmlArrayItem("Package", typeof(AppInstallerPackageEntry))]
        public List<AppInstallerBaseEntry> Related { get; set; }
            
        [XmlArray("Dependencies")]
        [XmlArrayItem("Bundle", typeof(AppInstallerBundleEntry))]
        [XmlArrayItem("Package", typeof(AppInstallerPackageEntry))]
        public List<AppInstallerBaseEntry> Dependencies { get; set; }
            
        [XmlElement("UpdateSettings")]
        public UpdateSettings UpdateSettings { get; set; }

        public static async Task<AppInstallerConfig> FromFile(string filePath)
        {
            var allText = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);

            var hasBom = allText.Take(3).Select(b => (ushort)b).SequenceEqual(new[] {(ushort)0xEF, (ushort)0xBB, (ushort)0xBF });
            if (hasBom)
            {
                allText = allText.Skip(3).ToArray();
            }

            var xml = System.Text.Encoding.UTF8.GetString(allText);

            var doc = XDocument.Parse(xml);
            if (doc.Root == null)
            {
                throw new FormatException("XML file does not have a root element.");
            }
            if (string.IsNullOrEmpty(doc.Root.Name.NamespaceName) || !doc.Root.Name.NamespaceName.StartsWith("http://schemas.microsoft.com/appx/appinstaller/"))
            {
                throw new FormatException("Root element must have appinstaller namespace.");
            }

            using var memStream = new MemoryStream(allText);
            switch (doc.Root.Name.NamespaceName)
            {
                case "http://schemas.microsoft.com/appx/appinstaller/2017":
                    return await AppInstallerConfig2017.FromStream(memStream).ConfigureAwait(false);
                case "http://schemas.microsoft.com/appx/appinstaller/2017/2":
                    return await AppInstallerConfig20172.FromStream(memStream).ConfigureAwait(false);
                case "http://schemas.microsoft.com/appx/appinstaller/2018":
                    return await AppInstallerConfig2018.FromStream(memStream).ConfigureAwait(false);
                default:
                    throw new FormatException("Namespace " + doc.Root.Name.NamespaceName + " is not supported.");
            }
        }
    }
}
