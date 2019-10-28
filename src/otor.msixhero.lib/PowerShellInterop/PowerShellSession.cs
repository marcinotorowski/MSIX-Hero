using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace otor.msixhero.lib.PowerShellInterop
{
    public class PowerShellSession
    {
        public static async Task<PowerShell> CreateForModule(string module = null, bool skipEditionCheck = false)
        {
            var ps = PowerShell.Create();
            ps.AddCommand("Set-ExecutionPolicy");
            ps.AddParameter("ExecutionPolicy", "ByPass");
            ps.AddParameter("Scope", "Process");
            await ps.InvokeAsync().ConfigureAwait(false);
            ps.Commands.Clear();

            if (!string.IsNullOrEmpty(module))
            {
                ps.AddCommand("Import-Module");
                ps.AddParameter("Name", module);
                if (skipEditionCheck)
                {
                    ps.AddParameter("SkipEditionCheck");
                }

                await ps.InvokeAsync().ConfigureAwait(false);
                ps.Commands.Clear();
            }

            return ps;
        }

        public static Task<PowerShell> CreateForAppxModule()
        {
            return CreateForModule("Appx");
        }
    }
}
