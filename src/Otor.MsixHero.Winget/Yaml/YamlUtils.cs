// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Winget.Helpers;
using Otor.MsixHero.Winget.Yaml.Entities;

namespace Otor.MsixHero.Winget.Yaml
{
    public class YamlUtils
    {
        public async Task<string> CalculateHashAsync(FileInfo fileInfo, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("File does not exist.", fileInfo.FullName);
            }

            using var stream = File.OpenRead(fileInfo.FullName);

            cancellationToken.ThrowIfCancellationRequested();

            return await CalculateHashAsync(stream, fileInfo.Length, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> CalculateSignatureHashAsync(Uri url, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var progressForDownload = wrappedProgress.GetChildProgress(85);
            var progressForHashing = wrappedProgress.GetChildProgress(15);

            var webRequest = (HttpWebRequest) WebRequest.Create(url);
            using var response = webRequest.GetResponse();
            var tempFileName = Path.Combine(Path.GetTempPath(), "msixhero-" + Guid.NewGuid().ToString("N").Substring(0, 8) + FileConstants.MsixExtension);

            try
            {
                // ReSharper disable once UseAwaitUsing
                using (var fs = File.OpenWrite(tempFileName))
                {
                    var buffer = new byte[4096];

                    await using var responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new InvalidOperationException("Could not download the file.");
                    }

                    int read;

                    var totalSize = response.ContentLength;
                    var processed = 0L;
                    var lastFlush = 0L;
                    const long bufferFlushing = 1024 * 1024 * 10; // 10 MB

                    while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        processed += read;

                        if (totalSize > 0)
                        {
                            var p = (int)(100.0 * processed / totalSize);
                            progressForDownload.Report(new ProgressData(p, $"Downloading... ({p}%)"));
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                        await fs.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);

                        if (processed + bufferFlushing > lastFlush)
                        {
                            await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                            lastFlush = processed;
                        }
                    }

                    await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                }

                return await this.CalculateSignatureHashAsync(new FileInfo(tempFileName), cancellationToken, progressForHashing).ConfigureAwait(false);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        public async Task<string> CalculateSignatureHashAsync(FileInfo fileInfo, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var ext = Path.GetExtension(fileInfo.FullName);

            switch (ext.ToLowerInvariant())
            {
                case FileConstants.AppxExtension:
                case FileConstants.MsixExtension:
                case FileConstants.AppxBundleExtension:
                case FileConstants.MsixBundleExtension:
                {
                    using IAppxFileReader src = new ZipArchiveFileReaderAdapter(fileInfo.FullName);
                    if (!src.FileExists("AppxSignature.p7x"))
                    {
                        throw new ArgumentException($"The file '{fileInfo.Name}' does not contain a signature.", nameof(fileInfo));
                    }
                        
                    await using var appxSignature = src.GetFile("AppxSignature.p7x");
                    var buffer = new byte[ushort.MaxValue];
                    var read = await appxSignature.ReadAsync(buffer, 0, ushort.MaxValue, cancellationToken).ConfigureAwait(false);
                                
                    var builder = new StringBuilder();

                    using var sha = SHA256.Create();
                    foreach (var b in sha.ComputeHash(buffer, 0, read))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        builder.Append(b.ToString("X2"));
                    }

                    return builder.ToString();
                }
            }

            var directory = fileInfo.Directory;
            // ReSharper disable once PossibleNullReferenceException
            var signatureInfo = new FileInfo(Path.Combine(directory.FullName, "AppxSignature.p7x"));
            if (signatureInfo.Exists)
            {
                return await CalculateHashAsync(signatureInfo, cancellationToken, progress).ConfigureAwait(false);
            }

            throw new ArgumentException("Only MSIX/APPX formats support signature footprints.", nameof(fileInfo));
        }

