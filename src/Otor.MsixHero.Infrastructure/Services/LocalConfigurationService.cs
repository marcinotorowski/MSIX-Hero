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
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder;
using Dapplo.Log;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Infrastructure.Configuration.Migrations;

namespace Otor.MsixHero.Infrastructure.Services
{
    public class LocalConfigurationService : IConfigurationService, IDisposable
    {
        private static readonly LogSource Logger = new();

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
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
        };

        private readonly AutoResetEvent _lockObject = new AutoResetEvent(true);

        private Configuration.Configuration _currentConfiguration;

        void IDisposable.Dispose()
        {
            this._lockObject.Dispose();
        }

        public async Task<Configuration.Configuration> GetCurrentConfigurationAsync(bool preferCached =  true, CancellationToken token = default)
        {
            if (this._currentConfiguration != null && preferCached)
            {
                return this._currentConfiguration;
            }

            var waited = this._lockObject.WaitOne();

            try
            {
                if (this._currentConfiguration != null && preferCached)
                {
                    return this._currentConfiguration;
                }

                var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "msix-hero", "config.json");

                if (!File.Exists(file))
                {
                    this._currentConfiguration = new Configuration.Configuration();
                }
                else
                {
                    try
                    {
                        await using var fileStream = File.OpenRead(file);
                        using var textReader = new StreamReader(fileStream);
                        using var jsonReader = new JsonTextReader(textReader);
                        var json = await JObject.LoadAsync(jsonReader, token).ConfigureAwait(false);

                        var jsonSerializer = new JsonSerializer();
                        jsonSerializer.Converters.Add(new ResolvablePathConverter());

                        this._currentConfiguration = json.ToObject<Configuration.Configuration>(jsonSerializer);
                    }
                    catch (Exception e)
                    {
                        this._currentConfiguration = new Configuration.Configuration();
                        Logger.Warn().WriteLine(e, Resources.Localization.Infrastructure_Settings_Error_UseDefault);
                    }

                    var migrations = new Migration(this._currentConfiguration);
                    migrations.Migrate();
                }

                return this._currentConfiguration;
            }
            finally
            {
                if (waited)
                {
                    this._lockObject.Set();
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

            var waited = this._lockObject.WaitOne();
            try
            {
                var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "msix-hero", "config.json");

                var jsonString = JsonConvert.SerializeObject(configuration, SerializerSettings);

                var dirInfo = new FileInfo(file).Directory;
                if (dirInfo != null && !dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                await File.WriteAllTextAsync(file, jsonString, cancellationToken).ConfigureAwait(false);
                this._currentConfiguration = configuration;
            }
            finally
            {
                if (waited)
                {
                    this._lockObject.Set();
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
    }
}
