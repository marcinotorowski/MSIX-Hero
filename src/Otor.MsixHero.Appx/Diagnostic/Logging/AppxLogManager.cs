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
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Appx.Diagnostic.Logging.Enums;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.PowerShell;

namespace Otor.MsixHero.Appx.Diagnostic.Logging
{
    public class AppxLogManager : IAppxLogManager
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        public async Task<List<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Getting last {0} log files...", maxCount);

            var allLogs = new List<Log>();

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
                    progresses[index] = wrapperProgress.GetChildProgress();
                }

                var allTasks = new List<Task<IList<Log>>>();
                for (var index = 0; index < logNames.Length; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var item = logNames[index];
                    var itemProgress = progresses[index];
                    
                    allTasks.Add(this.GetLogsFromProvider(item, maxCount, cancellationToken, itemProgress));
                }

                await Task.WhenAll(allTasks).ConfigureAwait(false);

                foreach (var item in allTasks)
                {
                    allLogs.AddRange(await item.ConfigureAwait(false));
                }
            }

            if (maxCount > 0)
            {
                return allLogs.OrderByDescending(l => l.DateTime).Take(maxCount).ToList();
            }

            return allLogs;
        }

        public Task OpenEventViewer(EventLogCategory type, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            string logName;
            switch (type)
            {
                case EventLogCategory.AppXDeploymentOperational:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeployment%4Operational.evtx";
                    break;
                case EventLogCategory.AppXDeploymentServerOperational:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeploymentServer%4Operational.evtx";
                    break;
                case EventLogCategory.AppXDeploymentServerRestricted:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeploymentServer%4Restricted.evtx";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            logName = Environment.ExpandEnvironmentVariables(logName);
            var process = new ProcessStartInfo("eventvwr", $"/l:\"{logName}\"")
            {
                Verb = "runas",
                UseShellExecute = true
            };

            Process.Start(process);
            return Task.FromResult(true);
        }

        private async Task<IList<Log>> GetLogsFromProvider(
            PSObject provider,
            int maxCount = 0,
            CancellationToken cancellationToken = default, IProgress<ProgressData> itemProgress = null)
        {
            var factory = new LogFactory();
            cancellationToken.ThrowIfCancellationRequested();
            var logName = (string)provider.BaseObject;

            itemProgress?.Report(new ProgressData(0, "Reading " + logName + "..."));

            cancellationToken.ThrowIfCancellationRequested();

            using var psLocal = await PowerShellSession.CreateForModule().ConfigureAwait(false);
            var scriptContent = "Get-WinEvent -ProviderName " + logName + " -ErrorAction SilentlyContinue";
            if (maxCount > 0)
            {
                scriptContent += " -MaxEvents " + maxCount;
            }

            using var scriptLocal = psLocal.AddScript(scriptContent);

            itemProgress?.Report(new ProgressData(10, "Getting events, please wait..."));
            using var logItems = await psLocal.InvokeAsync(itemProgress).ConfigureAwait(false);
            itemProgress?.Report(new ProgressData(70, "Getting events, please wait..."));
            return logItems.Select(log => factory.CreateFromPowerShellObject(log, logName)).ToList();
        }
    }
}