        public async Task<string> CalculateHashAsync(Uri file, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var httpReq = (HttpWebRequest) WebRequest.Create(file);
            var response = await httpReq.GetResponseAsync().ConfigureAwait(false);
            if (response == null)
            {
                throw new InvalidOperationException("Could not download the file " + file);
            }

            cancellationToken.ThrowIfCancellationRequested();
            using var responseStream = response.GetResponseStream();
            if (responseStream == null)
            {
                throw new InvalidOperationException("Could not download the file " + file);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return await CalculateHashAsync(responseStream, response.ContentLength, cancellationToken, progress).ConfigureAwait(false);
        }

        private static async Task<string> CalculateHashAsync(Stream source, long? responseLength, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            using var sha256 = SHA256.Create();

            var hashing = new AsyncHashing(sha256);
            // ReSharper disable once AccessToDisposedClosure
            var byteHash = await hashing.ComputeHashAsync(source, responseLength, cancellationToken, progress).ConfigureAwait(false);

            var builder = new StringBuilder();
            foreach (var b in byteHash)
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.Append(b.ToString("X2"));
            }

            return builder.ToString();
        }

        public async Task<YamlManifest> CreateFromFile(string filePath, CancellationToken cancellationToken = default)
        {
            var detector = new InstallerTypeDetector();
            var detected = await detector.DetectSetupType(filePath, cancellationToken).ConfigureAwait(false);

            YamlManifest yaml;

            switch (detected)
            {
                case YamlInstallerType.Msi:
                    yaml = await Task.Run(() => this.CreateFromMsi(filePath), cancellationToken).ConfigureAwait(false);
                    break;
                case YamlInstallerType.None:
                case YamlInstallerType.Exe:
                case YamlInstallerType.InnoSetup:
                case YamlInstallerType.Nullsoft:
                case YamlInstallerType.Wix:
                    yaml = await Task.Run(() => this.CreateFromExe(filePath), cancellationToken).ConfigureAwait(false);
                    yaml.Installers[0].InstallerType = detected;
                    break;
                case YamlInstallerType.Msix:
                case YamlInstallerType.Appx:
                    yaml = await this.CreateFromMsix(filePath, cancellationToken).ConfigureAwait(false);
                    yaml.Installers[0].InstallerType = detected;
                    yaml.MinimumOperatingSystemVersion = Version.Parse("10.0.0");
                    break;
                default:
                    yaml = new YamlManifest
                    {
                        Installers = new List<YamlInstaller>
                        {
                            new YamlInstaller()
                        }
                        
                    };
                    break;
            }

            yaml.PackageIdentifier = (yaml.Publisher + "." + yaml.PackageName).Replace(" ", string.Empty);
            yaml.Installers[0].InstallerSha256 = await this.CalculateHashAsync(new FileInfo(filePath), cancellationToken).ConfigureAwait(false);
            
            if (yaml.Copyright != null && yaml.Copyright.IndexOf("Copy", StringComparison.OrdinalIgnoreCase) == -1 && yaml.Copyright.IndexOf("(C)", StringComparison.OrdinalIgnoreCase) == -1 && yaml.Copyright.IndexOf("http", StringComparison.OrdinalIgnoreCase) == -1)
            {
                yaml.Copyright = "Copyright (C) " + yaml.Copyright;
            }

            return yaml;
        }

