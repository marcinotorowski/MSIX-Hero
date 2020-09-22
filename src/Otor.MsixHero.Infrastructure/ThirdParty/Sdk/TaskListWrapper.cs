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
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class TaskListWrapper : ExeWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TaskListWrapper));

        public async Task<IList<AppProcess>> GetBasicAppProcesses(string status = null, CancellationToken cancellationToken = default)
        {
            var stringBuilder = new StringBuilder();

            Action<string> callback = output => stringBuilder.AppendLine(output);
            var exitCode = await RunAsync(GetExecutablePath(), GetCommandLine(status, false), cancellationToken, callback).ConfigureAwait(false);
            if (exitCode != 0)
            {
                throw new InvalidOperationException("Tasklist.exe returned non zero exit code.");
            }

            using (var reader = new StringReader(stringBuilder.ToString()))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.TrimOptions = TrimOptions.Trim;

                    try
                    {
                        return await csv.GetRecordsAsync<AppProcess>().ToListAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (BadDataException e)
                    {
                        Logger.Error($"Invalid data format. Index={e.ReadingContext.CurrentIndex}, CharPos={e.ReadingContext.CharPosition}, ColCount={e.ReadingContext.ColumnCount}, Field={e.ReadingContext.Field}, Row={e.ReadingContext.Row}, Raw={e.ReadingContext.RawRecord}, RawRow={e.ReadingContext.RawRow}", e);
                        Logger.Warn("CSV content:\r\n" + stringBuilder.ToString());
                        throw;
                    }
                }
            }
        }

        public async Task<IList<AppProcessVerbose>> GetVerboseAppProcesses(string status = null, CancellationToken cancellationToken = default)
        {
            var stringBuilder = new StringBuilder();

            Action<string> callback = output => stringBuilder.AppendLine(output);
            var exitCode = await RunAsync(GetExecutablePath(), GetCommandLine(status, true), cancellationToken, callback).ConfigureAwait(false);
            if (exitCode != 0)
            {
                throw new InvalidOperationException("Tasklist.exe returned non zero exit code.");
            }

            using (var reader = new StringReader(stringBuilder.ToString()))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.TrimOptions = TrimOptions.Trim;
                    return await csv.GetRecordsAsync<AppProcessVerbose>().ToListAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static string GetCommandLine(string status, bool verbose)
        {
            var sb = new StringBuilder("/apps");
            if (!string.IsNullOrEmpty(status))
            {
                sb.Append(" /fi \"status eq ");
                sb.Append(status);
                sb.Append("\"");
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
