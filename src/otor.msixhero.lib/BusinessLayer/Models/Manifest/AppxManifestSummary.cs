using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using otor.msixhero.lib.BusinessLayer.Helpers;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest
{
    public class AppxManifestSummary
    {
        private const string AppxManifestName = "AppxManifest.xml";

        private AppxManifestSummary()
        {
        }
        
        public static async Task<AppxManifestSummary> FromMsix(string fullMsixFilePath)
        {
            if (!System.IO.File.Exists(fullMsixFilePath))
            {
                throw new FileNotFoundException("MSIX file does not exist.", fullMsixFilePath);
            }

            using var zipFile = ZipFile.OpenRead(fullMsixFilePath);
            var entry = zipFile.GetEntry(AppxManifestName);
            if (entry == null)
            {
                throw new FileNotFoundException("Manifest file not found.");
            }

            var result = new AppxManifestSummary();

            await using (var stream = entry.Open())
            {
                await result.SetPropertiesFromManifest(stream).ConfigureAwait(false);
            }

            return result;
        }

        public static Task<AppxManifestSummary> FromInstallLocation(string installLocation)
        {
            if (!System.IO.Directory.Exists(installLocation))
            {
                throw new DirectoryNotFoundException("Install location " + installLocation + " not found.");
            }

            return FromManifest(System.IO.Path.Join(installLocation, AppxManifestName));
        }

        public static async Task<AppxManifestSummary> FromManifest(string fullManifestPath)
        {
            if (!System.IO.File.Exists(fullManifestPath))
            {
                throw new FileNotFoundException("Manifest file does not exist.", fullManifestPath);
            }

            var result = new AppxManifestSummary();

            var allBytes = await File.ReadAllBytesAsync(fullManifestPath).ConfigureAwait(false);

            await using (var m = new MemoryStream(allBytes))
            {
                await result.SetPropertiesFromManifest(m).ConfigureAwait(false);
            }

            return result;
        }

        private async Task SetPropertiesFromManifest(Stream manifestStream)
        {
            var xmlDocument = new XmlDocument();
            var streamReader = new StreamReader(manifestStream);
            xmlDocument.LoadXml(await streamReader.ReadToEndAsync().ConfigureAwait(false));

            var identity = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']");

            this.Name = identity.Attributes["Name"]?.Value;
            this.Version = identity.Attributes["Version"]?.Value;
            this.Publisher = identity.Attributes["Publisher"]?.Value;
            this.ProcessorArchitecture = identity.Attributes["ProcessorArchitecture"]?.Value;
            this.SupportedOperatingSystems = new List<TargetOperatingSystem>();

            var node = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Properties']");

            foreach (XmlNode subNode in node.ChildNodes)
            {
                switch (subNode.LocalName)
                {
                    case "DisplayName":
                        this.DisplayName = subNode.InnerText;
                        break;
                    case "PublisherDisplayName":
                        this.DisplayPublisher = subNode.InnerText;
                        break;
                    case "Description":
                        this.Description = subNode.InnerText;
                        break;
                    case "Logo":
                        this.Logo = subNode.InnerText;
                        break;
                }
            }

            node = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']/*[local-name()='VisualElements']");
            this.AccentColor = node?.Attributes["BackgroundColor"]?.Value ?? "Transparent";

            var nodes = xmlDocument.SelectNodes("/*[local-name()='Package']/*[local-name()='Dependencies']/*[local-name()='TargetDeviceFamily']");
            foreach (var n in nodes.OfType<XmlNode>())
            {
                var minVersion = n.Attributes["MinVersion"]?.Value;
                var operatingSystem = n.Attributes["Name"]?.Value;

                var actual = Windows10Parser.GetOperatingSystemFromNameAndVersion(operatingSystem, minVersion);
                if (actual == null)
                {
                    continue;
                }

                this.SupportedOperatingSystems.Add(actual);
            }
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public string ProcessorArchitecture { get; set; }

        public string Publisher { get; set; }

        public string Logo { get; private set; }

        public string DisplayName { get; private set; }

        public string DisplayPublisher { get; private set; }

        public string Description { get; private set; }

        public string AccentColor { get; private set; }

        public List<TargetOperatingSystem> SupportedOperatingSystems { get; private set; }
    }
}
