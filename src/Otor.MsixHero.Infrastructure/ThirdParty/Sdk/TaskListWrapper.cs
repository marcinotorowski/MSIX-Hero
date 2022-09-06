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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class TaskListWrapper : ExeWrapper
    {
        private static readonly LogSource Logger = new();
        public async Task<IList<AppProcess>> GetBasicAppProcesses(string status = null, CancellationToken cancellationToken = default)
        {
            var stringBuilder = new StringBuilder();

            Action<string> callback = output => stringBuilder.AppendLine(output);
            await RunAsync(GetExecutablePath(), GetCommandLine(status, false), 0, callback, cancellationToken).ConfigureAwait(false);
            
            using var reader = new StringReader(stringBuilder.ToString());
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, 
                TrimOptions = TrimOptions.Trim
            };

            using var csv = new CsvReader(reader, csvConfiguration);
            try
            {
                return await csv.GetRecordsAsync<AppProcess>(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (BadDataException e)
            {
                Logger.Error().WriteLine(Resources.Localization.Infrastructure_Sdk_Error_TaskListInvalidDataFormat, e);
                Logger.Warn().WriteLine(Resources.Localization.Infrastructure_Sdk_Error_TaskListCsv + "\r\n" + stringBuilder);
                throw;
            }
        }

        public async Task<IList<AppProcessVerbose>> GetVerboseAppProcesses(string status = null, CancellationToken cancellationToken = default)
        {
            var stringBuilder = new StringBuilder();

            Action<string> callback = output => stringBuilder.AppendLine(output);
            await RunAsync(GetExecutablePath(), GetCommandLine(status, true), 0, callback, cancellationToken).ConfigureAwait(false);
            
            using var reader = new StringReader(stringBuilder.ToString());
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, 
                TrimOptions = TrimOptions.Trim
            };

            using var csv = new CsvReader(reader, csvConfig);
            return await csv.GetRecordsAsync<AppProcessVerbose>(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        private static string GetCommandLine(string status, bool verbose)
        {
            var sb = new StringBuilder("/apps");
            if (!string.IsNullOrEmpty(status))
            {
                sb.Append(" /fi ");
                sb.Append(CommandLineHelper.EncodeParameterArgument("status eq " + status));
            }

            sb.Append(" /fo CSV");
            if (verbose)
            {
                sb.Append(" /V");
            }

            return sb.ToString();
        }

        private static string GetExecutablePath()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var file = Path.Combine(folder, "tasklist.exe");
            return file;
        }

        public interface IAppProcess
        {
            string ImageName { get; set; }

            int ProcessId { get; set; }

            string MemoryUsage { get; set; }

            string PackageName { get; set; }
        }

        public class AppProcess : IAppProcess
        {
            [Index(0)]
            //[Name("Image Name")]
            public string ImageName { get; set; }

            [Index(1)]
            //[Name("PID")]
            public int ProcessId { get; set; }

            [Index(2)]
            //[Name("Mem Usage")]
            [Optional]
            public string MemoryUsage { get; set; }

            [Index(3)]
            //[Name("Package Name")]
            public string PackageName { get; set; }
        }

        public class AppProcessVerbose : IAppProcess
        {
            [Index(0)]
            //[Name("Image Name")]
            public string ImageName { get; set; }

            [Index(1)]
            //[Name("PID")]
            public int ProcessId { get; set; }

            [Index(2)]
            //[Name("Session Name")]
            [Optional]
            public string SessionName { get; set; }

            [Index(3)]
            //[Name("Session#")]
            [Optional]
            public int SessionNumber { get; set; }

            [Index(4)]
            //[Name("Mem Usage")]
            [Optional]
            public string MemoryUsage { get; set; }

            [Index(5)]
            //[Name("Status")]
            public string Status { get; set; }

            [Index(6)]
            //[Name("User Name")]
            [Optional]
            public string UserName { get; set; }

            [Index(7)]
            //[Name("CPU Time")]
            [Optional]
            public TimeSpan CpuTime { get; set; }

            [Index(8)]
            //[Name("Window Title")]
            [Optional]
            public string WindowTitle { get; set; }

            [Index(9)]
            //[Name("Package Name")]
            public string PackageName { get; set; }
        }
    }
}
