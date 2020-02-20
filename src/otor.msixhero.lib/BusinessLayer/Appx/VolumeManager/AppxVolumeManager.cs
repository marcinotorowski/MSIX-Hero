using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.VolumeManager
{
    public class AppxVolumeManager : IAppxVolumeManager
    {
        public async Task<AppxVolume> GetDefault(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Get-AppxDefaultVolume");

            var result = await session.InvokeAsync(progress).ConfigureAwait(false);

            var item = result.FirstOrDefault();
            if (item == null)
            {
                return null;
            }

            var baseType = item.BaseObject.GetType();
            var name = (string) baseType.GetProperty("Name")?.GetValue(item.BaseObject);
            var packageStorePath = (string) baseType.GetProperty("PackageStorePath")?.GetValue(item.BaseObject);
            return new AppxVolume {Name = name, PackageStorePath = packageStorePath};
        }

        public async Task<List<AppxVolume>> GetAll(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Get-AppxVolume");

            var results = await command.InvokeAsync().ConfigureAwait(false);
            if (!results.Any())
            {
                return new List<AppxVolume>();
            }

            var list = new List<AppxVolume>();

            foreach (var item in results)
            {
                var baseType = item.BaseObject.GetType();
                var name = (string)baseType.GetProperty("Name")?.GetValue(item.BaseObject);
                var packageStorePath = (string)baseType.GetProperty("PackageStorePath")?.GetValue(item.BaseObject);
                list.Add(new AppxVolume { Name = name, PackageStorePath = packageStorePath });
            }
            
            return list;
        }
        
        public async Task<AppxVolume> Add(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Add-AppxVolume");
            command.AddParameter("Path", Path.Join(drivePath, "WindowsApps"));

            var results = await command.InvokeAsync().ConfigureAwait(false);
            var obj = results.FirstOrDefault();
            if (obj == null)
            {
                throw new InvalidOperationException($"Volume {drivePath} could not be created.");
            }

            var baseType = obj.BaseObject.GetType();
            var name = (string)baseType.GetProperty("Name")?.GetValue(obj.BaseObject);
            var packageStorePath = (string)baseType.GetProperty("PackageStorePath")?.GetValue(obj.BaseObject);

            return new AppxVolume { Name = name, PackageStorePath = packageStorePath };
        }

        public async Task Delete(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Remove-AppxVolume");
            command.AddParameter("Volume", volume.Name);
            await command.InvokeAsync();
        }

        public async Task Delete(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var p1 = wrappedProgress.GetChildProgress(50);
            var p2 = wrappedProgress.GetChildProgress(50);
            var allVolumes = await this.GetAll(cancellationToken, p1).ConfigureAwait(false);
            
            var volume = allVolumes.FirstOrDefault(v => String.Equals(name, v.Name, StringComparison.OrdinalIgnoreCase));
            if (volume == null)
            {
                return;
            }

            await this.Delete(volume, cancellationToken, p2).ConfigureAwait(false);
        }

        public async Task SetDefault(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Set-AppxDefaultVolume");
            command.AddParameter("Volume", volume.Name);
            await session.InvokeAsync(progress).ConfigureAwait(false);
        }

        public async Task SetDefault(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var p1 = wrappedProgress.GetChildProgress(50);
            var p2 = wrappedProgress.GetChildProgress(50);
            var allVolumes = await this.GetAll(cancellationToken, p1).ConfigureAwait(false);

            if (!drivePath.EndsWith("\\"))
            {
                drivePath += "\\";
            }

            var volume = allVolumes.FirstOrDefault(v => v.PackageStorePath.StartsWith(drivePath, StringComparison.OrdinalIgnoreCase));
            if (volume == null)
            {
                return;
            }

            await this.SetDefault(volume, cancellationToken, p2).ConfigureAwait(false);
        }
    }
}
