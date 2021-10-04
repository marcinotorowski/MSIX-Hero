// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Interop;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Packaging.Installation
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class AppxPackageManager : IAppxPackageManager
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        
        public async Task<AppInstallerUpdateAvailabilityResult> CheckForUpdates(string itemPackageId, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (string.IsNullOrEmpty(itemPackageId))
            {
                return AppInstallerUpdateAvailabilityResult.Unknown;
            }

            var pkg = PackageManagerWrapper.Instance.FindPackageForUser(string.Empty, itemPackageId);
            if (pkg == null)
            {
                return AppInstallerUpdateAvailabilityResult.Unknown;
            }

            var appInstaller = pkg.GetAppInstallerInfo();
            if (appInstaller == null)
            {
                return AppInstallerUpdateAvailabilityResult.Unknown;
            }

            var u = await AsyncOperationHelper.ConvertToTask(pkg.CheckUpdateAvailabilityAsync(), cancellationToken).ConfigureAwait(false);
            if (u.Availability == PackageUpdateAvailability.Error)
            {
                if (u.ExtendedError != null)
                {
                    throw u.ExtendedError;
                }

                throw new ApplicationException("Checking for updates failed from an unspecified reason.");
            }

            return (AppInstallerUpdateAvailabilityResult)(int)u.Availability;
        }
        
        public async Task Stop(string packageFullName, CancellationToken cancellationToken = default)
        {
            Logger.Info("Stopping package " + packageFullName);

            var taskListWrapper = new TaskListWrapper();
            IList<TaskListWrapper.AppProcess> processes;
            try
            {
                processes = await taskListWrapper.GetBasicAppProcesses(null, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not get the list of running processes.", e);
            }

            var processIds = processes.Where(p => p.PackageName == packageFullName).Select(p => p.ProcessId).ToArray();
            Logger.Info("The package appears to have currently {0} associated running processes with the following PIDs: {1}.", processIds.Length, string.Join(", ", processIds));
            foreach (var pid in processIds)
            {
                Logger.Info("Killing process PID = " + pid + " and its children...");

                try
                {
                    var p = Process.GetProcessById(pid);
                    p.Kill(true);
                }
                catch (ArgumentException)
                {
                    Logger.Warn("Could not find the process PID = " + pid + ". Most likely, the process has been already killed or stopped.");
                }
                catch (Exception e)
                {
                    Logger.Warn("Could not kill process PID = " + pid + ".", e);
                }
            }
        }
    }
}