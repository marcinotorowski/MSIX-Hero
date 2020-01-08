using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Manifest.Build;
using otor.msixhero.lib.Infrastructure.Logging;

namespace otor.msixhero.lib.BusinessLayer.Appx.Detection
{
    public class BuildDetection
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        public BuildInfo Detect(string appxManifestPath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(appxManifestPath);

            Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Metadata']/*[local-name()='Item'] for a single node...");
            var buildNotes = xmlDocument.SelectNodes("/*[local-name()='Package']/*[local-name()='Metadata']/*[local-name()='Item']");
            var buildKeyValues = new Dictionary<string, string>();

            foreach (var buildNode in buildNotes.OfType<XmlNode>())
            {
                var attrName = buildNode.Attributes["Name"]?.Value;
                if (attrName == null)
                {
                    continue;
                }

                var attrVersion = buildNode.Attributes["Version"]?.Value;
                if (attrVersion == null)
                {
                    attrVersion = buildNode.Attributes["Value"]?.Value;
                    if (attrVersion == null)
                    {
                        continue;
                    }
                }

                buildKeyValues[attrName] = attrVersion;
            }

            return this.Detect(buildKeyValues, Path.GetDirectoryName(appxManifestPath));
        }

        public BuildInfo Detect(Dictionary<string, string> buildValues, string manifestDirectory = null)
        {
            if (this.DetectAdvancedInstaller(buildValues, out var buildInfo))
            {
                return buildInfo;
            }

            if (this.DetectVisualStudio(buildValues, out buildInfo))
            {
                return buildInfo;
            }

            if (this.DetectMsixHero(buildValues, out buildInfo))
            {
                return buildInfo;
            }
            
            return null;
        }
        
        private bool DetectVisualStudio(Dictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            var visualStudio = GetValue(buildValues, "VisualStudio");
            if (visualStudio == null)
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductName = "Microsoft Visual Studio",
                ProductVersion = visualStudio,
                
            };

            var win10 = GetValue(buildValues, "OperatingSystem");
            if (win10 != null)
            {
                var firstUnit = win10.Split(' ')[0];
                buildInfo.OperatingSystem = Windows10Parser.GetOperatingSystemFromNameAndVersion(firstUnit).ToString();
            }

            buildInfo.Components = buildValues;
            return true;
        }

        private bool DetectAdvancedInstaller(Dictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            var advInst = GetValue(buildValues, "AdvancedInstaller");
            if (advInst == null) 
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductLicense = GetValue(buildValues, "ProjectLicenseType"),
                ProductName = "Advanced Installer",
                ProductVersion = advInst
            };

            var os = GetValue(buildValues, "OperatingSystem");
            if (os != null)
            {
                var win10Version = Windows10Parser.GetOperatingSystemFromNameAndVersion(os);
                buildInfo.OperatingSystem = win10Version.ToString();
            }

            return true;
        }

        private bool DetectMsixHero(Dictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            var msixHero = GetValue(buildValues, "MsixHero");
            if (msixHero == null) 
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductName = "MSIX Hero",
                ProductVersion = msixHero
            };

            var os = GetValue(buildValues, "OperatingSystem");
            if (os != null)
            {
                var win10Version = Windows10Parser.GetOperatingSystemFromNameAndVersion(os);
                buildInfo.OperatingSystem = win10Version.ToString();
            }

            return true;
        }

        private static string GetValue(Dictionary<string, string> dict, string name)
        {
            return !dict.TryGetValue(name, out var value) ? null : value;
        }
    }
}
