using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Management.Deployment;
using Microsoft.Win32;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.Models.Logs;
using otor.msixhero.lib.BusinessLayer.Models.Manifest;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.Models.Users;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.PowerShellInterop;

namespace otor.msixhero.lib.Managers
{
    public class AppxPackageManager : IAppxPackageManager 
    {
        public Task<List<User>> GetUsersForPackage(Package package)
        {
            return this.GetUsersForPackage(package.Name);
        }

        public Task<List<User>> GetUsersForPackage(string packageName)
        {
            var pkgManager = new PackageManager();
            return Task.Run(() =>
            {
                var list = pkgManager.FindUsers(packageName).Select(u => new User(SidToAccountName(u.UserSecurityId))).ToList();
                return list;
            });
        }

        public async Task Remove(IEnumerable<Package> packages, bool forAllUsers = false, bool preserveAppData = false, IProgress<ProgressData> progress = null)
        {
            if (packages == null)
            {
                return;
            }
            
            var mmm = new PackageManager();
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var item in packages)
            {
                var task = AsyncOperationHelper.ConvertToTask(
                    mmm.RemovePackageAsync(item.ProductId,
                        forAllUsers ? RemovalOptions.RemoveForAllUsers : RemovalOptions.None),
                    "Removing " + item.DisplayName, CancellationToken.None, progress);
                await task.ConfigureAwait(false);
            }
        }

        public async Task Add(string filePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var reader = await AppxManifestSummaryBuilder.FromMsix(filePath).ConfigureAwait(false);
            var pkgManager = new PackageManager();
            await AsyncOperationHelper.ConvertToTask(
                pkgManager.AddPackageAsync(new Uri(filePath, UriKind.Absolute), Enumerable.Empty<Uri>(), DeploymentOptions.ForceApplicationShutdown), 
                "Installing " + reader.DisplayName + "...", 
                cancellationToken, 
                progress).ConfigureAwait(false);

        }

        public async Task RunToolInContext(Package package, string toolName)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (toolName == null)
            {
                throw new ArgumentNullException(nameof(toolName));
            }

            using var ps = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var cmd = ps.AddCommand("Invoke-CommandInDesktopPackage");
            using var param1 = cmd.AddParameter("Command", toolName);
            using var param2 = cmd.AddParameter("PackageFamilyName", package.PackageFamilyName);
            using var param3 = cmd.AddParameter("AppId", package.Name);
            using var param4 = cmd.AddParameter("PreventBreakaway");

            try
            {
                using var result = await ps.InvokeAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e.HResult == -2147024891 /* 0x80070005 E_ACCESSDENIED */)
                {
                    throw new DeveloperModeException("Developer mode must be enabled to use this feature.", e);
                }

