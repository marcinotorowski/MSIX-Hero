using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        public Task<Configuration> GetConfiguration(CancellationToken token)
        {
            var result = new Configuration();
            result.List.Sidebar.Visible = true;

            result.List.Tools.Add(new ToolListConfiguration() { Name = "Registry editor", Path = "regedit.exe" });
            result.List.Tools.Add(new ToolListConfiguration() { Name = "Notepad", Path = "notepad.exe" });
            result.List.Tools.Add(new ToolListConfiguration() { Name = "Command Prompt", Path = "cmd.exe" });
            result.List.Tools.Add(new ToolListConfiguration() { Name = "PowerShell Console", Path = "powershell.exe" });

            return Task.FromResult(result);
        }
    }
}
