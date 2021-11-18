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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Infrastructure.Branding
{
    public class MsixHeroBrandingInjector
    {
        public enum BrandingInjectorOverrideOption
        {
            Default, // will prefer existing with exception of MsixHero, makeappx.exe and signtool.exe which must be taken over from the current toolset
            PreferExisting, // will prefer existing values and never overwrite anything with exception of MsixHero
            PreferIncoming // will replace existing values with new ones
        }

        public void Inject(XDocument modifiedDocument, BrandingInjectorOverrideOption overwrite = BrandingInjectorOverrideOption.Default)
        {
            XNamespace windows10Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace appxNamespace = "http://schemas.microsoft.com/appx/2010/manifest";
            XNamespace buildNamespace = XNamespace.Get("http://schemas.microsoft.com/developer/appx/2015/build");

            if (modifiedDocument.Root == null)
            {
                throw new ArgumentException("The XML file is corrupted. Root element is not present.");
            }

            var package = modifiedDocument.Element(windows10Namespace + "Package") ?? modifiedDocument.Element(appxNamespace + "Package");
            if (package == null)
            {
                return;
            }

            var namespaceAdded = false;
            
            var originalBuildNamespace = modifiedDocument.Root.Attribute(XNamespace.Xmlns + "build");
            if (originalBuildNamespace != null)
            {
                buildNamespace = originalBuildNamespace.Value;
            }
            else
            {
                modifiedDocument.Root.SetAttributeValue(XNamespace.Xmlns + "build", buildNamespace);
                namespaceAdded = true;
            }
            
            if (namespaceAdded)
            {
                var ignorable = modifiedDocument.Root.Attribute("IgnorableNamespaces");
                if (ignorable == null)
                {
                    modifiedDocument.Root.Add(new XAttribute("IgnorableNamespaces", "build"));
                }
                else
                {
                    modifiedDocument.Root.SetAttributeValue("IgnorableNamespaces", string.Join(" ", ignorable.Value.Split(' ').Union(new[] { "build" })));
                }
            }

            var metaData = package.Element(buildNamespace + "Metadata");
            
            if (metaData == null)
            {
                metaData = new XElement(buildNamespace + "Metadata");
                package.Add(metaData);
            }

            var operatingSystemVersion = NdDll.RtlGetVersion();
            var msixHeroVersion = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version;
            
            // MSIX-Hero specific lines:
            InjectComponentAndVersion(metaData, buildNamespace, "MsixHero", msixHeroVersion);

            // Generic lines:
            InjectComponentAndVersion(metaData, buildNamespace, "OperatingSystem", operatingSystemVersion, overwrite == BrandingInjectorOverrideOption.PreferIncoming);

            // SDK lines:
            InjectComponentAndVersion(metaData, buildNamespace, "SignTool.exe", overwrite != BrandingInjectorOverrideOption.PreferExisting);
            InjectComponentAndVersion(metaData, buildNamespace, "MakePri.exe", overwrite == BrandingInjectorOverrideOption.PreferIncoming);
            InjectComponentAndVersion(metaData, buildNamespace, "MakeAppx.exe", overwrite != BrandingInjectorOverrideOption.PreferExisting);
        }

        private static void InjectComponentAndVersion(XElement metadata, XNamespace buildNamespace, string name, Version version, bool overwriteExisting = true)
        {
            InjectComponentAndVersion(metadata, buildNamespace, name, version?.ToString(4), overwriteExisting);
        }

        private static void InjectComponentAndVersion(XElement metadata, XNamespace buildNamespace, string sdkFileName, bool overwriteExisting = true)
        {
            InjectComponentAndVersion(metadata, buildNamespace, sdkFileName, GetVersion(sdkFileName), overwriteExisting);
        }

        private static void InjectComponentAndVersion(XElement metadata, XNamespace buildNamespace, string name, string version, bool overwriteExisting = true)
        {
            var node = metadata.Elements(buildNamespace + "Item").FirstOrDefault(item => string.Equals(item.Attribute("Name")?.Value, name, StringComparison.OrdinalIgnoreCase));

            if (node == null && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
                node = metadata.Elements(buildNamespace + "Item").FirstOrDefault(item => string.Equals(item.Attribute("Name")?.Value, fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            }

            if (node == null)
            {
                node = new XElement(buildNamespace + "Item");
                node.Add(new XAttribute("Name", name));
                if (version != null)
                {
                    node.Add(new XAttribute("Version", version));
                }

                metadata.Add(node);
            }
            else
            {
                var versionAttr = node.Attribute("Version");
                if (versionAttr == null)
                {
                    if (version != null)
                    {
                        versionAttr = new XAttribute("Version", version);
                        node.Add(versionAttr);
                    }
                }
                else if (overwriteExisting)
                {
                    versionAttr.Value = version;
                }
            }
        }

        private static string GetVersion(string sdkFile)
        {
            var path = SdkPathHelper.GetSdkPath(sdkFile);
            return File.Exists(path) ? FileVersionInfo.GetVersionInfo(path).ProductVersion : null;
        }
    }
}
