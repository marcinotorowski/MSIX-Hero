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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder;
using Dapplo.Log;

namespace Otor.MsixHero.Infrastructure.Services
{
    public class LocalConfigurationService : IConfigurationService, IDisposable
    {
        private static readonly LogSource Logger = new();
        private readonly AutoResetEvent lockObject = new AutoResetEvent(true);

        private Configuration.Configuration currentConfiguration;

        void IDisposable.Dispose()
        {
            this.lockObject.Dispose();
        }

        public async Task<Configuration.Configuration> GetCurrentConfigurationAsync(bool preferCached =  true, CancellationToken token = default)
        {
            if (this.currentConfiguration != null && preferCached)
            {
                return this.currentConfiguration;
            }

            var waited = this.lockObject.WaitOne();

            try
            {
                if (this.currentConfiguration != null && preferCached)
                {
                    return this.currentConfiguration;
                }

                var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "msix-hero", "config.json");

                if (!File.Exists(file))
                {
                    var cfg = FixConfiguration(new Configuration.Configuration());
                    cfg.Update = new UpdateConfiguration
                    {
                        HideNewVersionInfo = false,
                        // ReSharper disable once PossibleNullReferenceException
                        LastShownVersion = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version.ToString(3)
                    };

                    return cfg;
                }

                var fileContent = await File.ReadAllTextAsync(file, token).ConfigureAwait(false);

                try
                {
                    this.currentConfiguration = FixConfiguration(JsonConvert.DeserializeObject<Configuration.Configuration>(fileContent, new ResolvablePathConverter()));
                }
                catch (Exception e)
                {
                    this.currentConfiguration = FixConfiguration(new Configuration.Configuration());
                    Logger.Warn().WriteLine(e, Resources.Localization.Infrastructure_Settings_Error_UseDefault);
                }

                return this.currentConfiguration;
            }
            finally
            {
                if (waited)
                {
                    this.lockObject.Set();
                }
            }
        }

        public Configuration.Configuration GetCurrentConfiguration(bool preferCached = true)
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

        public async Task SetCurrentConfigurationAsync(Configuration.Configuration configuration, CancellationToken cancellationToken = default)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var waited = this.lockObject.WaitOne();
            try
            {
                var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "msix-hero", "config.json");

                var jsonString = JsonConvert.SerializeObject(configuration, new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>
                    {
                        new ResolvablePathConverter()
                    },
                    DefaultValueHandling = DefaultValueHandling.Include,
                    DateParseHandling = DateParseHandling.DateTime,
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

                var dirInfo = new FileInfo(file).Directory;
                if (dirInfo != null && !dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                await File.WriteAllTextAsync(file, jsonString, cancellationToken).ConfigureAwait(false);
                this.currentConfiguration = configuration;
            }
            finally
            {
                if (waited)
                {
                    this.lockObject.Set();
                }
            }
        }

        public void SetCurrentConfiguration(Configuration.Configuration configuration)
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

        private static Configuration.Configuration GetDefault()
        {
            var result = new Configuration.Configuration();
            result.Packages.Sidebar.Visible = true;
            result.Packages.Tools.Add(new ToolListConfiguration { Name = "Registry editor", Path = "regedit.exe", AsAdmin = true });
            result.Packages.Tools.Add(new ToolListConfiguration { Name = "Notepad", Path = "notepad.exe" });
            result.Packages.Tools.Add(new ToolListConfiguration { Name = "Command Prompt", Path = "cmd.exe" });
            result.Packages.Tools.Add(new ToolListConfiguration { Name = "PowerShell Console", Path = "powershell.exe" });
            return result;
        }

        private static Configuration.Configuration FixConfiguration(Configuration.Configuration result)
        {
            var defaults = GetDefault();

            if (result == null)
            {
                result = new Configuration.Configuration();
            }

            if (result.AppAttach == null)
            {
                result.AppAttach = defaults.AppAttach;
            }

            if (result.Signing == null)
            {
                result.Signing = defaults.Signing;
            }

            if (result.UiConfiguration == null)
            {
                result.UiConfiguration = defaults.UiConfiguration;
            }

            if (result.Signing.DefaultOutFolder == null)
            {
                result.Signing.DefaultOutFolder = defaults.Signing.DefaultOutFolder;
            }

            if (result.Packer == null)
            {
                result.Packer = defaults.Packer;
            }

            if (result.Events == null)
            {
                result.Events = defaults.Events;
            }

            if (result.Events.Filter == null)
            {
                result.Events.Filter = defaults.Events.Filter;
            }

            if (result.Events.Sorting == null)
            {
                result.Events.Sorting = defaults.Events.Sorting;
            }

            if (result.Packages == null)
            {
                result.Packages = defaults.Packages;
            }

            if (result.Packages.Tools == null || !result.Packages.Tools.Any())
            {
                result.Packages.Tools = defaults.Packages.Tools;
            }

            if (result.Packages.Sidebar == null)
            {
                result.Packages.Sidebar = defaults.Packages.Sidebar;
            }

            if (result.Packages.Filter == null)
            {
                result.Packages.Filter = defaults.Packages.Filter;
            }

            if (result.Packages.Sorting == null)
            {
                result.Packages.Sorting = defaults.Packages.Sorting;
            }

            if (result.Packages.Group == null)
            {
                result.Packages.Group = defaults.Packages.Group;
            }
            if (result.AppInstaller == null)
            {
                result.AppInstaller = defaults.AppInstaller;
            }

            return result;
        }
    }
}
