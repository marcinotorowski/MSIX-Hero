using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using ControlzEx.Standard;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Wrappers;

namespace otor.msixhero.lib.BusinessLayer.Appx.Detection
{
    public class MsixHeroBrandingInjector
    {
        private const string DefaultNamespacePrefix = "msixHero";

        public void Inject(XmlDocument modifiedDocument)
        {
            var namespaceManager = new XmlNamespaceManager(modifiedDocument.NameTable);
            if (!namespaceManager.HasNamespace("uap4"))
            {
                namespaceManager.AddNamespace("uap4", "http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
            }

            if (!namespaceManager.HasNamespace("build"))
            {
                namespaceManager.AddNamespace("build", "http://schemas.microsoft.com/developer/appx/2015/build");
            }

            if (!namespaceManager.HasNamespace(DefaultNamespacePrefix))
            {
                namespaceManager.AddNamespace(DefaultNamespacePrefix, modifiedDocument.DocumentElement.NamespaceURI);
            }

            var package = GetOrCreateNode(modifiedDocument, modifiedDocument, "Package", DefaultNamespacePrefix, namespaceManager);

            var metaData = GetNode(package, "build:Metadata", namespaceManager);
            if (metaData != null)
            {
                package.RemoveChild(metaData);
            }

            metaData = CreateNode(modifiedDocument, package, "build:Metadata", namespaceManager);

            var version = NtDll.RtlGetVersion();

            var operatingSystem = CreateNode(modifiedDocument, metaData, "build:Item", namespaceManager);
            operatingSystem.SetAttribute("Name", "OperatingSystem");
            operatingSystem.SetAttribute("Version", $"{version.ToString(4)}");

            var msixHero = CreateNode(modifiedDocument, metaData, "build:Item", namespaceManager);
            msixHero.SetAttribute("Name", "MsixHero");
            msixHero.SetAttribute("Version", (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Version.ToString());

            var signTool = CreateNode(modifiedDocument, metaData, "build:Item", namespaceManager);
            signTool.SetAttribute("Name", "SignTool.exe");
            signTool.SetAttribute("Version", GetVersion("SignTool.exe"));

            var makepri = CreateNode(modifiedDocument, metaData, "build:Item", namespaceManager);
            makepri.SetAttribute("Name", "MakePri.exe");
            makepri.SetAttribute("Version", GetVersion("MakePri.exe"));

            var makeappx = CreateNode(modifiedDocument, metaData, "build:Item", namespaceManager);
            makeappx.SetAttribute("Name", "MakeAppx.exe");
            makeappx.SetAttribute("Version", GetVersion("MakeAppx.exe"));
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

        private static XmlElement GetNode(XmlNode xmlNode, string qualifiedName, XmlNamespaceManager namespaceManager)
        {
            if (string.IsNullOrEmpty(qualifiedName))
            {
                throw new ArgumentNullException(nameof(qualifiedName));
            }

            var indexOf = qualifiedName.IndexOf(':');
            if (indexOf == -1)
            {
                return GetNode(xmlNode, qualifiedName, DefaultNamespacePrefix, namespaceManager);
            }

            return GetNode(xmlNode, qualifiedName.Substring(indexOf + 1), qualifiedName.Substring(0, indexOf), namespaceManager);
        }

        private static XmlElement CreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string qualifiedName, XmlNamespaceManager namespaceManager)
        {
            if (string.IsNullOrEmpty(qualifiedName))
            {
                throw new ArgumentNullException(nameof(qualifiedName));
            }

            var indexOf = qualifiedName.IndexOf(':');
            if (indexOf == -1)
            {
                return CreateNode(xmlDocument, xmlNode, qualifiedName, DefaultNamespacePrefix, namespaceManager);
            }

            return CreateNode(xmlDocument, xmlNode, qualifiedName.Substring(indexOf + 1), qualifiedName.Substring(0, indexOf), namespaceManager);
        }

        private static XmlElement GetNode(XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var node = xmlNode.SelectSingleNode($"{prefix}:{name}", namespaceManager);
            return (XmlElement)node;
        }

        private static XmlElement GetOrCreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var node = xmlNode.SelectSingleNode($"{prefix}:{name}", namespaceManager);
            // ReSharper disable once InvertIf
            if (node == null)
            {
                return CreateNode(xmlDocument, xmlNode, name, prefix, namespaceManager);
            }

            return (XmlElement)node;
        }

        private static XmlElement CreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var ns = namespaceManager.LookupNamespace(prefix);
            if (string.IsNullOrEmpty(ns))
            {
                ns = xmlDocument.DocumentElement.NamespaceURI;
            }

            if (ns != xmlDocument.DocumentElement.NamespaceURI && !string.IsNullOrEmpty(prefix))
            {
                name = $"{prefix}:{name}";
            }

            var node = xmlDocument.CreateElement(name, ns);
            xmlNode.AppendChild(node);
            return node;
        }
    }
}
