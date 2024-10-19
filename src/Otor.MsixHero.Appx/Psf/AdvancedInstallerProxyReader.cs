using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;
using Registry;

namespace Otor.MsixHero.Appx.Psf;

public class AdvancedInstallerProxyReader(
    string applicationId,
    string advancedInstallerProxyFileName,
    IAppxFileReader fileReader)
    : ApplicationProxyReader
{
    private readonly string _advancedInstallerProxyFileName = advancedInstallerProxyFileName;
    private readonly IAppxFileReader _fileReader = fileReader;

    public override async Task<BaseApplicationProxy> Inspect(CancellationToken cancellationToken = default)
    {
        // This is an old Advanced Installer stuff
        if (this._fileReader.FileExists("Registry.dat"))
        {
            RegistryHiveOnDemand reg;
            await using (var stream = this._fileReader.GetFile("Registry.dat"))
            {
                if (stream is FileStream fileStream)
                {
                    reg = new RegistryHiveOnDemand(fileStream.Name);
                }
                else
                {
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                    await memoryStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    reg = new RegistryHiveOnDemand(memoryStream.ToArray(), "Registry.dat");
                }
            }

            var key = reg.GetKey(@"root\registry\machine\software\caphyon\advanced installer\" + applicationId);
            if (key?.Values != null)
            {
                var psfDef = new AdvancedInstallerApplicationProxy();

                foreach (var item in key.Values.Where(item => item.ValueName != null))
                {
                    switch (item.ValueName.ToLowerInvariant())
                    {
                        case "path":
                            psfDef.Executable = (item.ValueData ?? string.Empty).Replace("[{AppVPackageRoot}]\\", string.Empty);
                            break;
                        case "pathai":
                            psfDef.Executable = (item.ValueData ?? string.Empty).Replace("[{AppVPackageRoot}]\\", string.Empty);
                            break;
                        case "workingdirectory":
                            psfDef.WorkingDirectory = (item.ValueData ?? string.Empty).Replace("[{AppVPackageRoot}]\\", string.Empty);
                            break;
                        case "workingdirectoryai":
                            psfDef.WorkingDirectory = (item.ValueData ?? string.Empty).Replace("[{AppVPackageRoot}]\\", string.Empty);
                            break;
                        case "args":
                            psfDef.Arguments = item.ValueData;
                            break;
                    }
                }

                if (string.IsNullOrWhiteSpace(psfDef.Executable))
                {
                    psfDef.Executable = null;
                }
                else if (psfDef.Executable.StartsWith("[{", StringComparison.OrdinalIgnoreCase))
                {
                    var indexOfClosing = psfDef.Executable.IndexOf("}]", StringComparison.OrdinalIgnoreCase);
                    if (indexOfClosing != -1)
                    {
                        var middlePart = psfDef.Executable.Substring(2, indexOfClosing - 2);
                        var testedPath = "VFS\\" + middlePart + psfDef.Executable.Substring(indexOfClosing + 2);

                        if (this._fileReader.FileExists(testedPath))
                        {
                            // this is to make sure that a path like [{ProgramFilesX86}]\test is replaced to VFS\ProgramFilesX86\test if present
                            psfDef.Executable = testedPath;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(psfDef.WorkingDirectory))
                {
                    psfDef.WorkingDirectory = null;
                }

                if (string.IsNullOrWhiteSpace(psfDef.Arguments))
                {
                    psfDef.Arguments = null;
                }

                return psfDef;
            }
        }

        return null;
    }
}