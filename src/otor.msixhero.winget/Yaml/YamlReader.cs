using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Winget.Yaml.Entities;
using YamlDotNet.Serialization;

namespace Otor.MsixHero.Winget.Yaml
{
    public class YamlReader
    {
        public async Task<YamlDefinition> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using (var textReader = new StreamReader(stream, leaveOpen: true))
            {
                return await this.ReadAsync(textReader, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<string> ValidateAsync(string yamlPath, IAppxPackageManager packageManager, bool throwIfWingetMissing = false, CancellationToken cancellationToken = default)
        {
            var pkgMan = new PackageManager();
            var pkg = await Task.Run(() => pkgMan.FindPackagesForUser(string.Empty, "Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe").FirstOrDefault(), cancellationToken).ConfigureAwait(false);
            if (pkg == null)
            {
                pkg = await Task.Run(() => pkgMan.FindPackagesForUser(string.Empty, "Microsoft.WindowsTerminal_8wekyb3d8bbwe").FirstOrDefault(), cancellationToken).ConfigureAwait(false);
            }

            if (pkg == null)
            {
                if (throwIfWingetMissing)
                {
                    throw new FileNotFoundException("winget not found.", "winget");
                }

                return null;
            }

            const string cmd = "cmd.exe";
            var outputPath = Path.GetTempFileName();
            var args = $"/c winget validate \"{yamlPath}\" >> {outputPath}";
            try
            {
                var psi = new ProcessStartInfo(cmd, args)
                {
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };

                var p = Process.Start(psi);
                if (p == null)
                {
                    if (throwIfWingetMissing)
                    {
                        throw new InvalidOperationException("Could not start winget.");
                    }

                    return null;
                }

                p.WaitForExit();

                return await File.ReadAllTextAsync(outputPath, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
            }
        }

        public async Task<YamlDefinition> ReadAsync(string yamlContent, CancellationToken cancellationToken = default)
        {
            using (var textReader = new StringReader(yamlContent))
            {
                return await this.ReadAsync(textReader, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task<YamlDefinition> ReadAsync(TextReader textReader, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                var deserializerBuilder = new DeserializerBuilder().IgnoreUnmatchedProperties();
                var deserializer = deserializerBuilder.Build();
                return deserializer.Deserialize<YamlDefinition>(textReader);
            },
            cancellationToken);
        }

        public YamlDefinition Read(Stream stream)
        {
            using (var textReader = new StreamReader(stream, leaveOpen: true))
            {
                return this.Read(textReader);
            }
        }

        public YamlDefinition Read(string yamlContent)
        {
            using (var textReader = new StringReader(yamlContent))
            {
                return this.Read(textReader);
            }
        }

        public YamlDefinition Read(TextReader textReader)
        {
            var deserializerBuilder = new DeserializerBuilder().IgnoreUnmatchedProperties();
            var deserializer = deserializerBuilder.Build();
            return deserializer.Deserialize<YamlDefinition>(textReader);
        }
    }
}