        private YamlManifest CreateFromMsi(string filePath)
        {
            var msiProps = new Msi().GetProperties(filePath);

            var lcid = msiProps.TryGetValue("ProductLanguage", out var language) ? language : null;
            
            var yamlDefinition = new YamlManifest
            {
                PackageName = msiProps.TryGetValue("ProductName", out var name  ) ? name   : null,
                PackageVersion = msiProps.TryGetValue("ProductVersion", out var version ) ? version  : null,
                Publisher = msiProps.TryGetValue("Manufacturer", out var publisher) ? publisher : null,
                ShortDescription = msiProps.TryGetValue("ARPCOMMENTS", out var comments ) ? comments  : null,
                PublisherUrl = msiProps.TryGetValue("ARPCONTACT", out var contactUrl) ? contactUrl : null,
                CopyrightUrl = msiProps.TryGetValue("ARPURLINFOABOUT", out var url) ? url : null,
                PublisherSupportUrl = msiProps.TryGetValue("ARPHELPLINK", out var supportUrl) ? supportUrl : null,
                Installers = new List<YamlInstaller>
                {
                    new YamlInstaller
                    {
                        InstallerType = YamlInstallerType.Msi,
                        Scope = msiProps.TryGetValue("ALLUSERS", out var allUsers) && allUsers == "1" ? YamlScope.Machine : YamlScope.None,
                        Architecture = msiProps.TryGetValue("Template", out var template) && (template.IndexOf("Intel64", StringComparison.OrdinalIgnoreCase) != -1 || template.IndexOf("x64", StringComparison.OrdinalIgnoreCase) != -1 || template.IndexOf("amd64", StringComparison.OrdinalIgnoreCase) != -1) ? YamlArchitecture.X64 : YamlArchitecture.X86,
                        Platform = new List<YamlPlatform>() { YamlPlatform.WindowsDesktop },
                        ProductCode = msiProps.TryGetValue("ProductCode", out var productCode) ? productCode : null,
                        // InstallModes = new List<YamlInstallMode> {YamlInstallMode.Interactive, YamlInstallMode.Silent, YamlInstallMode.SilentWithProgress },
                    }
                }
            };

            if (lcid != null && int.TryParse(lcid, out var lcidCode))
            {
                try
                {
                    var culture = CultureInfo.GetCultureInfo(lcid);
                    yamlDefinition.PackageLocale = culture.Name.ToLowerInvariant();
                    yamlDefinition.Installers[0].InstallerLocale = culture.Name.ToLowerInvariant();
                }
                catch
                {
                    // could not get culture by LCID.
                }
            }

            if (yamlDefinition.Copyright != null)
            {
                yamlDefinition.Copyright = yamlDefinition.Copyright.Trim();
                if (string.IsNullOrWhiteSpace(yamlDefinition.Copyright))
                {
                    yamlDefinition.Copyright = null;
                }

                yamlDefinition.Copyright = yamlDefinition.Copyright.Trim();
                if (string.IsNullOrWhiteSpace(yamlDefinition.CopyrightUrl))
                {
                    yamlDefinition.CopyrightUrl = null;
                }
            }
            
            return yamlDefinition;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private YamlManifest CreateFromExe(string filePath)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
            var yamlDefinition = new YamlManifest
            {
                PackageName = fileVersionInfo.ProductName?.Trim(),
                PackageVersion = fileVersionInfo.ProductVersion?.Trim(),
                Publisher = fileVersionInfo.CompanyName?.Trim(),
                ShortDescription = fileVersionInfo.FileDescription?.Trim(),
                License = fileVersionInfo.LegalCopyright ?? fileVersionInfo.LegalTrademarks,
                Installers = new List<YamlInstaller>
                {
                    new YamlInstaller
                    {
                        InstallerType = YamlInstallerType.Exe
                    }
                }
            };

            if (yamlDefinition.Copyright != null)
            {
                yamlDefinition.Copyright = yamlDefinition.Copyright.Trim();
                if (string.IsNullOrWhiteSpace(yamlDefinition.Copyright))
                {
                    yamlDefinition.Copyright = null;
                }
            }

            using var fs = File.OpenRead(filePath);
            using var binaryReader = new BinaryReader(fs);
            var mz = binaryReader.ReadUInt16();
            if (mz != 0x5a4d)
            {
                return yamlDefinition;
            }
            
            fs.Position = 60; // this location contains the offset for the PE header
            var offset = binaryReader.ReadUInt32();

            fs.Position = offset + sizeof(uint); // contains the architecture
            var machine = binaryReader.ReadUInt16();

            if (machine == 0x8664) // IMAGE_FILE_MACHINE_AMD64
            {
                yamlDefinition.Installers[0].Architecture = YamlArchitecture.X64;
            }
            else if (machine == 0x014c) // IMAGE_FILE_MACHINE_I386
            {
                yamlDefinition.Installers[0].Architecture = YamlArchitecture.X86;
            }
            else if (machine == 0x0200) // IMAGE_FILE_MACHINE_IA64
            {
                yamlDefinition.Installers[0].Architecture = YamlArchitecture.X64;
            }

            yamlDefinition.Installers[0].Platform = new List<YamlPlatform>() { YamlPlatform.WindowsDesktop };
            return yamlDefinition;
        }

