using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using otor.msixhero.lib.Infrastructure.Configuration.ResolvableFolder;
using otor.msixhero.lib.Infrastructure.Logging;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public class LocalConfigurationService : IConfigurationService
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        private Configuration currentConfiguration;

        public async Task<Configuration> GetCurrentConfigurationAsync(bool preferCached =  true, CancellationToken token = default)
        {
            if (this.currentConfiguration != null && preferCached)
            {
                return this.currentConfiguration;
            }

            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "msix-hero", "config.json");

            if (!File.Exists(file))
            {
                return GetDefault();
            }

            var fileContent = await File.ReadAllTextAsync(file, token).ConfigureAwait(false);

            try
            {
                this.currentConfiguration = FixConfiguration(JsonConvert.DeserializeObject<Configuration>(fileContent, new ResolvableFolderConverter()));
            }
            catch (Exception e)
            {
                this.currentConfiguration = GetDefault();
                Logger.Warn(e, "Could not read the settings. Default settings will be used.");
            }

            return this.currentConfiguration;
        }

        public Configuration GetCurrentConfiguration(bool preferCached = true)
        {
            try
            {
                return this.GetCurrentConfigurationAsync(preferCached, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                throw e.GetBaseException();
            }
        }

        public async Task SetCurrentConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "msix-hero", "config.json");

            var jsonString = JsonConvert.SerializeObject(configuration, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new ResolvableFolderConverter()
                },
                DefaultValueHandling = DefaultValueHandling.Include,
                DateParseHandling = DateParseHandling.DateTime,
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.Indented
            });

            var dirInfo = new FileInfo(file).Directory;
            if (dirInfo != null && !dirInfo.Exists)
            {
                dirInfo.Create();
            }

            await File.WriteAllTextAsync(file, jsonString, cancellationToken).ConfigureAwait(false);
            this.currentConfiguration = configuration;
        }

        public void SetCurrentConfiguration(Configuration configuration)
        {
            try
            {
                this.SetCurrentConfigurationAsync(configuration, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                throw e.GetBaseException();
            }
        }

        private static Configuration GetDefault()
        {
            var result = FixConfiguration(new Configuration());
            result.List.Sidebar.Visible = true;
            result.List.Tools.Add(new ToolListConfiguration() { Name = "Registry editor", Path = "regedit.exe" });
            result.List.Tools.Add(new ToolListConfiguration() { Name = "Notepad", Path = "notepad.exe" });
            result.List.Tools.Add(new ToolListConfiguration() { Name = "Command Prompt", Path = "cmd.exe" });
            result.List.Tools.Add(new ToolListConfiguration() { Name = "PowerShell Console", Path = "powershell.exe" });
            return result;
        }

        private static Configuration FixConfiguration(Configuration result)
        {
            var defaults = new Configuration();

            if (result.Signing == null)
            {
                result.Signing = defaults.Signing;
            }

            if (result.Signing.DefaultOutFolder == null)
            {
                result.Signing.DefaultOutFolder = defaults.Signing.DefaultOutFolder;
            }

            if (result.List == null)
            {
                result.List = defaults.List;
            }

            if (result.List.Sidebar == null)
            {
                result.List.Sidebar = defaults.List.Sidebar;
            }

            return result;
        }
    }
}
