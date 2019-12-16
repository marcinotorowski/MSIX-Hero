using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Manifest.Build;
using otor.msixhero.lib.Domain.Appx.Manifest.Summary;

namespace otor.msixhero.lib.BusinessLayer.Appx.Detection
{
    public class BuildDetection
    {
        public BuildInfo Detect(AppxManifestSummary appxManifest)
        {
            if (this.DetectAdvancedInstaller(appxManifest, out var buildInfo))
            {
                return buildInfo;
            }

            if (this.DetectVisualStudio(appxManifest, out buildInfo))
            {
                return buildInfo;
            }

            if (this.DetectMsixHero(appxManifest, out buildInfo))
            {
                return buildInfo;
            }
            
            return null;
        }
        
        private bool DetectVisualStudio(AppxManifestSummary appxManifest, out BuildInfo buildInfo)
        {
            buildInfo = null;
            var visualStudio = GetValue(appxManifest.BuildMetaData, "VisualStudio");
            if (visualStudio == null)
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductName = "Microsoft Visual Studio",
                ProductVersion = visualStudio,
                
            };

            var win10 = GetValue(appxManifest.BuildMetaData, "OperatingSystem");
            if (win10 != null)
            {
                var firstUnit = win10.Split(' ')[0];
                buildInfo.OperatingSystem = Windows10Parser.GetOperatingSystemFromNameAndVersion(firstUnit).ToString();
            }

            buildInfo.Components = appxManifest.BuildMetaData;
            return true;
        }

        private bool DetectAdvancedInstaller(AppxManifestSummary manifest, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (manifest.BuildMetaData == null)
            {
                return false;
            }

            var advInst = GetValue(manifest.BuildMetaData, "AdvancedInstaller");
            if (advInst == null) 
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductLicense = GetValue(manifest.BuildMetaData, "ProjectLicenseType"),
                ProductName = "Advanced Installer",
                ProductVersion = advInst
            };

            var os = GetValue(manifest.BuildMetaData, "OperatingSystem");
            if (os != null)
            {
                var win10Version = Windows10Parser.GetOperatingSystemFromNameAndVersion(os);
                buildInfo.OperatingSystem = win10Version.ToString();
            }

            return true;
        }

        private bool DetectMsixHero(AppxManifestSummary manifest, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (manifest.BuildMetaData == null)
            {
                return false;
            }

            var advInst = GetValue(manifest.BuildMetaData, "MsixHero");
            if (advInst == null) 
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductName = "Advanced Installer",
                ProductVersion = advInst
            };

            var os = GetValue(manifest.BuildMetaData, "OperatingSystem");
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
