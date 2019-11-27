using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.BusinessLayer.Models.Packages;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest.Summary
{
    public class AppxManifestSummaryBuilder
    {
        private const string AppxManifestName = "AppxManifest.xml";

        public static async Task<AppxManifestSummary> FromMsix(string fullMsixFilePath, bool basicInformation = true)
        {
            if (!System.IO.File.Exists(fullMsixFilePath))
            {
                throw new FileNotFoundException("MSIX file does not exist.", fullMsixFilePath);
            }

            try
            {
                using var zipFile = ZipFile.OpenRead(fullMsixFilePath);
                var entry = zipFile.GetEntry(AppxManifestName);
                if (entry == null)
                {
                    throw new FileNotFoundException("Manifest file not found.");
                }

                await using (var stream = entry.Open())
                {
                    return await FromManifest(stream, null).ConfigureAwait(false);
                }
            }
            catch (InvalidDataException e)
            {
                throw new InvalidOperationException("File " + fullMsixFilePath + " does not seem to be a valid MSIX package.", e);
            }
        }

        public static Task<AppxManifestSummary> FromInstallLocation(string installLocation)
        {
            if (!System.IO.Directory.Exists(installLocation))
            {
                throw new DirectoryNotFoundException("Install location " + installLocation + " not found.");
            }

            return FromManifest(System.IO.Path.Join(installLocation, AppxManifestName));
        }

        public static async Task<AppxManifestSummary> FromManifest(Stream manifestStream)
        {
            return await FromManifest(manifestStream, null).ConfigureAwait(false);
        }

        public static async Task<AppxManifestSummary> FromManifest(string fullManifestPath)
        {
            if (!System.IO.File.Exists(fullManifestPath))
            {
                throw new FileNotFoundException("Manifest file does not exist.", fullManifestPath);
            }

            using (var fs = File.OpenRead(fullManifestPath))
            {
                return await FromManifest(fs, null).ConfigureAwait(false);
            }
        }

        private static async Task<AppxManifestSummary> FromManifest(Stream manifestStream, Stream configJsonStream)
        {
            var result = new AppxManifestSummary();
            var xmlDocument = new XmlDocument();
            var streamReader = new StreamReader(manifestStream);
            xmlDocument.LoadXml(await streamReader.ReadToEndAsync().ConfigureAwait(false));

            var identity = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']");

            result.Name = identity.Attributes["Name"]?.Value;
            result.Version = identity.Attributes["Version"]?.Value;
            result.Publisher = identity.Attributes["Publisher"]?.Value;
            result.ProcessorArchitecture = identity.Attributes["ProcessorArchitecture"]?.Value;
            // result.OperatingSystemDependencies = new List<OperatingSystemDependency>();
            // result.PackageDependencies = new List<PackageDependency>();

            result.PackageType = 0;

            var applications = xmlDocument.SelectNodes("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']");
            foreach (var subNode in applications.OfType<XmlNode>())
            {
                var entryPoint = subNode.Attributes["EntryPoint"]?.Value;
                var executable = subNode.Attributes["Executable"]?.Value;
                var startPage = subNode.Attributes["StartPage"]?.Value;
                result.PackageType |= PackageTypeConverter.GetPackageTypeFrom(entryPoint, executable, startPage);
            }
            
            var node = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Properties']");

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
                }
            }

            node = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']/*[local-name()='VisualElements']");
            result.AccentColor = node?.Attributes["BackgroundColor"]?.Value ?? "Transparent";

            //SetDependencies(xmlDocument, result);
            //SetPsf(xmlDocument, result);
            return result;
        }

        // private static void SetPsf(XmlDocument xmlDocument, AppxManifestSummary result)
        // {
        //    var nodes = xmlDocument.SelectNodes("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']");
        //    foreach (var xmlNode in nodes.OfType<XmlNode>())
        //    {
        //        var exe = xmlNode.Attributes["Executable"]?.Value;
        //        if (string.Equals(exe, "PsfLauncher32.exe", StringComparison.OrdinalIgnoreCase) || string.Equals(exe, "PsfLauncher64.exe", StringComparison.OrdinalIgnoreCase) || string.Equals(exe, "PsfLauncher.exe", StringComparison.OrdinalIgnoreCase))
        //        {
        //            result.IsPsfLauncher = true;
        //        }
        //    }
        //}

        //private static void SetPackageDependency(XmlNode node, AppxManifestSummary result)
        //{
        //    var minVersion = node.Attributes["MinVersion"]?.Value;
        //    var name = node.Attributes["Name"]?.Value;
        //    var publisher = node.Attributes["Publisher"]?.Value;

        //    var packageDependency = new PackageDependency
        //    {
        //        Name = name,
        //        Version = minVersion,
        //        Publisher = publisher
        //    };

        //    result.PackageDependencies.Add(packageDependency);
        //}

        //private static void SetDeviceFamilyDependency(XmlNode node, AppxManifestSummary result)
        //{
        //    var minVersion = node.Attributes["MinVersion"]?.Value;
        //    var testedVersion = node.Attributes["MaxVersionTested"]?.Value;
        //    var operatingSystem = node.Attributes["Name"]?.Value;

        //    var dep = new OperatingSystemDependency();
        //    var actual = Windows10Parser.GetOperatingSystemFromNameAndVersion(operatingSystem, minVersion);
        //    if (actual != null)
        //    {
        //        dep.Minimum = actual;
        //    }

        //    actual = Windows10Parser.GetOperatingSystemFromNameAndVersion(operatingSystem, testedVersion);
        //    if (actual != null)
        //    {
        //        dep.Tested = actual;
        //    }

        //    if (dep.Tested == null && dep.Minimum == null)
        //    {
        //        return;
        //    }

        //    result.OperatingSystemDependencies.Add(dep);
        //}

        //private static void SetDependencies(XmlDocument manifest, AppxManifestSummary result)
        //{
        //    var nodes = manifest.SelectNodes("/*[local-name()='Package']/*[local-name()='Dependencies']/*");

        //    foreach (var n in nodes.OfType<XmlNode>())
        //    {
        //        switch (n.LocalName)
        //        {
        //            case "TargetDeviceFamily":
        //                SetDeviceFamilyDependency(n, result);
        //                break;
        //            case "PackageDependency":
        //                SetPackageDependency(n, result);
        //                break;
        //        }
        //    }
        //}
    }
}