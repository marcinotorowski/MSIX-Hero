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
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Management.Automation;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;

namespace Otor.MsixHero.Appx.Diagnostic.Logging
{
    internal class LogFactory
    {
        public Log CreateFromPowerShellObject(PSObject source, string logName)
        {
            var log = new Log
            {
                Source = logName.StartsWith("microsoft-windows-", StringComparison.OrdinalIgnoreCase) ? logName.Remove(0, "microsoft-windows-".Length) : logName
            };

            if (source.BaseObject is EventLogRecord eventLogRecord)
            {
                log.Source = eventLogRecord.ContainerLog.IndexOf('/') == -1 ? eventLogRecord.ContainerLog : eventLogRecord.ContainerLog.Remove(0, eventLogRecord.ContainerLog.IndexOf('/') + 1);
                log.ActivityId = eventLogRecord.ActivityId;
                if (eventLogRecord.TimeCreated.HasValue)
                {
                    log.DateTime = eventLogRecord.TimeCreated.Value;
                }

                log.Level = eventLogRecord.LevelDisplayName;
                log.Message = eventLogRecord.FormatDescription();


                log.ThreadId = eventLogRecord.ThreadId ?? 0;

                log.OpcodeDisplayName = eventLogRecord.OpcodeDisplayName;

                var account = (NTAccount)eventLogRecord.UserId.Translate(typeof(NTAccount));
                try
                {
                    log.User = account.ToString();
                }
                catch (Exception)
                {
                    log.User = eventLogRecord.UserId.ToString();
                }
            }
            else
            {
                var properties = source.Properties;
                if (properties == null)
                {
                    return log;
                }

                foreach (var item in properties)
                {
                    switch (item.Name)
                    {
                        case "Message":
                            log.Message = (string)item.Value;
                            break;
                        case "ThreadId":
                            log.ThreadId = (int)item.Value;
                            break;
                        case "TimeCreated":
                            log.DateTime = (DateTime)item.Value;
                            break;
                        case "LevelDisplayName":
                            log.Level = (string)item.Value;
                            break;
                        case "UserId":
                            var sid = (SecurityIdentifier)item.Value;
                            var account = (NTAccount)sid.Translate(typeof(NTAccount));
                            try
                            {
                                log.User = account.ToString();
                            }
                            catch (Exception)
                            {
                                log.User = sid.ToString();
                            }

                            break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(log.Message))
            {
                var reg = Regex.Match(log.Message, @"\b([\w\.\-]+)(?:_~)?_[a-z0-9]{13}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (reg.Success)
                {
                    log.PackageName = reg.Groups[1].Value.TrimEnd('_');
                }
                else
                {
                    reg = Regex.Match(log.Message, @"\bDeleted\\([\w\.\-]+)(?:_~)?_[a-z0-9]{13}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    if (reg.Success)
                    {
                        log.PackageName = reg.Groups[1].Value.TrimEnd('_');
                    }
                    else
                    {
                        reg = Regex.Match(log.Message, @"\b(?:file:///(?<mainpath>[^\s]+))?(?<filename>[^/\ ]+)\.(?<extension>msix|appx|appxbundle|appinstaller|msixbundle)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        if (reg.Success)
                        {
                            if (reg.Groups["mainpath"].Success)
                            {
                                log.FilePath = Uri.UnescapeDataString(reg.Groups["mainpath"].Value + reg.Groups["filename"].Value + "." + reg.Groups["extension"]).Replace('/', '\\');

                                if (!string.IsNullOrWhiteSpace(log.FilePath))
                                {
                                    log.PackageName = Path.GetFileName(log.FilePath);
                                }
                            }
                            else
                            {
                                log.FilePath = Uri.UnescapeDataString(reg.Groups["filename"].Value + "." + reg.Groups["extension"]).Replace('/', '\\');
                                log.PackageName = log.FilePath;
                            }
                        }
                    }
                }

                if (log.Level == "Error")
                {
                    reg = Regex.Match(log.Message, @"\b0x[0-9A-F]{8}\b");
                    if (reg.Success)
                    {
                        log.ErrorCode = reg.Value;
                    }
                }
            }

            return log;

            /*[0]: "Message"
            [1]: "Id"
            [2]: "Version"
            [3]: "Qualifiers"
            [4]: "Level"
            [5]: "Task"
            [6]: "Opcode"
            [7]: "Keywords"
            [8]: "RecordId"
            [9]: "ProviderName"
            [10]: "ProviderId"
            [11]: "LogName"
            [12]: "ProcessId"
            [13]: "ThreadId"
            [14]: "MachineName"
            [15]: "UserId"
            [16]: "TimeCreated"
            [17]: "ActivityId"
            [18]: "RelatedActivityId"
            [19]: "ContainerLog"
            [20]: "MatchedQueryIds"
            [21]: "Bookmark"
            [22]: "LevelDisplayName"
            [23]: "OpcodeDisplayName"
            [24]: "TaskDisplayName"
            [25]: "KeywordsDisplayNames"
            [26]: "Properties"*/
        }
    }
}
