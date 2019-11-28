﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Management.Deployment;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.BusinessLayer.Models.Logs;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Full;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Summary;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.Models.Users;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.PowerShellInterop;
using LogFactory = otor.msixhero.lib.BusinessLayer.Helpers.LogFactory;

namespace otor.msixhero.lib.Managers
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class CurrentUserAppxPackageManager : IAppxPackageManager
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        private readonly IAppxSigningManager signingManager;

        public CurrentUserAppxPackageManager(IAppxSigningManager signingManager)
        {
            this.signingManager = signingManager;
        }

        public Task<List<User>> GetUsersForPackage(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.GetUsersForPackage(package.Name, cancellationToken, progress);
        }

        public async Task<List<User>> GetUsersForPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Getting users who installed package {0}...", packageName);
            if (!UserHelper.IsAdministrator())
            {
                Logger.Info("The user is not administrator. Returning an empty list.");
                return new List<User>();
            }

            var pkgManager = new PackageManager();
            var result = await Task.Run(
                () =>
                {
                    var list = pkgManager.FindUsers(packageName).Select(u => new User(SidToAccountName(u.UserSecurityId))).ToList();
                    return list;
                },
                cancellationToken).ConfigureAwait(false);

            Logger.Info("Returning {0} users...", result.Count);
            return result;
        }

        public Task InstallCertificate(string certificateFilePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.signingManager.InstallCertificate(certificateFilePath, cancellationToken, progress);
        }

        public async Task Remove(IReadOnlyCollection<Package> packages, bool forAllUsers = false,
            bool preserveAppData = false, CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            if (!packages.Any())
            {
                Logger.Warn("Removing 0 packages, the list from the user is empty.");
                return;
            }

            Logger.Info("Removing {0} packages...", packages.Count);

            var mmm = new PackageManager();
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var item in packages)
            {
                Logger.Info("Removing {0}", item.ProductId);

                var task = AsyncOperationHelper.ConvertToTask(
                    mmm.RemovePackageAsync(item.ProductId,
                        forAllUsers ? RemovalOptions.RemoveForAllUsers : RemovalOptions.None),
                    "Removing " + item.DisplayName, CancellationToken.None, progress);
                await task.ConfigureAwait(false);
            }
        }

        public async Task Add(string filePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Installing package {0}", filePath);
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            try
            {
                var reader = await AppxManifestSummaryBuilder.FromMsix(filePath).ConfigureAwait(false);
                var pkgManager = new PackageManager();
                await AsyncOperationHelper.ConvertToTask(
                    pkgManager.AddPackageAsync(new Uri(filePath, UriKind.Absolute), Enumerable.Empty<Uri>(), DeploymentOptions.ForceApplicationShutdown),
                    "Installing " + reader.DisplayName + "...",
                    cancellationToken,
                    progress).ConfigureAwait(false);
            }
            catch (InvalidDataException e)
            {
                throw new InvalidOperationException("File " + filePath + " does not seem to be a valid MSIX package." + e, e);
            }
        }

        public Task RunToolInContext(Package package, string toolPath, string arguments, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (toolPath == null)
            {
                throw new ArgumentNullException(nameof(toolPath));
            }

            return this.RunToolInContext(package.PackageFamilyName, package.Name, toolPath, arguments, cancellationToken, progress);
        }

        public async Task RunToolInContext(string packageFamilyName, string appId, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Running tool '{0}' with arguments '{1}' in package '{2}' (AppId = '{3}')...", toolPath, arguments, packageFamilyName, appId);
            if (packageFamilyName == null)
            {
                throw new ArgumentNullException(nameof(packageFamilyName));
            }

            if (toolPath == null)
            {
                throw new ArgumentNullException(nameof(toolPath));
            }

            using var ps = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var cmd = ps.AddCommand("Invoke-CommandInDesktopPackage");
            cmd.AddParameter("Command", toolPath);
            cmd.AddParameter("PackageFamilyName", packageFamilyName);
            cmd.AddParameter("AppId", appId);
            cmd.AddParameter("PreventBreakaway");

            if (!string.IsNullOrEmpty(arguments))
            {
                cmd.AddParameter("Args", arguments);
            }

            Logger.Debug("Executing Invoke-CommandInDesktopPackage");
            try
            {
                // ReSharper disable once UnusedVariable
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

        public Task<RegistryMountState> GetRegistryMountState(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.GetRegistryMountState(package.InstallLocation, package.Name);
        }

        public async Task<IList<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Getting last {0} log files...", maxCount);

            using var ps = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var script = ps.AddScript("Get-AppxLog -All | Select -f " + maxCount);
            using var logs = await ps.InvokeAsync().ConfigureAwait(false);
            
            var factory = new LogFactory();
            return new List<Log>(logs.Select(log => factory.CreateFromPowerShellObject(log)));
        }

        public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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

        public Task UnmountRegistry(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.UnmountRegistry(package.Name, cancellationToken, progress);
        }

        public Task UnmountRegistry(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Task.Run(() =>
            {
                var proc = new ProcessStartInfo("cmd.exe", @"/c REG UNLOAD HKLM\MSIX-Hero-" + packageName);
                Console.WriteLine(@"/c REG UNLOAD HKLM\MSIX-Hero-" + packageName);
                proc.UseShellExecute = true;
                proc.Verb = "runas";

                var p = Process.Start(proc);
                if (p == null)
                {
                    throw new InvalidOperationException("Could not start process for un-mounting");
                }

                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    throw new InvalidOperationException($"cmd.exe returned {p.ExitCode} exit code.");
                }
            },
            cancellationToken);
        }

        public Task MountRegistry(Package package, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.MountRegistry(package.Name, package.InstallLocation, startRegedit);
        }

        public Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Task.Run(() => {
                if (startRegedit)
                {
                    try
                    {
                        SetRegistryValue(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", "LastKey", @"Computer\HKEY_LOCAL_MACHINE\MSIX-Hero-" + packageName + @"\REGISTRY");
                    }
                    catch (Exception)
                    {
                        // whatever...
                    }
                }

                Console.WriteLine(@"/c REG LOAD HKLM\MSIX-Hero-" + packageName);
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
                if (p.ExitCode != 0)
                {
                    throw new InvalidOperationException($"cmd.exe returned {p.ExitCode} exit code.");
                }

                if (!startRegedit)
                {
                    return;
                }

                proc = new ProcessStartInfo("regedit.exe")
                {
                    Verb = "runas", 
                    UseShellExecute = true
                };

                Process.Start(proc);
            },
            cancellationToken);
        }

        public Task Run(Package package, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.Run(package.ManifestLocation, package.PackageFamilyName, appId, cancellationToken, progress);
        }

        public Task Run(string packageManifestLocation, string packageFamilyName, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (packageManifestLocation == null || !File.Exists(packageManifestLocation))
            {
                throw new FileNotFoundException();
            }

            if (packageFamilyName == null)
            {
                throw new ArgumentNullException(nameof(packageFamilyName));
            }

            var entryPoint = GetEntryPoints(packageManifestLocation, packageFamilyName).FirstOrDefault(e => appId == null || e == appId);
            if (entryPoint == null)
            {
                if (appId == null)
                {
                    throw new InvalidOperationException("This package has no entry points.");
                }

                throw new InvalidOperationException($"Entry point '{appId}' was not found.");
            }

            var p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                Verb = "runas",
                FileName = entryPoint
            };

            p.StartInfo = startInfo;
            p.Start();
            return Task.FromResult(true);
        }

        public Task<IList<Package>> Get(PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.Get(null, mode, cancellationToken, progress);
        }

        public async Task<Package> Get(string packageName, string publisher, PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var pkgMan = new PackageManager();

            Windows.ApplicationModel.Package pkg;
            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                    pkg = await Task.Run(() => pkgMan.FindPackagesForUser(packageName, publisher).First(), cancellationToken).ConfigureAwait(false);
                    break;
                case PackageFindMode.AllUsers:
                    pkg = await Task.Run(() => pkgMan.FindPackages(packageName, publisher).First(), cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            if (pkg == null)
            {
                return null;
            }

            return await ConvertFrom(pkg, cancellationToken, progress).ConfigureAwait(false);
        }

        public Task<AppxPackage> Get(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Task.Run(() =>
            {
                var nativePkg = Native.QueryPackageInfo(packageName, Native.PackageConstants.PACKAGE_INFORMATION_FULL);

                AppxPackage mainApp = null;
                IList<AppxPackage> dependencies = new List<AppxPackage>();

                foreach (var item in nativePkg)
                {
                    if (mainApp == null)
                    {
                        mainApp = item;
                    }
                    else
                    {
                        dependencies.Add(item);
                    }
                }

                if (mainApp == null)
                {
                    return null;
                }

                foreach (var dependency in mainApp.PackageDependencies)
                {
                    dependency.Dependency = dependencies.FirstOrDefault(d =>
                        d.Publisher == dependency.Publisher &&
                        d.Name == dependency.Name &&
                        Version.Parse(d.Version) >= Version.Parse(dependency.Version));
                }

                return mainApp;
            }, cancellationToken);
        }

        private async Task<IList<Package>> Get(string packageName, PackageFindMode mode, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
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
                Logger.Info("Getting all packages by find mode = '{0}'", mode);
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
                Logger.Info("Getting package name '{0}' by find mode = '{1}'", packageName, mode);
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
                var converted = await ConvertFrom(item, cancellationToken, progress).ConfigureAwait(false);
                if (converted != null)
                {
                    list.Add(converted);
                }
            }

            Logger.Info("Returning {0} packages...", list.Count);
            return list;
        }

        private async Task<Package> ConvertFrom(Windows.ApplicationModel.Package item, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
        {
            Logger.Debug("Getting details about package {0}...", item.Id.Name);
            string installLocation;
            DateTime installDate;
            try
            {
                installLocation = item.InstalledLocation?.Path;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Installed location for package {0} is invalid. This may be expected for some installed packages.", item.Id.Name);
                return null;
            }

            try
            {
                installDate = item.InstalledDate.LocalDateTime;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Installed date for package {0} is invalid. This may be expected for some installed packages.", item.Id.Name);
                installDate = DateTime.MinValue;
            }

            var details = await GetManifestDetails(installLocation, cancellationToken, progress).ConfigureAwait(false);
            var hasRegistry = await this.GetRegistryMountState(installLocation, item.Id.Name, cancellationToken, progress).ConfigureAwait(false);

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
                PackageType = details.PackageType,
                Version = new Version(item.Id.Version.Major, item.Id.Version.Minor, item.Id.Version.Build, item.Id.Version.Revision),
                SignatureKind = Convert(item.SignatureKind),
                HasRegistry = hasRegistry,
                InstallDate = installDate
            };

            return pkg;
        }

        private static IEnumerable<string> GetEntryPoints(string manifestLocation, string familyName)
        {
            if (!File.Exists(manifestLocation))
            {
                yield break;
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(manifestLocation);
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

                yield return @"shell:appsFolder\" + familyName + attrVal;
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

        private static async Task<PkgDetails> GetManifestDetails(string installLocation, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var reader = await AppxManifestSummaryBuilder.FromInstallLocation(installLocation);
                var logo = Path.Combine(installLocation, reader.Logo);
                
                if (File.Exists(Path.Combine(installLocation, logo)))
                {
                    return new PkgDetails(
                        reader.DisplayName, 
                        reader.DisplayPublisher, 
                        logo, 
                        reader.Description, 
                        reader.AccentColor,
                        reader.PackageType);
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

                cancellationToken.ThrowIfCancellationRequested();
                return new PkgDetails(reader.DisplayName, reader.DisplayPublisher, logo, reader.Description, reader.AccentColor, reader.PackageType);
            }
            catch (OperationCanceledException)
            {
                throw;
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

                if (!string.IsNullOrEmpty(value))
                {
                    regKey.SetValue(name, value, RegistryValueKind.String);
                }
                else
                {
                    regKey.DeleteValue(name, false);
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
            public PkgDetails(string displayName, string displayPublisherName, string logo, string description, string color, PackageType packageType)
            {
                this.DisplayName = displayName;
                this.DisplayPublisherName = displayPublisherName;
                this.Logo = logo;
                this.Description = description;
                this.Color = color;
                this.PackageType = packageType;
            }

            public string Description;
            public string DisplayName;
            public string DisplayPublisherName;
            public string Logo;
            public string Color;
            public PackageType PackageType;
        }
    }
}