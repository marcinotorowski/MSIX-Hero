using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Parsers
{
    public class AppxExtensionsParser
    {
        private static readonly XNamespace NamespaceWindows10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10");
        private static readonly XNamespace NamespaceAppx = XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");
        private static readonly XNamespace NamespaceUap3 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/3");
        private static readonly XNamespace NamespaceUap5 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/5");
        private static readonly XNamespace NamespaceUap10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/10");
        private static readonly XNamespace NamespaceDesktop2 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/2");
        private static readonly XNamespace NamespaceDesktop6 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/6");

        public IEnumerable<AppxExtension> ParseManifest(XContainer extensionsNode)
        {
            if (extensionsNode == null)
            {
                yield break;
            }
            
            foreach (var extension in extensionsNode
                                          .Elements()
                                          .Where(e =>
                                              e.Name.LocalName == "Extension" &&
                                              (
                                                  e.Name.Namespace == NamespaceWindows10 ||
                                                  e.Name.Namespace == NamespaceAppx ||
                                                  e.Name.Namespace == NamespaceDesktop6 ||
                                                  e.Name.Namespace == NamespaceDesktop2 ||
                                                  e.Name.Namespace == NamespaceUap10 ||
                                                  e.Name.Namespace == NamespaceUap5 ||
                                                  e.Name.Namespace == NamespaceUap3)))
            {
                var category = extension.Attribute("Category")?.Value;

                switch (category)
                {
                    case "windows.hostRuntime":
                        var hostRuntimeNode = extension.Element(NamespaceUap10 + "HostRuntime");

                        var appxHost = new AppxHost
                        {
                            Category = category,
                            Id = hostRuntimeNode?.Attribute("Id")?.Value,
                            Executable = extension.Attribute("Executable")?.Value,
                            Behavior = Enum.TryParse<AppxHost.RunTimeBehavior>(extension.Attribute(NamespaceUap10 + "RuntimeBehavior")?.Value, true, out var parsedBehavior) ? parsedBehavior : 0,
                            TrustLevel = Enum.TryParse<AppxExtensionTrustLevel>(extension.Attribute(NamespaceUap10 + "TrustLevel")?.Value ?? "", true, out var parsed) ? parsed : 0
                        };

                        yield return appxHost;
                        break;

                    case "windows.service":
                        var serviceNode = extension.Element(NamespaceDesktop6 + "Service");
                        if (serviceNode == null)
                        {
                            continue;
                        }

                        var service = new AppxService
                        {
                            Category = "windows.service"
                        };

                        service.EntryPoint = extension.Attribute("EntryPoint")?.Value;
                        service.Executable = extension.Attribute("Executable")?.Value;

                        service.Name = extension.Attribute("Name")?.Value;
                        service.StartAccount = extension.Attribute("StartAccount")?.Value;
                        service.StartupType = extension.Attribute("StartupType")?.Value;

                        yield return service;
                        break;
                }
            }
        }
    }
}
