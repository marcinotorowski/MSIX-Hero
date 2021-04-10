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
        public void Inject(XDocument modifiedDocument)
        {
            XNamespace windows10Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace appxNamespace = "http://schemas.microsoft.com/appx/2010/manifest";
            XNamespace uap4Namespace = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
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
            
            var originalUap4Namespace = modifiedDocument.Root.Attribute(XNamespace.Xmlns + "uap4");
            if (originalUap4Namespace != null)
            {
                uap4Namespace = originalUap4Namespace.Value;
            }
            else
            {
                modifiedDocument.Root.SetAttributeValue(XNamespace.Xmlns + "uap4", uap4Namespace);
                namespaceAdded = true;
            }

            if (namespaceAdded)
            {
                var ignorable = modifiedDocument.Root.Attribute("IgnorableNamespaces");
                if (ignorable == null)
                {
                    modifiedDocument.Root.Add(new XAttribute("IgnorableNamespaces", "build uap4"));
                }
                else
                {
                    modifiedDocument.Root.SetAttributeValue("IgnorableNamespaces", string.Join(" ", ignorable.Value.Split(' ').Union(new[] { "build", "uap4" })));
                }
            }

            var metaData = package.Element(buildNamespace + "Metadata");
            metaData?.Remove();

            metaData = new XElement(buildNamespace + "Metadata");
            package.Add(metaData);
            var version = NdDll.RtlGetVersion();

            var operatingSystem = new XElement(buildNamespace + "Item"); //(  "bu", "Item");
            metaData.Add(operatingSystem);
            operatingSystem.Add(new XAttribute("Name", "OperatingSystem"));
            operatingSystem.Add(new XAttribute("Version", version.ToString(4)));

            var msixHero = new XElement(buildNamespace + "Item");
            metaData.Add(msixHero);
            msixHero.Add(new XAttribute("Name", "MsixHero"));
            // ReSharper disable once PossibleNullReferenceException
            msixHero.Add(new XAttribute("Version", (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Version.ToString()));
            
            var signTool = new XElement(buildNamespace + "Item");
            metaData.Add(signTool);
            signTool.Add(new XAttribute("Name", "SignTool.exe"));
            signTool.Add(new XAttribute("Version", GetVersion("SignTool.exe")));

            // ReSharper disable once IdentifierTypo
            var makepri = new XElement(buildNamespace + "Item");
            metaData.Add(makepri);
            makepri.Add(new XAttribute("Name", "MakePri.exe"));
            makepri.Add(new XAttribute("Version", GetVersion("MakePri.exe")));

            // ReSharper disable once IdentifierTypo
            var makeappx = new XElement(buildNamespace + "Item");
            metaData.Add(makeappx);
            makeappx.Add(new XAttribute("Name", "MakeAppx.exe"));
            makeappx.Add(new XAttribute("Version", GetVersion("MakeAppx.exe")));
        }

        private static string GetVersion(string sdkFile)
        {
            var path = MsixSdkWrapper.GetSdkPath(sdkFile);
            return File.Exists(path) ? FileVersionInfo.GetVersionInfo(path).ProductVersion : null;
        }
    }
}