        private async Task<YamlManifest> CreateFromMsix(string filePath, CancellationToken cancellationToken = default)
        {
            var yamlDefinition = new YamlManifest()
            {
                Installers = new List<YamlInstaller>
                {
                    new YamlInstaller
                    {
                        Scope = YamlScope.User,
                        InstallerType = YamlInstallerType.Msix
                    }
                }
            };

            using IAppxFileReader reader = FileReaderFactory.CreateFileReader(filePath);
            try
            {
                yamlDefinition.Installers[0].SignatureSha256 = await this.CalculateSignatureHashAsync(new FileInfo(filePath), cancellationToken).ConfigureAwait(false);
            }
            catch (ArgumentException)
            {
            }
                
            var manifestReader = new AppxManifestReader();
            var details = await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);

            yamlDefinition.PackageName = details.DisplayName;
            yamlDefinition.Publisher = details.PublisherDisplayName;
            yamlDefinition.PackageVersion = details.Version;
            yamlDefinition.ShortDescription = details.Description;
            yamlDefinition.Installers[0].Capabilities = details.Capabilities?.Where(c => c.Type == CapabilityType.General || c.Type == CapabilityType.Device).Select(c => c.Name).ToList();
            yamlDefinition.Installers[0].RestrictedCapabilities = details.Capabilities?.Where(c => c.Type == CapabilityType.Restricted).Select(c => c.Name).ToList();

            if (details.Applications?.Any() == true)
            {
                // Exclude some unrelated PSF stuff - they are not the right choice for the app moniker.
                var candidateForAppMoniker = details.Applications.Select(a => a.Executable)
                    .FirstOrDefault(a => 
                        !string.IsNullOrEmpty(a) && !a.StartsWith("psf", StringComparison.OrdinalIgnoreCase) &&
                        !a.StartsWith("AI_stubs", StringComparison.OrdinalIgnoreCase) &&
                        a.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

                yamlDefinition.Platform = new List<YamlPlatform>() { details.Applications.Any(a => a.EntryPoint == "Windows.FullTrustApplication") ? YamlPlatform.WindowsDesktop : YamlPlatform.WindowsUniversal };
                
                if (!string.IsNullOrEmpty(candidateForAppMoniker))
                {
                    yamlDefinition.Moniker = candidateForAppMoniker.Substring(0, candidateForAppMoniker.Length - ".exe".Length).Split('\\', '/').Last();
                }
            }

            switch (details.ProcessorArchitecture)
            {
                case AppxPackageArchitecture.Arm:
                    yamlDefinition.Installers[0].Architecture = YamlArchitecture.Arm;
                    break;
                case AppxPackageArchitecture.Neutral:
                    yamlDefinition.Installers[0].Architecture = YamlArchitecture.Neutral;
                    break;
                case AppxPackageArchitecture.Arm64:
                    yamlDefinition.Installers[0].Architecture = YamlArchitecture.Arm64;
                    break;
                case AppxPackageArchitecture.x86:
                    yamlDefinition.Installers[0].Architecture = YamlArchitecture.X86;
                    break;
                case AppxPackageArchitecture.x64:
                    yamlDefinition.Installers[0].Architecture = YamlArchitecture.X64;
                    break;
            }

            return yamlDefinition;
        }

        public void FillGaps(YamlManifest manifest)
        {
            if (manifest == null)
            {
                return;
            }
            
            // propagate installer type from parent to all children without the type. This property should normally be left
            // unused, but this is to retain compatibility with some earlier YAML formats.
#pragma warning disable 618
            if (manifest.Installers != null && manifest.InstallerType != default)
            {
                foreach (var installer in manifest.Installers.Where(i => i.InstallerType == default))
                {
                    installer.InstallerType = manifest.InstallerType;
                }
            }
#pragma warning restore 618
            
            if (manifest.PackageIdentifier == null && !string.IsNullOrEmpty(manifest.Publisher) && !string.IsNullOrEmpty(manifest.PackageName))
            {
                manifest.PackageIdentifier = (manifest.Publisher + "." + manifest.PackageName).Replace(" ", string.Empty);
            }
#pragma warning restore 618
        }
    }
}
