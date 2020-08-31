using System;
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
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Exceptions;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Interop;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.ThirdParty.PowerShell;
using LogFactory = Otor.MsixHero.Appx.Diagnostic.Logging.LogFactory;

namespace Otor.MsixHero.Appx.Packaging.Installation
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class AppxPackageManager : IAppxPackageManager
    {
        protected readonly ISideloadingChecker SideloadingChecker = new RegistrySideloadingChecker();

        private static readonly ILog Logger = LogManager.GetLogger();
        private readonly IRegistryManager registryManager;
        private readonly IConfigurationService configurationService;

        public AppxPackageManager(IRegistryManager registryManager, IConfigurationService configurationService)
        {
            this.registryManager = registryManager;
            this.configurationService = configurationService;
        }

        public Task<List<User>> GetUsersForPackage(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.GetUsersForPackage(package.Name, cancellationToken, progress);
        }

        public async Task<List<User>> GetUsersForPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Getting users who installed package {0}...", packageName);
            if (!await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
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
        
        public async Task Remove(IReadOnlyCollection<InstalledPackage> packages, bool forAllUsers = false, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
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
                    await Deprovision(item.PackageFamilyName, cancellationToken, progress).ConfigureAwait(false);
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
            if (SideloadingChecker.GetStatus() < SideloadingStatus.Sideloading)
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

        public async Task RunToolInContext(InstalledPackage package, string toolPath, string arguments, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (toolPath == null)
            {
                throw new ArgumentNullException(nameof(toolPath));
            }

            using (IAppxFileReader reader = new FileInfoFileReaderAdapter(package.ManifestLocation))
            {
                var maniReader = new AppxManifestReader();
                var manifest = await maniReader.Read(reader, cancellationToken).ConfigureAwait(false);

                if (!manifest.Applications.Any())
                {
                    throw new InvalidOperationException("Cannot start tool on a package without applications.");
                }
                
                await RunToolInContext(package.PackageFamilyName, manifest.Applications[0].Id, toolPath, arguments, cancellationToken, progress).ConfigureAwait(false);
            }
        }

        public async Task RunToolInContext(string packageFamilyName, string appId, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (SideloadingChecker.GetStatus() != SideloadingStatus.DeveloperMode)
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
            using var script = ps.AddScript("(Get-WinEvent -ListLog *Microsoft-Windows-*Appx* -ErrorAction SilentlyContinue).ProviderNames");
            using var logs = await ps.InvokeAsync().ConfigureAwait(false);

            var logNames = logs.ToArray();
            
            var progresses = new IProgress<ProgressData>[logNames.Length];

            using (var wrapperProgress = new WrappedProgress(progress ?? new Progress<ProgressData>()))
            {
                for (var index = 0; index < logNames.Length; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progresses[index] = wrapperProgress.GetChildProgress(100.0);
                }

                for (var index = 0; index < logNames.Length; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var item = logNames[index];
                    var itemProgress = progresses[index];
                    var logName = (string) item.BaseObject;
                    itemProgress.Report(new ProgressData(0, "Reading " + logName + "..."));

                    cancellationToken.ThrowIfCancellationRequested();

                    using var psLocal = await PowerShellSession.CreateForModule().ConfigureAwait(false);
                    using var scriptLocal = psLocal.AddScript("Get-WinEvent -ProviderName " + logName + " -MaxEvents " + maxCount + " -ErrorAction SilentlyContinue");
                    using var logItems = await psLocal.InvokeAsync(itemProgress).ConfigureAwait(false);
                    
                    allLogs.AddRange(logItems.Select(log => factory.CreateFromPowerShellObject(log)));
                }
            }

            return allLogs.OrderByDescending(l => l.DateTime).Take(maxCount).ToList();
        }

        public Task Run(InstalledPackage package, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Run(package.ManifestLocation, package.PackageFamilyName, appId, cancellationToken, progress);
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

        public async Task<List<InstalledPackage>> GetModificationPackages(string packageFullName, PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var pkgManager = new PackageManager();

            if (mode == PackageFindMode.Auto)
            {
                mode = (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false)) ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
            }

            var find = await Task.Run(() => mode == PackageFindMode.CurrentUser ? pkgManager.FindPackageForUser(string.Empty, packageFullName) : pkgManager.FindPackage(packageFullName), cancellationToken).ConfigureAwait(false);
            if (find == null)
            {
                var packageIdentity = PackageIdentity.FromFullName(packageFullName);
                find = await Task.Run(() => mode == PackageFindMode.CurrentUser ? pkgManager.FindPackageForUser(string.Empty, packageIdentity.AppName) : pkgManager.FindPackage(packageIdentity.AppName), cancellationToken).ConfigureAwait(false);

                if (find == null)
                {
                    return new List<InstalledPackage>();
                }
            }

            var dependencies = find.Dependencies;

            var list = new List<InstalledPackage>();

            foreach (var dep in dependencies.Where(p => p.IsOptional))
            {
                var converted = await ConvertFrom(dep, cancellationToken).ConfigureAwait(false);
                list.Add(converted);
            }

            return list;
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

            var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false);
            if (mode == PackageFindMode.Auto)
            {
                mode = isAdmin ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
            }

            var pkgMan = new PackageManager();


            progress?.Report(new ProgressData(0, "Reading packages..."));

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
            }

            progress?.Report(new ProgressData(5, "Reading packages..."));

            IList<Windows.ApplicationModel.Package> allPackages;

            if (string.IsNullOrEmpty(packageName))
            {
                Logger.Info("Getting all packages by find mode = '{0}'", mode);
                switch (mode)
                {
                    case PackageFindMode.CurrentUser:
                        allPackages = await Task.Run(() => pkgMan.FindPackagesForUserWithPackageTypes(string.Empty, PackageTypes.Framework | PackageTypes.Main | PackageTypes.Optional).ToList(), cancellationToken).ConfigureAwait(false);
                        break;
                    case PackageFindMode.AllUsers:
                        allPackages = await Task.Run(() => pkgMan.FindPackagesWithPackageTypes(PackageTypes.Framework | PackageTypes.Main | PackageTypes.Optional).ToList(), cancellationToken).ConfigureAwait(false);
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

            progress?.Report(new ProgressData(30, "Reading packages..."));

            var total = 10.0;
            var single = allPackages.Count == 0 ? 0.0 : 90.0 / allPackages.Count;

            var cnt = 0;
            var all = allPackages.Count;
            
            var tasks = new HashSet<Task<InstalledPackage>>();
            var config = await this.configurationService.GetCurrentConfigurationAsync(true, cancellationToken).ConfigureAwait(false);

            int maxThreads;
            if (config.Advanced?.DisableMultiThreadingForGetPackages == false || config.Advanced?.MaxThreadsForGetPackages < 2)
            {
                maxThreads = 1;
            }
            else if (config.Advanced?.MaxThreadsForGetPackages == null)
            {
                maxThreads = Environment.ProcessorCount;
            }
            else
            {
                maxThreads = Math.Min(config.Advanced?.MaxThreadsForGetPackages ?? 1, Environment.ProcessorCount);
            }

            var sw = new Stopwatch();
            sw.Start();

            foreach (var item in allPackages)
            {
                cnt += 1;
                total += single;
                progress?.Report(new ProgressData((int)total, $"Reading package {cnt} out of {all}..."));

                cancellationToken.ThrowIfCancellationRequested();
                
                if (tasks.Count >= maxThreads) 
                {
                    var awaited = await Task.WhenAny(tasks).ConfigureAwait(false);
                    tasks.Remove(awaited);

                    var converted = await awaited.ConfigureAwait(false);
                    if (converted != null)
                    {
                        converted.Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers;
                        if (provisioned.Contains(converted.PackageId))
                        {
                            converted.IsProvisioned = true;
                        }

                        list.Add(converted);
                    }
                }

                tasks.Add(ConvertFrom(item, cancellationToken, progress));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            foreach (var item in tasks)
            {
                var converted = await item.ConfigureAwait(false);

                if (converted != null)
                {
                    converted.Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers;
                    if (provisioned.Contains(converted.PackageId))
                    {
                        converted.IsProvisioned = true;
                    }

                    list.Add(converted);
                }
            }

            sw.Stop();

            Logger.Info("Returning {0} packages (the operation took {1})...", list.Count, sw.Elapsed);
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
                hasRegistry = await this.registryManager.GetRegistryMountState(installLocation, item.Id.Name, cancellationToken, progress).ConfigureAwait(false);
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
                Architecture = item.Id.Architecture.ToString(),
                IsFramework = item.IsFramework,
                IsOptional = item.IsOptional,
                TileColor = details.Color,
                PackageType = details.PackageType,
                Version = new Version(item.Id.Version.Major, item.Id.Version.Minor, item.Id.Version.Build, item.Id.Version.Revision),
                SignatureKind = Convert(item.SignatureKind),
                HasRegistry = hasRegistry,
                InstallDate = installDate
            };

            if (pkg.Architecture[0] == 'X')
            {
                pkg.Architecture = "x" + pkg.Architecture.Substring(1);
            }

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

                // ReSharper disable once AssignNullToNotNullAttribute
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
                DisplayName = displayName;
                DisplayPublisherName = displayPublisherName;
                Logo = logo;
                Description = description;
                Color = color;
                PackageType = packageType;
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