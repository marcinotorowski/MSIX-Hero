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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Enums;
using Otor.MsixHero.Appx.Resources;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations
{
    public class ServiceRecommendationAdvisor : IServiceRecommendationAdvisor
    {
        private static readonly LogSource Logger = new();
        public Task<bool> Fix(IServiceRecommendation recommendation)
        {
            switch (recommendation.Type)
            {
                case ServiceRecommendationType.Service:
                    return ChangeStatus(recommendation.ServiceName, recommendation.ExpectedToRun);
                case ServiceRecommendationType.OneTime:
                    return KillProcesses(recommendation.ServiceName);
                case ServiceRecommendationType.WindowsDefender:
                    return DefenderSettings();
                default:
                    return Task.FromException<bool>(new NotSupportedException());
            }

        }

        private Task<bool> DefenderSettings()
        {
            try
            {
                Process.Start("explorer.exe", "windowsdefender:");
                return Task.FromResult(false);
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine(e);
                throw;
            }
        }

        public Task<bool> Revert(IServiceRecommendation recommendation)
        {
            return ChangeStatus(recommendation.ServiceName, !recommendation.ExpectedToRun);
        }

        private Task<bool> KillProcesses(string name)
        {
            return Task.Run(() =>
            {
                try
                {
                    var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);

                    var psi = new ProcessStartInfo()
                    {
                        Verb = "runas",
                        UseShellExecute = true,
#if !DEBUG
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
#endif
                    };

                    psi.FileName = Path.Combine(system32, "taskkill.exe");
                    psi.Arguments = "/im \"" + name + ".exe\" /f /t";

                    var p = Process.Start(psi);
                    if (p == null)
                    {
                        return false;
                    }

                    global::System.Threading.Thread.Sleep(300);

                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        return false;
                    }

                    return Process.GetProcessesByName(name).Length == 0;
                }
                catch (Exception e)
                {
                    Logger.Error().WriteLine(e);
                    throw;
                }
            });
        }

        private Task<bool> ChangeStatus(string name, bool expectedRunState)
        {
            return Task.Run(() =>
            {
                try
                {

                    var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);

                    if (expectedRunState && IsServiceRunning(name))
                    {
                        return true;
                    }

                    if (!expectedRunState && IsServiceDisabled(name))
                    {
                        return true;
                    }

                    var psi = new ProcessStartInfo()
                    {
                        Verb = "runas",
                        UseShellExecute = true,
#if !DEBUG
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
#endif
                    };

                    if (expectedRunState)
                    {
                        if (!IsServiceDisabled(name))
                        {
                            // Expected: ACTIVE
                            // Actual: Not running, not disabled
                            // Action: Start
                            psi.FileName = Path.Combine(system32, "sc.exe");
                            psi.Arguments = "start " + name;
                        }
                        else
                        {
                            // Expected: ACTIVE
                            // Actual: Not running, disabled
                            // Action: Enable and Start
                            psi.FileName = Path.Combine(system32, "cmd.exe");
                            psi.Arguments = "/c \"sc.exe config " + name + " start= auto && sc.exe start " + name + "\"";
                        }
                    }
                    else
                    {
                        // Expected: DISABLED
                        // Actual: Not running
                        // Action: Stop and disable
                        psi.FileName = Path.Combine(system32, "cmd.exe");
                        psi.Arguments = "/c \"sc.exe stop " + name + " && sc.exe config " + name + " start= disabled\"";
                    }

                    var p = Process.Start(psi);
                    if (p == null)
                    {
                        return false;
                    }

                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        return false;
                    }

                    if (expectedRunState && IsServiceRunning(name))
                    {
                        return true;
                    }

                    if (!expectedRunState && IsServiceDisabled(name))
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception e)
                {
                    Logger.Error().WriteLine(e);
                    throw;
                }
            });
        }

        private bool IsServiceRunning(string serviceName)
        {
            try
            {
                using var serviceController = new ServiceController(serviceName);
                return serviceController.Status == ServiceControllerStatus.Running || serviceController.Status == ServiceControllerStatus.StartPending;
            }
            catch (Exception e)
            {
                Logger.Warn().WriteLine(e);
                return false;
            }
        }

        private bool IsServiceDisabled(string serviceName)
        {
            try
            {
                using var serviceController = new ServiceController(serviceName);
                return serviceController.StartType == ServiceStartMode.Disabled;
            }
            catch (Exception e)
            {
                Logger.Warn().WriteLine(e);
                return false;
            }
        }

        public IEnumerable<IServiceRecommendation> Advise(AdvisorMode mode, params string[] ignoredServiceNames)
        {
            // PLRestartMgrService","DPS","CscService","WSearch","wuauserv","CcmExec
            foreach (var serviceController in ServiceController.GetServices())
            {
                using (serviceController)
                {
                    switch (serviceController.ServiceName.ToLowerInvariant())
                    {
                        case "cscservice":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, Localization.System_Service_cscservice, false, !IsServiceDisabled(serviceController.ServiceName) || IsServiceRunning(serviceController.ServiceName));
                            break;

                        case "wsearch":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, Localization.System_Service_wsearch, false, !IsServiceDisabled(serviceController.ServiceName) || IsServiceRunning(serviceController.ServiceName));
                            break;

                        case "wuauserv":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, Localization.System_Service_wuauserv, false, !IsServiceDisabled(serviceController.ServiceName) || IsServiceRunning(serviceController.ServiceName));
                            break;

                        case "dps":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, Localization.System_Service_dps, false, !IsServiceDisabled(serviceController.ServiceName) || IsServiceRunning(serviceController.ServiceName));
                            break;
                            
                        case "ccmexec":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, Localization.System_Service_ccmexec, false, !IsServiceDisabled(serviceController.ServiceName) || IsServiceRunning(serviceController.ServiceName));
                            break;
                    }
                }
            }

            var proc = Process.GetProcessesByName("msiexec");
            if (proc.Any())
            {
                yield return new ServiceRecommendation(proc[0].ProcessName, Localization.System_Process_msiexec, Localization.System_Process_msiexec_Description, false, true, ServiceRecommendationType.OneTime);
            }

            proc = Process.GetProcessesByName("gacutil");
            if (proc.Any())
            {
                yield return new ServiceRecommendation(proc[0].ProcessName, Localization.System_Process_gacutil, Localization.System_Process_gacutil_Description, false, true, ServiceRecommendationType.OneTime);
            }

            proc = Process.GetProcessesByName("ngen");
            if (proc.Any())
            {
                yield return new ServiceRecommendation(proc[0].ProcessName, Localization.System_Process_ngen, Localization.System_Process_ngen_Description, false, true, ServiceRecommendationType.OneTime);
            }

            proc = Process.GetProcessesByName("crossgen");
            if (proc.Any())
            {
                yield return new ServiceRecommendation(proc[0].ProcessName, Localization.System_Process_crossgen, Localization.System_Process_crossgen_Description, false, true, ServiceRecommendationType.OneTime);
            }
        }
    }
}