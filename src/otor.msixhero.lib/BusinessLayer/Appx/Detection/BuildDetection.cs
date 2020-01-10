using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (appxManifestPath == null)
            {
                throw new ArgumentNullException(nameof(appxManifestPath));
            }

            if (!File.Exists(appxManifestPath))
            {
                throw new FileNotFoundException("The manifest file does not exist.");
            }

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

            if (this.DetectRayPack(buildValues, manifestDirectory, out buildInfo))
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

        private bool DetectRayPack(Dictionary<string, string> buildValues, string manifestDirectory, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                // Detect RayPack by taking a look at the metadata of PsfLauncher.
                var fileLauncher = Path.Combine(manifestDirectory, "PsfLauncher.exe");
                if (File.Exists(fileLauncher))
                {
                    var fvi = FileVersionInfo.GetVersionInfo(fileLauncher);
                    if (fvi.ProductName != null &&  fvi.ProductName.StartsWith("Raynet", StringComparison.OrdinalIgnoreCase))
                    {
                        var pv = fvi.ProductVersion;
                        buildInfo = new BuildInfo
                        {
                            ProductName = "RayPack " + Version.Parse(pv).ToString(2),
                            ProductVersion = fvi.ProductVersion
                        };

                        return true;
                    }
                }
            }
            else
            {
                // Detect RayPack 6.2 which uses build meta data like this:
                // <build:Item Name="OperatingSystem" Version="6.2.9200.0" /><build:Item Name="Raynet.RaySuite.Common.Appx" Version="6.2.5306.1168" /></build:Metadata>
                var rayPack = GetValue(buildValues, "Raynet.RaySuite.Common.Appx");
                if (rayPack != null)
                {
                    buildInfo = new BuildInfo
                    {
                        ProductName = "RayPack " + Version.Parse(rayPack).ToString(2), 
                        ProductVersion = rayPack
                    };

                    var os = GetValue(buildValues, "OperatingSystem");
                    if (os != null)
                    {
                        var win10Version = Windows10Parser.GetOperatingSystemFromNameAndVersion(os);
                        buildInfo.OperatingSystem = win10Version.ToString();
                    }

                    return true;
                }
            }

            return false;
        }

        private static string GetValue(Dictionary<string, string> dict, string name)
        {
            return !dict.TryGetValue(name, out var value) ? null : value;
        }
    }
}
