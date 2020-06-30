using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using ControlzEx.Standard;
using otor.msixhero.lib.Infrastructure.Wrappers;

namespace otor.msixhero.lib.BusinessLayer.Appx.Detection
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
            var version = NtDll.RtlGetVersion();

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
