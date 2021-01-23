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

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Infrastructure.Branding
{
    public class MsixHeroBrandingInjector
    {
        public void Inject(XmlDocument modifiedDocument)
        {
            var doc = XDocument.Parse(modifiedDocument.OuterXml);
            this.Inject(doc);
            modifiedDocument.LoadXml(doc.ToString(SaveOptions.None));
        }

        public void Inject(XDocument modifiedDocument)
        {
            var win10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10");
            var namespace4 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
            var namespaceBuild = XNamespace.Get("http://schemas.microsoft.com/developer/appx/2015/build");

            Debug.Assert(modifiedDocument.Root != null, "modifiedDocument.Root != null");

            modifiedDocument.Root.SetAttributeValue(XNamespace.Xmlns + "build", namespaceBuild);
            modifiedDocument.Root.SetAttributeValue(XNamespace.Xmlns + "uap4", namespace4);

            var ignorable = modifiedDocument.Root.Attribute("IgnorableNamespaces");
            if (ignorable == null)
            {
                modifiedDocument.Root.Add(new XAttribute("IgnorableNamespaces", "build uap4"));
            }
            else
            {
                modifiedDocument.Root.SetAttributeValue("IgnorableNamespaces", string.Join(" ", ignorable.Value.Split(' ').Union(new [] { "build", "uap4" })));
            }

            var package = modifiedDocument.Element(win10 + "Package");
            if (package == null)
            {
                return;
            }

            var metaData = package.Element(namespaceBuild + "Metadata");
            if (metaData != null)
            {
                metaData.Remove();
            }

            metaData = new XElement(namespaceBuild + "Metadata");
            package.Add(metaData);
            var version = NdDll.RtlGetVersion();

            var operatingSystem = new XElement(namespaceBuild + "Item"); //(  "bu", "Item");
            metaData.Add(operatingSystem);
            operatingSystem.Add(new XAttribute("Name", "OperatingSystem"));
            operatingSystem.Add(new XAttribute("Version", $"{version.ToString(4)}"));

            var msixHero = new XElement(namespaceBuild + "Item");
            metaData.Add(msixHero);
            msixHero.Add(new XAttribute("Name", "MsixHero"));
            // ReSharper disable once PossibleNullReferenceException
            msixHero.Add(new XAttribute("Version", (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Version.ToString()));

            var signTool = new XElement(namespaceBuild + "Item");
            metaData.Add(signTool);
            signTool.Add(new XAttribute("Name", "SignTool.exe"));
            signTool.Add(new XAttribute("Version", GetVersion("SignTool.exe")));

            var makepri = new XElement(namespaceBuild + "Item");
            metaData.Add(makepri);
            makepri.Add(new XAttribute("Name", "MakePri.exe"));
            makepri.Add(new XAttribute("Version", GetVersion("MakePri.exe")));

            var makeappx = new XElement(namespaceBuild + "Item");
            metaData.Add(makeappx);
            makeappx.Add(new XAttribute("Name", "MakeAppx.exe"));
            makeappx.Add(new XAttribute("Version", GetVersion("MakeAppx.exe")));
        }

        private static string GetVersion(string sdkFile)
        {
            var path = MsixSdkWrapper.GetSdkPath(sdkFile);
            if (!File.Exists(path))
            {
                return null;
            }

            return FileVersionInfo.GetVersionInfo(path).ProductVersion;
        }
    }
}