                throw;
            }
        }

        public Task<RegistryMountState> GetRegistryMountState(Package package)
        {
            return this.GetRegistryMountState(package.InstallLocation, package.Name);
        }

        public async Task<IList<Log>> GetLogs(int maxCount)
        {
            using var ps = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var script = ps.AddScript("Get-AppxLog -All | Select -f " + maxCount);
            using var logs = await ps.InvokeAsync().ConfigureAwait(false);
            
            var factory = new LogFactory();
            return new List<Log>(logs.Select(log => factory.CreateFromPowerShellObject(log)));
        }

        public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName)
        {
            RegistryMountState hasRegistry;

            if (!File.Exists(Path.Combine(installLocation, "registry.dat")))
            {
                hasRegistry = RegistryMountState.NotApplicable;
            }
            else
            {
                using (var reg = Registry.LocalMachine.OpenSubKey("MSIX-Hero-" + packageName))
                {
                    if (reg != null)
                    {
                        hasRegistry = RegistryMountState.Mounted;
                    }
                    else
                    {
                        hasRegistry = RegistryMountState.NotMounted;
                    }
                }
            }

            return Task.FromResult(hasRegistry);
        }

        public Task UnmountRegistry(Package package)
        {
            return this.UnmountRegistry(package.Name);
        }

        public Task UnmountRegistry(string packageName)
        {
            return Task.Run(() =>
            {
                var proc = new ProcessStartInfo("cmd.exe", @"/c REG UNLOAD HKLM\MSIX-Hero-" + packageName);
                proc.UseShellExecute = true;
                proc.Verb = "runas";

                var p = Process.Start(proc);
                if (p == null)
                {
                    throw new InvalidOperationException("Could not start process for un-mounting");
                }

                p.WaitForExit();
            });
        }

        public Task MountRegistry(Package package, bool startRegedit = false)
        {
            return this.MountRegistry(package.Name, package.InstallLocation, startRegedit);
        }

        public Task MountRegistry(string packageName, string installLocation, bool startRegedit = false)
        {
            return Task.Run(() => {
                var proc = new ProcessStartInfo("cmd.exe", @"/c REG LOAD HKLM\MSIX-Hero-" + packageName + " \"" + Path.Combine(installLocation, "Registry.dat") + "\"")
                {
                    UseShellExecute = true, 
                    CreateNoWindow = true, 
                    Verb = "runas"
                };

                var p = Process.Start(proc);
                if (p == null)
                {
                    throw new InvalidOperationException("Could not start process for mounting");
                }

                p.WaitForExit();

                if (!startRegedit)
                {
                    return;
                }

                RegistryKey registry = null;
                try
                {
                    registry = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit");

                    if (registry == null)
                    {
                        registry = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit");
                    }

                    if (registry == null)
                    {
                        throw new InvalidOperationException();
                    }

                    var currentValue = registry.GetValue("LastKey") as string;

                    SetRegistryValue(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", "LastKey", @"Computer\HKEY_LOCAL_MACHINE\MSIX-Hero-" + packageName + @"\REGISTRY");

                    proc = new ProcessStartInfo("regedit.exe")
                    {
                        Verb = "runas", 
                        UseShellExecute = true
                    };

                    Process.Start(proc);
                    System.Threading.Thread.Sleep(200);

                    if (currentValue != null)
                    {
                        SetRegistryValue(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", "LastKey", currentValue);
                    }
                }
                finally
                {
                    if (registry != null)
                    {
                        registry.Dispose();
                    }
                }
            });
        }

        public Task Run(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (package.ManifestLocation == null || !File.Exists(package.ManifestLocation))
            {
                throw new FileNotFoundException();
            }

            var entryPoint = GetEntryPoints(package).FirstOrDefault();
            if (entryPoint == null)
            {
                throw new InvalidOperationException("This package has no entry points.");
            }

            var p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = entryPoint
            };

            p.StartInfo = startInfo;
            p.Start();
            return Task.FromResult(true);
        }

        public Task<IList<Package>> Get(PackageFindMode mode = PackageFindMode.Auto)
        {
            return this.Get(null, mode);
        }

        public async Task<Package> Get(string packageName, string publisher, PackageFindMode mode = PackageFindMode.Auto)
        {
            var pkgMan = new PackageManager();

            Windows.ApplicationModel.Package pkg;
            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                    pkg = await Task.Run(() => pkgMan.FindPackagesForUser(packageName, publisher).First()).ConfigureAwait(false);
                    break;
                case PackageFindMode.AllUsers:
                    pkg = await Task.Run(() => pkgMan.FindPackages(packageName, publisher).First()).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            if (pkg == null)
            {
                return null;
            }

            return await ConvertFrom(pkg).ConfigureAwait(false);
        }

        public async Task<IList<Package>> Get(string packageName, PackageFindMode mode = PackageFindMode.Auto)
        {
            var list = new List<Package>();

            if (mode == PackageFindMode.Auto)
            {
                mode = UserHelper.IsAdministrator() ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
            }

            var pkgMan = new PackageManager();
            IList<Windows.ApplicationModel.Package> allPackages;

            if (string.IsNullOrEmpty(packageName))
            {
                switch (mode)
                {
                    case PackageFindMode.CurrentUser:
                        allPackages = await Task.Run(() => pkgMan.FindPackagesForUser(string.Empty).ToList()).ConfigureAwait(false);
                        break;
                    case PackageFindMode.AllUsers:
                        allPackages = await Task.Run(() => pkgMan.FindPackages().ToList()).ConfigureAwait(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }
            else
            {
                switch (mode)
                {
                    case PackageFindMode.CurrentUser:
                        allPackages = new List<Windows.ApplicationModel.Package>
                        {
                            await Task.Run(() => pkgMan.FindPackageForUser(string.Empty, packageName)).ConfigureAwait(false)
                        };

                        break;
                    case PackageFindMode.AllUsers:
                        allPackages = new List<Windows.ApplicationModel.Package>
                        {
                            await Task.Run(() => pkgMan.FindPackage(packageName)).ConfigureAwait(false)
                        };

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }

            foreach (var item in allPackages)
            {
                var converted = await ConvertFrom(item).ConfigureAwait(false);
                if (converted != null)
                {
                    list.Add(converted);
                }
            }

            return list;
        }

        private async Task<Package> ConvertFrom(Windows.ApplicationModel.Package item)
        {
            string installLocation;
            DateTime installDate;
            try
            {
                installLocation = item.InstalledLocation?.Path;
            }
            catch (Exception)
            {
                return null;
            }

            try
            {
                installDate = item.InstalledDate.LocalDateTime;
            }
            catch (Exception)
            {
                installDate = DateTime.MinValue;
            }

            var details = await GetManifestDetails(installLocation).ConfigureAwait(false);
            var hasRegistry = await this.GetRegistryMountState(installLocation, item.Id.Name).ConfigureAwait(false);

            var pkg = new Package()
            {
                DisplayName = details.DisplayName,
                Name = item.Id.Name,
                Image = details.Logo,
                ProductId = item.Id.FullName,
                InstallLocation = installLocation,
                PackageFamilyName = item.Id.FamilyName,
                Description = details.Description,
                DisplayPublisherName = details.DisplayPublisherName,
                Publisher = item.Id.Publisher,
                TileColor = details.Color,
                Version = new Version(item.Id.Version.Major, item.Id.Version.Minor, item.Id.Version.Build, item.Id.Version.Revision),
                SignatureKind = Convert(item.SignatureKind),
                HasRegistry = hasRegistry,
                InstallDate = installDate
            };

            return pkg;
        }

        private static IEnumerable<string> GetEntryPoints(Package package)
        {
            if (!File.Exists(package.ManifestLocation))
            {
                yield break;
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(package.ManifestLocation);
            var nodes = xmlDocument.SelectNodes("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']");

            foreach (var node in nodes.OfType<XmlNode>())
            {
                var attrVal = node.Attributes["Id"]?.Value;
                if (string.IsNullOrEmpty(attrVal))
                {
                    attrVal = string.Empty;
                }
                else
                {
                    attrVal = "!" + attrVal;
                }

                yield return @"shell:appsFolder\" + package.PackageFamilyName + attrVal;
            }
        }

        private static string SidToAccountName(string sidString)
        {
            var sid = new SecurityIdentifier(sidString);
            try
            {
                var account = (NTAccount)sid.Translate(typeof(NTAccount));
                return account.ToString();
            }
            catch (IdentityNotMappedException)
            {
                return sidString;
            }
        }

        private static async Task<PkgDetails> GetManifestDetails(string installLocation)
        {
            try
            {
                var reader = await AppxManifestSummaryBuilder.FromInstallLocation(installLocation);
                var logo = Path.Combine(installLocation, reader.Logo);

                if (File.Exists(Path.Combine(installLocation, logo)))
                {
                    return new PkgDetails(reader.DisplayName, reader.DisplayPublisher, logo, reader.Description, reader.AccentColor);
                }

                var extension = Path.GetExtension(logo);
                var baseName = Path.GetFileNameWithoutExtension(logo);
                var baseFolder = Path.GetDirectoryName(logo);

                logo = null;

                var dirInfo = new DirectoryInfo(Path.Combine(installLocation, baseFolder));
                if (dirInfo.Exists)
                {
                    var found = dirInfo.EnumerateFiles(baseName + "*" + extension).FirstOrDefault();
                    if (found != null)
                    {
                        logo = found.FullName;
                    }
                }

                return new PkgDetails(reader.DisplayName, reader.DisplayPublisher, logo, reader.Description, reader.AccentColor);
            }
            catch (Exception)
            {
                return new PkgDetails();
            }
        }

        private static void SetRegistryValue(string key, string name, string value)
        {
            RegistryKey regKey = null;

            try
            {
                regKey = Registry.CurrentUser.OpenSubKey(key);
                if (regKey == null)
                {
                    regKey = Registry.CurrentUser.CreateSubKey(key);
                }

                if (regKey == null)
                {
                    throw new InvalidOperationException();
                }

                regKey.DeleteValue(name, false);
                if (!string.IsNullOrEmpty(value))
                {
                    regKey.SetValue(name, value, RegistryValueKind.String);
                }
            }
            finally
            {
                if (regKey != null)
                {
                    regKey.Dispose();
                }
            }
        }

        private static SignatureKind Convert(Windows.ApplicationModel.PackageSignatureKind signatureKind)
        {
            switch (signatureKind)
            {
                case Windows.ApplicationModel.PackageSignatureKind.None:
                    return SignatureKind.None;
                case Windows.ApplicationModel.PackageSignatureKind.Developer:
                    return SignatureKind.Developer;
                case Windows.ApplicationModel.PackageSignatureKind.Enterprise:
                    return SignatureKind.Enterprise;
                case Windows.ApplicationModel.PackageSignatureKind.Store:
                    return SignatureKind.Store;
                case Windows.ApplicationModel.PackageSignatureKind.System:
                    return SignatureKind.System;
                default:
                    throw new NotSupportedException();
            }
        }

        private struct PkgDetails
        {
            public PkgDetails(string displayName, string displayPublisherName, string logo, string description, string color)
            {
                this.DisplayName = displayName;
                this.DisplayPublisherName = displayPublisherName;
                this.Logo = logo;
                this.Description = description;
                this.Color = color;
            }

            public string Description;
            public string DisplayName;
            public string DisplayPublisherName;
            public string Logo;
            public string Color;
        }
    }
}