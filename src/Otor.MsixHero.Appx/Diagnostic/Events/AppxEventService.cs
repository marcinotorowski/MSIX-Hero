// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Appx.Diagnostic.Events.Entities;
using Otor.MsixHero.Appx.Diagnostic.Events.Enums;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Diagnostic.Events
{
    public class AppxEventService : IAppxEventService
    {
        private static readonly LogSource Logger = new();

        public async Task<List<AppxEvent>> GetEvents(EventCriteria criteria, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (criteria.MaxCount.HasValue || (criteria.TimeSpan.HasValue && criteria.TimeSpan.Value != LogCriteriaTimeSpan.All))
            {
                Logger.Info().WriteLine("Getting events with criteria…", criteria.MaxCount);

                if (criteria.MaxCount.HasValue)
                {
                    Logger.Info().WriteLine(" * Max number of entries: {0}", criteria.MaxCount);
                }

                if (criteria.TimeSpan.HasValue && criteria.TimeSpan.Value != LogCriteriaTimeSpan.All)
                {
                    switch (criteria.TimeSpan)
                    {
                        case LogCriteriaTimeSpan.LastHour:
                            Logger.Info().WriteLine(" * Start date: {0}", DateTime.Now.Subtract(TimeSpan.FromHours(1)));
                            break;
                        case LogCriteriaTimeSpan.LastDay:
                            Logger.Info().WriteLine(" * Start date: {0}", DateTime.Now.Subtract(TimeSpan.FromDays(1)));
                            break;
                        case LogCriteriaTimeSpan.LastWeek:
                            Logger.Info().WriteLine(" * Start date: {0}", DateTime.Now.Subtract(TimeSpan.FromDays(7)));
                            break;
                    }
                }
            }
            else
            {
                Logger.Info().WriteLine("Getting events without criteria…", criteria.MaxCount);
            }

            var allLogs = new ConcurrentBag<AppxEvent>();

            IDictionary<SecurityIdentifier, string> cachedUsers = new Dictionary<SecurityIdentifier, string>();

            using var eventLogSession = new EventLogSession();

            // ReSharper disable once AccessToDisposedClosure
            var validLogNames = await Task.Run(() => eventLogSession.GetLogNames().Where(IsLogKnown).ToList(), cancellationToken).ConfigureAwait(false);
            var processingTasks = new List<Task>();

            const int maxTasksAtTime = 5;
            using (var wrapperProgress = new WrappedProgress(progress ?? new Progress<ProgressData>()))
            {
                var progresses = validLogNames.ToDictionary(v => v, _ => wrapperProgress.GetChildProgress());

                foreach (var validLogName in validLogNames)
                {
                    while (processingTasks.Count > maxTasksAtTime)
                    {
                        var finished = await Task.WhenAny(processingTasks).ConfigureAwait(false);
                        processingTasks.Remove(finished);
                    }

                    var localProgress = progresses[validLogName];
                    var task = ProcessLog(allLogs, eventLogSession, validLogName, criteria, cachedUsers, cancellationToken, localProgress);
                    processingTasks.Add(task);
                }

                await Task.WhenAll(processingTasks).ConfigureAwait(false);
            }
            
            if (criteria.MaxCount.HasValue && allLogs.Count > criteria.MaxCount.Value)
            {
                return allLogs.OrderByDescending(l => l.DateTime).Take(criteria.MaxCount.Value).ToList();
            }

            return allLogs.ToList();
        }

        private static bool IsLogKnown(string logName)
        {
            switch (logName)
            {
                case AppxEventSources.DeploymentDiagnostic:
                case AppxEventSources.DeploymentOperational:
                case AppxEventSources.PackagingDebug:
                case AppxEventSources.PackagingOperational:
                case AppxEventSources.PackagingPerformance:
                case AppxEventSources.DeploymentServerDebug:
                case AppxEventSources.DeploymentServerRestricted:
                case AppxEventSources.DeploymentServerDiagnostic:
                case AppxEventSources.DeploymentServerOperational:
                    return true;
                default:
                    return false;
            }
        }

        public Task OpenEventViewer(EventCategory type, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            string logName;
            switch (type)
            {
                case EventCategory.AppXDeploymentOperational:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeployment%4Operational.evtx";
                    break;
                case EventCategory.AppXDeploymentServerOperational:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeploymentServer%4Operational.evtx";
                    break;
                case EventCategory.AppXDeploymentServerRestricted:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeploymentServer%4Restricted.evtx";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            logName = Environment.ExpandEnvironmentVariables(logName);
            var process = new ProcessStartInfo("eventvwr", $"/l:{CommandLineHelper.EncodeParameterArgument(logName, true)}")
            {
                Verb = "runas",
                UseShellExecute = true
            };

            Process.Start(process);
            return Task.FromResult(true);
        }

        private static async Task ProcessLog(
            ConcurrentBag<AppxEvent> writingTo,
            EventLogSession session,
            string logName,
            EventCriteria eventCriteria,
            IDictionary<SecurityIdentifier, string> cachedUsers,
            CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            progress.Report(new ProgressData(0, string.Format(Resources.Localization.Events_Reading, logName)));
            
            if (string.Equals(AppxEventSources.DeploymentServerRestricted, logName, StringComparison.OrdinalIgnoreCase))
            {
                if (!await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                {
                    progress.Report(new ProgressData(100, string.Format(Resources.Localization.Events_Reading, logName)));
                    return;
                }
            }

            var logObj = new EventLogConfiguration(logName, session);
            
            var info = session.GetLogInformation(logName, PathType.LogName);
            var progressCount = info.RecordCount;
            if (!progressCount.HasValue)
            {
                progressCount = 0;
            }

            if (eventCriteria.MaxCount.HasValue)
            {
                progressCount = Math.Min(eventCriteria.MaxCount.Value, progressCount.Value);
            }

            // var providerMetadata = new ProviderMetadata(logObj.OwningProviderName, eventLogSession, CultureInfo.CurrentCulture);

            var elq = new EventLogQuery(logObj.LogName, PathType.LogName);


            using var elr = new EventLogReader(elq);

            DateTime? startDate = default;
            if (eventCriteria.TimeSpan.HasValue)
            {
                switch (eventCriteria.TimeSpan)
                {
                    case LogCriteriaTimeSpan.LastHour:
                        startDate = DateTime.Now.Subtract(TimeSpan.FromHours(1));
                        break;
                    case LogCriteriaTimeSpan.LastDay:
                        startDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                        break;
                    case LogCriteriaTimeSpan.LastWeek:
                        startDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
                        break;
                }
            }

            // ReSharper disable once AccessToDisposedClosure
            await using (cancellationToken.Register(() => elr.CancelReading()))
            {
                var processed = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (eventCriteria.MaxCount.HasValue && processed >= eventCriteria.MaxCount.Value)
                    {
                        break;
                    }

                    // ReSharper disable once AccessToDisposedClosure
                    var eventLogRecord = ExceptionGuard.Guard(() => elr.ReadEvent());
                    if (eventLogRecord == null)
                    {
                        Logger.Debug().WriteLine("Skipping here, because we reached EOF.");
                        break;
                    }

                    // Let's assume the dates are chronological (why wouldn't they be?)
                    // Revisit this.
                    if (startDate.HasValue && eventLogRecord.TimeCreated.HasValue && eventLogRecord.TimeCreated.Value < startDate.Value)
                    {
                        // The log is older than the requested time.
                        continue;
                    }

                    processed++;

                    var currentProgress = progressCount == 0 ? 0 : Math.Min(100, (int)(100.0 * processed / progressCount));
                    progress.Report(new ProgressData(currentProgress, string.Format(Resources.Localization.Events_Reading, logName)));

                    var newLog = new AppxEvent
                    {
                        ActivityId = eventLogRecord.ActivityId
                    };

                    if (eventLogRecord.TimeCreated.HasValue)
                    {
                        newLog.DateTime = eventLogRecord.TimeCreated.Value;
                    }

                    newLog.Source = logObj.LogName;
                    newLog.Type = eventLogRecord.Level == null ? AppxEventType.Information : (AppxEventType)eventLogRecord.Level;
                    newLog.Message = eventLogRecord.FormatDescription();

                    newLog.ThreadId = eventLogRecord.ThreadId ?? 0;

                    newLog.OpcodeDisplayName = eventLogRecord.OpcodeDisplayName.TrimEnd();

                    if (!cachedUsers.TryGetValue(eventLogRecord.UserId, out var account))
                    {
                        account = ExceptionGuard.Guard(() => ((NTAccount)eventLogRecord.UserId.Translate(typeof(NTAccount))).ToString());
                        if (account != null)
                        {
                            cachedUsers[eventLogRecord.UserId] = account;
                        }
                    }

                    if (account == null)
                    {
                        account = eventLogRecord.UserId.ToString();
                        cachedUsers[eventLogRecord.UserId] = account;
                    }

                    newLog.User = account;


                    if (!string.IsNullOrWhiteSpace(newLog.Message))
                    {
                        var reg = Regex.Match(newLog.Message, @"\b([\w\.\-]+)(?:_~)?_[a-z0-9]{13}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        if (reg.Success)
                        {
                            newLog.PackageName = reg.Groups[1].Value.TrimEnd('_');
                        }
                        else
                        {
                            reg = Regex.Match(newLog.Message, @"\bDeleted\\([\w\.\-]+)(?:_~)?_[a-z0-9]{13}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            if (reg.Success)
                            {
                                newLog.PackageName = reg.Groups[1].Value.TrimEnd('_');
                            }
                            else
                            {
                                reg = Regex.Match(newLog.Message, @"\b(?:file:///(?<mainpath>[^\s]+))?(?<filename>[^/\ ]+)\.(?<extension>msix|appx|appxbundle|appinstaller|msixbundle)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                if (reg.Success)
                                {
                                    if (reg.Groups["mainpath"].Success)
                                    {
                                        newLog.FilePath = Uri.UnescapeDataString(reg.Groups["mainpath"].Value + reg.Groups["filename"].Value + "." + reg.Groups["extension"]).Replace('/', '\\');

                                        if (!string.IsNullOrWhiteSpace(newLog.FilePath))
                                        {
                                            newLog.PackageName = Path.GetFileName(newLog.FilePath);
                                        }
                                    }
                                    else
                                    {
                                        newLog.FilePath = Uri.UnescapeDataString(reg.Groups["filename"].Value + "." + reg.Groups["extension"]).Replace('/', '\\');
                                        newLog.PackageName = newLog.FilePath;
                                    }
                                }
                            }
                        }

                        // note: look for exit codes in hexadecimal format. Exclude ones following the string "with flags".
                        reg = Regex.Match(newLog.Message, @"(?<!with flags )\b0[xX][0-9A-Fa-f]{1,8}\b");
                        if (reg.Success)
                        {
                            newLog.ErrorCode = reg.Value;
                        }
                    }

                    writingTo.Add(newLog);
                }
            }

            progress.Report(new ProgressData(100, string.Format(Resources.Localization.Events_Reading, logName)));
        }
    }
}