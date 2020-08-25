using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Enums;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Lib.BusinessLayer.SystemState.Services
{
    public class ServiceRecommendationAdvisor : IServiceRecommendationAdvisor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServiceRecommendationAdvisor));

        public Task<bool> Fix(IServiceRecommendation recommendation)
        {
            switch (recommendation.Type)
            {
                case ServiceRecommendationType.Service:
                    return this.ChangeStatus(recommendation.ServiceName, recommendation.ExpectedToRun);
                case ServiceRecommendationType.OneTime:
                    return this.KillProcesses(recommendation.ServiceName);
                case ServiceRecommendationType.WindowsDefender:
                    return this.DefenderSettings();
                default:
                    throw new NotSupportedException();
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
                Logger.Error(e);
                throw;
            }
        }

        public Task<bool> Revert(IServiceRecommendation recommendation)
        {
            return this.ChangeStatus(recommendation.ServiceName, !recommendation.ExpectedToRun);
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

                    System.Threading.Thread.Sleep(300);

                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        return false;
                    }

                    return Process.GetProcessesByName(name).Length == 0;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
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

                    if (expectedRunState && this.IsServiceRunning(name))
                    {
                        return true;
                    }

                    if (!expectedRunState && this.IsServiceDisabled(name))
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
                        if (!this.IsServiceDisabled(name))
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

                    if (expectedRunState && this.IsServiceRunning(name))
                    {
                        return true;
                    }

                    if (!expectedRunState && this.IsServiceDisabled(name))
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
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
                Logger.Error(e);
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
                Logger.Error(e);
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
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "This service performs maintenance activities on the Offline Files cache. If it is active, unrelated system changes may be captured during the repackaging.", false, !this.IsServiceDisabled(serviceController.ServiceName) || this.IsServiceRunning(serviceController.ServiceName));
                            break;

                        case "wsearch":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "This service is responsible for background indexing of files and their content. If it is active, unrelated system changes may be captured during the repackaging.", false, !this.IsServiceDisabled(serviceController.ServiceName) || this.IsServiceRunning(serviceController.ServiceName));
                            break;

                        case "wuauserv":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "This service may update, install and uninstall software which is not related to the app being repackaged.", false, !this.IsServiceDisabled(serviceController.ServiceName) || this.IsServiceRunning(serviceController.ServiceName));
                            break;

                        case "dps":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "Disabling this service may improve performance and reduce the amount of unrelated captured changes in repackaged apps.", false, !this.IsServiceDisabled(serviceController.ServiceName) || this.IsServiceRunning(serviceController.ServiceName));
                            break;

                        //case "windefend":
                        //    yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "Background anti-virus activities may decrease the performance and reliability. Additionally, anti-virus tasks may cause irrelevant system changes to be captured.", false, !this.IsServiceDisabled(serviceController.ServiceName) || this.IsServiceRunning(serviceController.ServiceName), ServiceRecommendationType.WindowsDefender);
                        //    break;

                        case "ccmexec":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "System Center Configuration Manager Agent is responsible for managed installation, maintenance and removal of software. If it is active, unrelated system changes may be captured during the repackaging.", false, !this.IsServiceDisabled(serviceController.ServiceName) || this.IsServiceRunning(serviceController.ServiceName));
                            break;
                    }
                }
            }

            var proc = Process.GetProcessesByName("msiexec");
            if (proc.Any())
            {
                yield return new ServiceRecommendation(proc[0].ProcessName, "Windows Installer", "Windows Installer is running in the background which may cause unrelated system changes to be captured during the repackaging. Wait for all installations to finish before starting the repackaging.", false, true, ServiceRecommendationType.OneTime);
            }

            proc = Process.GetProcessesByName("gacutil");
            if (proc.Any())
            {
                yield return new ServiceRecommendation(proc[0].ProcessName, "Global Assembly Cache Tool", "Global Assembly Cache tool performs manipulations on the contents of the Global Assembly Cache and download cache folders. Wait for the tool to finish before starting the repackaging.", false, true, ServiceRecommendationType.OneTime);
            }

            proc = Process.GetProcessesByName("ngen");
            if (proc.Any())
            {
                yield return new ServiceRecommendation(proc[0].ProcessName, "Native Image Generator", "This tool optimizes .NET Framework assemblies by creating and installing their native images with compiled processor-specific machine code. Wait for the tool to finish before starting the repackaging.", false, true, ServiceRecommendationType.OneTime);
            }

            proc = Process.GetProcessesByName("crossgen");
            if (proc.Any())
            {
                yield return new ServiceRecommendation(proc[0].ProcessName, "CrossGen ", "This tool optimizes .NET Core assemblies by creating their native images with compiled processor-specific machine code. Wait for the tool to finish before starting the repackaging.", false, true, ServiceRecommendationType.OneTime);
            }
        }
    }
}