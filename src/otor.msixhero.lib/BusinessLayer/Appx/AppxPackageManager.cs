using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Management.Deployment;
using Microsoft.Win32;
using NLog.LayoutRenderers;
using otor.msixhero.lib.BusinessLayer.Appx.DeveloperMode;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Manifest.Summary;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Exceptions;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;
using LogFactory = otor.msixhero.lib.BusinessLayer.Helpers.LogFactory;

namespace otor.msixhero.lib.BusinessLayer.Appx
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class AppxPackageManager : IAppxPackageManager
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        protected readonly ISideloadingChecker SideloadingChecker = new RegistrySideloadingChecker();

        private readonly IAppxSigningManager signingManager;

        public AppxPackageManager(IAppxSigningManager signingManager)
        {
            this.signingManager = signingManager;
        }

        public Task<List<User>> GetUsersForPackage(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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

        public async Task Remove(IReadOnlyCollection<InstalledPackage> packages, bool forAllUsers = false,
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
                Logger.Info("Removing {0}", item.PackageId);

                var task = AsyncOperationHelper.ConvertToTask(
                    mmm.RemovePackageAsync(item.PackageId,
                        forAllUsers ? RemovalOptions.RemoveForAllUsers : RemovalOptions.None),
                    "Removing " + item.DisplayName, CancellationToken.None, progress);
                await task.ConfigureAwait(false);

                if (item.IsProvisioned && forAllUsers)
                {
                    await this.Deprovision(item.PackageFamilyName, cancellationToken, progress).ConfigureAwait(false);
                }
            }
        }

        public async Task Deprovision(string packageFamilyName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var mmm = new PackageManager();
            var task = AsyncOperationHelper.ConvertToTask(
                mmm.DeprovisionPackageForAllUsersAsync(packageFamilyName),
                "De-provisioning for all users",
                CancellationToken.None, progress);
            await task.ConfigureAwait(false);
        }

        public async Task Add(string filePath, AddPackageOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (this.SideloadingChecker.GetStatus() < SideloadingStatus.Sideloading)
            {
                throw new DeveloperModeException("Developer mode or sideloading must be enabled to install packages outside of Microsoft Store.");
            }

            Logger.Info("Installing package {0}", filePath);
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.Equals(Path.GetFileName(filePath), "AppxManifest.xml", StringComparison.OrdinalIgnoreCase))
            {
                var reader = await AppxManifestSummaryBuilder.FromManifest(filePath).ConfigureAwait(false);
                var pkgManager = new PackageManager();

                DeploymentOptions deploymentOptions = 0;

                if (options.HasFlag(AddPackageOptions.AllowDowngrade))
                {
                    deploymentOptions |= DeploymentOptions.ForceUpdateFromAnyVersion;
                }

                if (options.HasFlag(AddPackageOptions.KillRunningApps))
                {
                    deploymentOptions |= DeploymentOptions.ForceApplicationShutdown;
                    deploymentOptions |= DeploymentOptions.ForceTargetApplicationShutdown;
                }

                deploymentOptions |= DeploymentOptions.DevelopmentMode;

                await AsyncOperationHelper.ConvertToTask(
                    pkgManager.RegisterPackageAsync(new Uri(filePath), Enumerable.Empty<Uri>(), deploymentOptions),
                    $"Installing {reader.DisplayName} {reader.Version}...",
                    cancellationToken,
                    progress).ConfigureAwait(false);
            }
            else if (string.Equals(".appinstaller", Path.GetExtension(filePath), StringComparison.OrdinalIgnoreCase))
            {
                var pkgManager = new PackageManager();

                if (options.HasFlag(AddPackageOptions.AllUsers))
                {
                    throw new NotSupportedException("Cannot install a package from .appinstaller for all users.");
                }

                AddPackageByAppInstallerOptions deploymentOptions = 0;

                if (options.HasFlag(AddPackageOptions.AllowDowngrade))
                {
                    throw new NotSupportedException("Cannot force a downgrade with .appinstaller. The .appinstaller defines on its own whether the downgrade is allowed.");
                }

                if (options.HasFlag(AddPackageOptions.KillRunningApps))
                {
                    deploymentOptions |= AddPackageByAppInstallerOptions.ForceTargetAppShutdown;
                }

                var volume = pkgManager.GetDefaultPackageVolume();
                await AsyncOperationHelper.ConvertToTask(
                    pkgManager.AddPackageByAppInstallerFileAsync(new Uri(filePath, UriKind.Absolute), deploymentOptions, volume),
                    "Installing from " + Path.GetFileName(filePath) + "...",
                    cancellationToken,
                    progress).ConfigureAwait(false);
            }
            else
            {
                try
                {
                    var reader = await AppxManifestSummaryBuilder.FromMsix(filePath).ConfigureAwait(false);
                    var pkgManager = new PackageManager();

                    DeploymentOptions deploymentOptions = 0;

                    if (options.HasFlag(AddPackageOptions.AllowDowngrade))
                    {
                        deploymentOptions |= DeploymentOptions.ForceUpdateFromAnyVersion;
                    }

                    if (options.HasFlag(AddPackageOptions.KillRunningApps))
                    {
                        deploymentOptions |= DeploymentOptions.ForceApplicationShutdown;
                        deploymentOptions |= DeploymentOptions.ForceTargetApplicationShutdown;
                    }

                    if (options.HasFlag(AddPackageOptions.AllUsers))
                    {
                        var deploymentResult = await AsyncOperationHelper.ConvertToTask(
                            pkgManager.AddPackageAsync(new Uri(filePath, UriKind.Absolute), Enumerable.Empty<Uri>(), deploymentOptions),
                            $"Installing {reader.DisplayName} {reader.Version}...",
                            cancellationToken,
                            progress).ConfigureAwait(false);

                        if (!deploymentResult.IsRegistered)
                        {
                            throw new InvalidOperationException("The package could not be registered.");
                        }

                        var findInstalled = pkgManager.FindPackages(reader.Name, reader.Publisher).FirstOrDefault();
                        if (findInstalled == null)
                        {
                            throw new InvalidOperationException("The package could not be registered.");
                        }

                        var familyName = findInstalled.Id.FamilyName;

                        await AsyncOperationHelper.ConvertToTask(
                            pkgManager.ProvisionPackageForAllUsersAsync(familyName),
                            $"Provisioning {reader.DisplayName} {reader.Version}...",
                            cancellationToken,
                            progress).ConfigureAwait(false);
                    }
                    else
                    {
                        var deploymentResult = await AsyncOperationHelper.ConvertToTask(
                            pkgManager.AddPackageAsync(new Uri(filePath, UriKind.Absolute), Enumerable.Empty<Uri>(), deploymentOptions),
                            "Installing " + reader.DisplayName + "...",
                            cancellationToken,
                            progress).ConfigureAwait(false);

                        if (!deploymentResult.IsRegistered)
                        {
                            throw new InvalidOperationException("The package could not be registered.");
                        }
                    }
                }
                catch (InvalidDataException e)
                {
                    throw new InvalidOperationException("File " + filePath + " does not seem to be a valid MSIX package." + e, e);
                }
            }
        }

        public Task RunToolInContext(InstalledPackage package, string toolPath, string arguments, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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
            if (this.SideloadingChecker.GetStatus() != SideloadingStatus.DeveloperMode)
            {
                throw new DeveloperModeException("Developer mode must be enabled to use this feature.");
            }

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

                if (e.HResult == -2146233087)
                {
                    throw new AdminRightsRequiredException("This tool requires admin rights.", e);
                }

                throw;
            }
        }

        public Task<RegistryMountState> GetRegistryMountState(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.GetRegistryMountState(package.InstallLocation, package.Name);
        }

        public async Task<List<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Getting last {0} log files...", maxCount);

            //var cmd = "((Get-WinEvent -ListLog *Microsoft-Windows-*Appx* -ErrorAction SilentlyContinue).LogName).Split(\" \")";

            //progress?.Report(new ProgressData(0, "Querying log names..."));
            //var proc = new ProcessStartInfo("powershell.exe", "-NoLogo -WindowStyle Hidden -Command \"&{ " + cmd + "}\"")
            //{
            //    UseShellExecute = false,
            //    CreateNoWindow = true,
            //    RedirectStandardOutput = true,
            //    StandardOutputEncoding = Encoding.UTF8
            //};

            //var p = Process.Start(proc);
            //if (p == null)
            //{
            //    throw new InvalidOperationException("Could not start PowerShell.");
            //}

            //p.WaitForExit(15000);

            //var logNames = p.StandardOutput.ReadToEnd().Split().Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            //var singleProgress = logNames.Length == 0 ? 0 : 66.0 / logNames.Length;
            //var totalProgress = 20.0;

            var factory = new LogFactory();
            var allLogs = new List<Log>();

            //foreach (var item in logNames)
            //{
            //    progress?.Report(new ProgressData((int)totalProgress, $"Processing {item}..."));
            //    cmd = "Get-WinEvent -LogName " + item + " -MaxEvents 100 | Select-Object -Property Message,Id,ThreadId, TimeCreated,LevelDisplayName,UserId";
            //    proc = new ProcessStartInfo("powershell.exe", "-NoLogo -WindowStyle Hidden -Command \"&{ " + cmd + "}\"")
            //    {
            //        UseShellExecute = false,
            //        CreateNoWindow = true,
            //        RedirectStandardOutput = true,
            //        StandardOutputEncoding = Encoding.UTF8
            //    };

            //    p = Process.Start(proc);
            //    if (p == null)
            //    {
            //        throw new InvalidOperationException("Could not start PowerShell.");
            //    }

            //    p.WaitForExit(20000);

            //    while (!p.StandardOutput.EndOfStream)
            //    {
            //        var line = await p.StandardOutput.ReadLineAsync().ConfigureAwait(false);

            //        if (!line.StartsWith("Message"))
            //        {
            //            continue;
            //        }

            //        var message = line.Substring(line.IndexOf(':') + 2);
            //        line = await p.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            //        while (!line.StartsWith("Id"))
            //        {
            //            message += " " + line.Trim();
            //            line = await p.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            //        }

            //        var id = line.Substring(line.IndexOf(':') + 2);
            //        line = await p.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            //        var threadId = line.Substring(line.IndexOf(':') + 2);
            //        line = await p.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            //        var timeCreated = line.Substring(line.IndexOf(':') + 2);
            //        line = await p.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            //        var levelDisplayName = line.Substring(line.IndexOf(':') + 2);
            //        line = await p.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            //        var userId = line.Substring(line.IndexOf(':') + 2);

            //        allLogs.Add(factory.CreateFromValues(message, id, threadId, timeCreated, levelDisplayName, userId));
            //    }
            //}

            //return allLogs.OrderBy(l => l.DateTime).Take(maxCount).ToList();


            using var ps = await PowerShellSession.CreateForModule().ConfigureAwait(false);
            using var script = ps.AddScript("(Get-WinEvent -ListLog *Microsoft-Windows-*Appx* -ErrorAction SilentlyContinue).LogName");
            using var logs = await ps.InvokeAsync().ConfigureAwait(false);

            var logNames = logs.ToArray();

            foreach (var item in logNames)
            {
                using var psLocal = await PowerShellSession.CreateForModule().ConfigureAwait(false);
                var logName = (string)item.BaseObject;
                using var scriptLocal = psLocal.AddScript("Get-WinEvent -LogName " + logName + " -MaxEvents " + maxCount);
                using var logItems = await psLocal.InvokeAsync().ConfigureAwait(false);

                allLogs.AddRange(logItems.Select(log => factory.CreateFromPowerShellObject(log)));
            }

            return allLogs.OrderByDescending(l => l.DateTime).Take(maxCount).ToList();
        }

        public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            RegistryMountState hasRegistry;

            if (string.IsNullOrEmpty(installLocation))
            {
                hasRegistry = RegistryMountState.NotApplicable;
            }
            else if (!File.Exists(Path.Combine(installLocation, "registry.dat")))
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

        public Task UnmountRegistry(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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

        public Task MountRegistry(InstalledPackage package, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.MountRegistry(package.Name, package.InstallLocation, startRegedit);
        }

        public Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Task.Run(() =>
            {
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

        public Task Run(InstalledPackage package, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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
                FileName = entryPoint
            };

            p.StartInfo = startInfo;
            p.Start();
            return Task.FromResult(true);
        }

        public Task<List<InstalledPackage>> GetInstalledPackages(PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.GetInstalledPackages(null, mode, cancellationToken, progress);
        }

        public async Task<List<InstalledPackage>> GetPackagesByFamilyName(string packageFamilyName, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
        {
            var pkgMan = new PackageManager();
            var allPackages = await Task.Run(() => pkgMan.FindPackagesForUser(string.Empty, packageFamilyName).ToList(), cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var result = new List<InstalledPackage>();
            foreach (var item in allPackages)
            {
                var converted = await ConvertFrom(item, cancellationToken, progress).ConfigureAwait(false);
                converted.Context = PackageContext.CurrentUser;
                result.Add(converted);
            }

            return result;
        }

        public async Task<InstalledPackage> GetInstalledPackages(string packageName, string publisher, PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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

            var converted = await ConvertFrom(pkg, cancellationToken, progress).ConfigureAwait(false);
            converted.Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers;
            return converted;
        }

        public async Task<AppxPackage> GetByIdentity(string packageName, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var reader = new PackageIdentityFileReaderAdapter(mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers, packageName);
            var manifestReader = new AppxManifestReader();
            // ReSharper disable once AccessToDisposedClosure
            var package = await Task.Run(() => manifestReader.Read(reader, true, cancellationToken), cancellationToken).ConfigureAwait(false);
            return package;
        }

        public async Task<AppxPackage> GetByManifestPath(string manifestPath, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using IAppxFileReader reader = new FileInfoFileReaderAdapter(manifestPath);
            var manifestReader = new AppxManifestReader();
            // ReSharper disable once AccessToDisposedClosure
            var package = await Task.Run(() => manifestReader.Read(reader, true, cancellationToken), cancellationToken).ConfigureAwait(false);
            return package;
        }

        private async Task<List<InstalledPackage>> GetInstalledPackages(string packageName, PackageFindMode mode, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
        {
            var list = new List<InstalledPackage>();
            var provisioned = new HashSet<string>();

            var isAdmin = UserHelper.IsAdministrator();
            if (mode == PackageFindMode.Auto)
            {
                mode = isAdmin ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
            }

            var pkgMan = new PackageManager();


            progress?.Report(new ProgressData(0, "Getting packages..."));

            if (isAdmin)
            {
                Logger.Info("Getting provisioned packages...");
                var tempFile = Path.GetTempFileName();
                try
                {
                    var cmd = "(Get-AppxProvisionedPackage -Online).PackageName | Out-File '" + tempFile + "'";
                    var proc = new ProcessStartInfo("powershell.exe", "-NoLogo -WindowStyle Hidden -Command \"&{ " + cmd + "}\"")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Logger.Debug("Executing powershell.exe " + "-Command \"&{ " + cmd + "}\"");
                    var p = Process.Start(proc);
                    if (p == null)
                    {
                        Logger.Error("Could not get the list of provisioned apps.");
                    }
                    else
                    {
                        p.WaitForExit();
                        foreach (var line in await File.ReadAllLinesAsync(tempFile, cancellationToken).ConfigureAwait(false))
                        {
                            provisioned.Add(line.Replace("~", string.Empty));
                        }
                    }
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }

                /*
                pkgMan = new PackageManager();


                try
                {
                    using (var ps = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false))
                    {
                        Logger.Info("Getting provisioned packages...");
                        var cmd = ps.AddCommand("Get-AppxProvisionedPackage");
                        cmd.AddParameter("Online");

                        var provisionedPackages = await ps.InvokeAsync(progress).ConfigureAwait(false);
                        foreach (var pp in provisionedPackages.ReadAll())
                        {
                            var props1 = pp.Properties["PackageName"];
                            if (props1?.Value == null)
                            {
                                continue;
                            }

                            provisioned.Add(props1.Value.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    
                }*/
            }

            progress?.Report(new ProgressData(5, "Getting packages..."));

            IList<Windows.ApplicationModel.Package> allPackages;

            if (string.IsNullOrEmpty(packageName))
            {
                Logger.Info("Getting all packages by find mode = '{0}'", mode);
                switch (mode)
                {
                    case PackageFindMode.CurrentUser:
                        allPackages = await Task.Run(() => pkgMan.FindPackagesForUser(string.Empty).ToList(), cancellationToken).ConfigureAwait(false);
                        break;
                    case PackageFindMode.AllUsers:
                        allPackages = await Task.Run(() => pkgMan.FindPackages().ToList(), cancellationToken).ConfigureAwait(false);
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
                            await Task.Run(() => pkgMan.FindPackageForUser(string.Empty, packageName), cancellationToken).ConfigureAwait(false)
                        };

                        break;
                    case PackageFindMode.AllUsers:
                        allPackages = new List<Windows.ApplicationModel.Package>
                        {
                            await Task.Run(() => pkgMan.FindPackage(packageName), cancellationToken).ConfigureAwait(false)
                        };

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }

            progress?.Report(new ProgressData(10, "Getting packages..."));

            const double weight = 0.9;

            var total = (1.0 - weight) * 100.0;
            var single = allPackages.Count == 0 ? 0.0 : 100.0 * weight / allPackages.Count;

            foreach (var item in allPackages)
            {
                total += single;
                progress?.Report(new ProgressData((int)total, "Getting packages..."));

                var converted = await ConvertFrom(item, cancellationToken, progress).ConfigureAwait(false);
                if (converted != null)
                {
                    converted.Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers;
                    if (provisioned.Contains(item.Id.FullName))
                    {
                        converted.IsProvisioned = true;
                    }

                    list.Add(converted);
                }
            }

            Logger.Info("Returning {0} packages...", list.Count);
            return list;
        }

        private async Task<InstalledPackage> ConvertFrom(Windows.ApplicationModel.Package item, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
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
                installLocation = null;
            }

            if (installLocation != null)
            {
                try
                {
                    installDate = item.InstalledDate.LocalDateTime;
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Installed date for package {0} is invalid. This may be expected for some installed packages.", item.Id.Name);
                    installDate = DateTime.MinValue;
                }
            }
            else
            {
                installDate = DateTime.MinValue;
            }

            MsixPackageVisuals details;
            RegistryMountState hasRegistry;

            if (installLocation == null)
            {
                hasRegistry = RegistryMountState.NotApplicable;
                details = new MsixPackageVisuals(item.Id.Name, item.Id.Publisher, null, null, "#000000", 0);
            }
            else
            {
                details = await GetVisualsFromManifest(installLocation, cancellationToken).ConfigureAwait(false);
                hasRegistry = await this.GetRegistryMountState(installLocation, item.Id.Name, cancellationToken, progress).ConfigureAwait(false);
            }

            var pkg = new InstalledPackage()
            {
                DisplayName = details.DisplayName,
                Name = item.Id.Name,
                Image = details.Logo,
                PackageId = item.Id.FullName,
                InstallLocation = installLocation,
                PackageFamilyName = item.Id.FamilyName,
                Description = details.Description,
                DisplayPublisherName = details.DisplayPublisherName,
                Publisher = item.Id.Publisher,
                IsFramework = item.IsFramework,
                TileColor = details.Color,
                PackageType = details.PackageType,
                Version = new Version(item.Id.Version.Major, item.Id.Version.Minor, item.Id.Version.Build, item.Id.Version.Revision),
                SignatureKind = Convert(item.SignatureKind),
                HasRegistry = hasRegistry,
                InstallDate = installDate
            };

            if (installLocation != null && (pkg.DisplayName?.StartsWith("ms-resource:", StringComparison.Ordinal) ??
                pkg.DisplayPublisherName?.StartsWith("ms-resource:", StringComparison.Ordinal) ??
                pkg.Description?.StartsWith("ms-resource:", StringComparison.Ordinal) == true))
            {
                var priFile = Path.Combine(installLocation, "resources.pri");

                var appId = pkg.Name;
                pkg.DisplayName = StringLocalizer.Localize(priFile, appId, pkg.DisplayName);
                pkg.DisplayPublisherName = StringLocalizer.Localize(priFile, appId, pkg.DisplayPublisherName);
                pkg.Description = StringLocalizer.Localize(priFile, appId, pkg.Description);
            }

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

        private static async Task<MsixPackageVisuals> GetVisualsFromManifest(string installLocation, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var reader = await AppxManifestSummaryBuilder.FromInstallLocation(installLocation).ConfigureAwait(false);
                var logo = Path.Combine(installLocation, reader.Logo);

                if (File.Exists(Path.Combine(installLocation, logo)))
                {
                    return new MsixPackageVisuals(
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
                return new MsixPackageVisuals(reader.DisplayName, reader.DisplayPublisher, logo, reader.Description, reader.AccentColor, reader.PackageType);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return new MsixPackageVisuals();
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
                    return SignatureKind.Unsigned;
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

        public struct MsixPackageVisuals
        {
            public MsixPackageVisuals(string displayName, string displayPublisherName, string logo, string description, string color, MsixPackageType packageType)
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
            public MsixPackageType PackageType;
        }
    }
}