using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Infrastructure.Logging;

namespace otor.msixhero.lib.Domain.Appx.Manifest.Summary
{
    public class AppxManifestSummaryBuilder
    {
        private const string AppxManifestName = "AppxManifest.xml";

        private static readonly ILog Logger = LogManager.GetLogger();

        public static async Task<AppxManifestSummary> FromMsix(string fullMsixFilePath, bool basicInformation = true)
        {
            Logger.Info("Reading application manifest {0}...", fullMsixFilePath);

            if (!System.IO.File.Exists(fullMsixFilePath))
            {
                throw new FileNotFoundException("MSIX file does not exist.", fullMsixFilePath);
            }

            try
            {
                Logger.Debug("Opening file as ZIP...");
                using var zipFile = ZipFile.OpenRead(fullMsixFilePath);

                Logger.Debug("Getting entry {0}...", AppxManifestName);
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
            Logger.Debug("Reading application manifest from install location {0}...", installLocation);
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

            Logger.Debug("Loading XML file...");
            xmlDocument.LoadXml(await streamReader.ReadToEndAsync().ConfigureAwait(false));

            Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Identity'] for a single node...");
            var identity = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']");
            
            result.Name = identity.Attributes["Name"]?.Value;
            result.Version = identity.Attributes["Version"]?.Value;
            result.Publisher = identity.Attributes["Publisher"]?.Value;
            result.ProcessorArchitecture = identity.Attributes["ProcessorArchitecture"]?.Value;
            // result.OperatingSystemDependencies = new List<OperatingSystemDependency>();
            // result.PackageDependencies = new List<PackageDependency>();

            result.PackageType = 0;

            Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']...");
            var applications = xmlDocument.SelectNodes("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']");
            foreach (var subNode in applications.OfType<XmlNode>())
            {
                var entryPoint = subNode.Attributes["EntryPoint"]?.Value;
                var executable = subNode.Attributes["Executable"]?.Value;
                var startPage = subNode.Attributes["StartPage"]?.Value;
                result.PackageType |= PackageTypeConverter.GetPackageTypeFrom(entryPoint, executable, startPage);
            }

            Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Properties'] for a single node...");
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

            Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']/*[local-name()='VisualElements'] for a single node...");
            node = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']/*[local-name()='VisualElements']");
            result.AccentColor = node?.Attributes["BackgroundColor"]?.Value ?? "Transparent";

            //SetDependencies(xmlDocument, result);
            //SetPsf(xmlDocument, result);

            Logger.Debug("Manifest information parsed.");
            return result;
        }
    }
}