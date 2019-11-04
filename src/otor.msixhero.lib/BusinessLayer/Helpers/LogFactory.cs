using System;
using System.Management.Automation;
using System.Security.Principal;
using System.Text.RegularExpressions;
using otor.msixhero.lib.BusinessLayer.Models.Logs;

namespace otor.msixhero.lib.BusinessLayer.Helpers
{
    internal class LogFactory
    {
        public Log CreateFromPowerShellObject(PSObject source)
        {
            var log = new Log();

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
                        var reg = Regex.Match(log.Message ?? string.Empty, @"\b([\w\.]+)_[a-z0-9]{13}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        if (reg.Success)
                        {
                            log.PackageName = reg.Groups[1].Value;
                        }

                        break;
                    case "Id":
                        log.ActivityId = (int)item.Value;
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
