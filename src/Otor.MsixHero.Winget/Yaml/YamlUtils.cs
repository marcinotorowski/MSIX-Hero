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
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest;
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
            var tempFileName = Path.Combine(Path.GetTempPath(), "msixhero-" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".msix");

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
            if (
                string.Equals(".appx", ext, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".msix", ext, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".appxbundle", ext, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".msixbundle", ext, StringComparison.OrdinalIgnoreCase))
            {
                using (IAppxFileReader src = new ZipArchiveFileReaderAdapter(fileInfo.FullName))
                {
                    if (src.FileExists("AppxSignature.p7x"))
                    {
                        using (var appxSignature = src.GetFile("AppxSignature.p7x"))
                        {
                            var buffer = new byte[ushort.MaxValue];
                            var read = await appxSignature.ReadAsync(buffer, 0, ushort.MaxValue, cancellationToken).ConfigureAwait(false);
                            
                            var builder = new StringBuilder();
                            
                            using (var sha = SHA256.Create())
                            {
                                foreach (var b in sha.ComputeHash(buffer, 0, read))
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    builder.Append(b.ToString("X2"));
                                }

                                return builder.ToString();
                            }
                        }
                    }

                    throw new ArgumentException($"The file '{fileInfo.Name}' does not contain a signature.", nameof(fileInfo));
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

        public async Task<YamlDefinition> CreateFromFile(string filePath, CancellationToken cancellationToken = default)
        {
            var detector = new InstallerTypeDetector();
            var detected = await detector.DetectSetupType(filePath, cancellationToken).ConfigureAwait(false);

            YamlDefinition yaml;

            switch (detected)
            {
                case YamlInstallerType.msi:
                    yaml = await Task.Run(() => this.CreateFromMsi(filePath), cancellationToken).ConfigureAwait(false);
                    break;
                case YamlInstallerType.none:
                case YamlInstallerType.exe:
                case YamlInstallerType.inno:
                case YamlInstallerType.nullsoft:
                case YamlInstallerType.wix:
                    yaml = await Task.Run(() => this.CreateFromExe(filePath), cancellationToken).ConfigureAwait(false);
                    yaml.Installers[0].InstallerType = detected;
                    break;
                case YamlInstallerType.msix:
                case YamlInstallerType.appx:
                    yaml = await this.CreateFromMsix(filePath, cancellationToken).ConfigureAwait(false);
                    yaml.Installers[0].InstallerType = detected;
                    yaml.MinOperatingSystemVersion = Version.Parse("10.0.0");
                    break;
                default:
                    yaml = new YamlDefinition
                    {
                        Installers = new List<YamlInstaller>
                        {
                            new YamlInstaller()
                        }
                        
                    };
                    break;
            }

            yaml.Id = (yaml.Publisher + "." + yaml.Name).Replace(" ", string.Empty);
            yaml.Installers[0].Sha256 = await this.CalculateHashAsync(new FileInfo(filePath), cancellationToken).ConfigureAwait(false);
            
            if (yaml.License != null && yaml.License.IndexOf("Copy", StringComparison.OrdinalIgnoreCase) == -1 && yaml.License.IndexOf("(C)", StringComparison.OrdinalIgnoreCase) == -1 && yaml.License.IndexOf("http", StringComparison.OrdinalIgnoreCase) == -1)
            {
                yaml.License = "Copyright (C) " + yaml.License;
            }

            return yaml;
        }

        private YamlDefinition CreateFromMsi(string filePath)
        {
            var msiProps = new Msi().GetProperties(filePath);

            var yamlDefinition = new YamlDefinition
            {
                Name = msiProps.TryGetValue("ProductName", out var pn) ? pn : null,
                Version = msiProps.TryGetValue("ProductVersion", out var pv) ? pv : null,
                Publisher = msiProps.TryGetValue("Manufacturer", out var pm) ? pm : null,
                Description = msiProps.TryGetValue("ARPCOMMENTS", out var arpc) ? arpc : null,
                License = msiProps.TryGetValue("ARPCONTACT", out var arpcont) ? arpcont : null,
                LicenseUrl = msiProps.TryGetValue("ARPURLINFOABOUT", out var arpurl) || msiProps.TryGetValue("ARPHELPLINK", out arpurl) ? arpurl : null,
                // Language = msiProps.TryGetValue("ProductLanguage", out var pl) ? pl : null,
                Installers = new List<YamlInstaller>
                {
                    new YamlInstaller
                    {
                        InstallerType = YamlInstallerType.msi,
                        Scope = msiProps.TryGetValue("ALLUSERS", out var allusers) && allusers == "1" ? YamlScope.machine : YamlScope.none,
                        Arch = msiProps.TryGetValue("Template", out var template) && (template.IndexOf("Intel64", StringComparison.OrdinalIgnoreCase) != -1 || template.IndexOf("x64", StringComparison.OrdinalIgnoreCase) != -1 || template.IndexOf("amd64", StringComparison.OrdinalIgnoreCase) != -1) ? YamlArchitecture.x64 : YamlArchitecture.x86,
                    }
                }
            };

            if (yamlDefinition.License != null)
            {
                yamlDefinition.License = yamlDefinition.License.Trim();
                if (string.IsNullOrWhiteSpace(yamlDefinition.License))
                {
                    yamlDefinition.License = null;
                }

                yamlDefinition.License = yamlDefinition.License.Trim();
                if (string.IsNullOrWhiteSpace(yamlDefinition.LicenseUrl))
                {
                    yamlDefinition.LicenseUrl = null;
                }
            }
            
            return yamlDefinition;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private YamlDefinition CreateFromExe(string filePath)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
            var yamlDefinition = new YamlDefinition
            {
                Name = fileVersionInfo.ProductName?.Trim(),
                Version = fileVersionInfo.ProductVersion?.Trim(),
                Publisher = fileVersionInfo.CompanyName?.Trim(),
                Description = fileVersionInfo.FileDescription?.Trim(),
                License = fileVersionInfo.LegalCopyright ?? fileVersionInfo.LegalTrademarks,
                Installers = new List<YamlInstaller>
                {
                    new YamlInstaller
                    {
                        InstallerType = YamlInstallerType.exe
                    }
                }
            };

            if (yamlDefinition.License != null)
            {
                yamlDefinition.License = yamlDefinition.License.Trim();
                if (string.IsNullOrWhiteSpace(yamlDefinition.License))
                {
                    yamlDefinition.License = null;
                }
            }
            
            using (var fs = File.OpenRead(filePath))
            {
                using (var binaryReader = new BinaryReader(fs))
                {
                    var mz = binaryReader.ReadUInt16();
                    if (mz == 0x5a4d) // check if it's a valid image ("MZ")
                    {
                        fs.Position = 60; // this location contains the offset for the PE header
                        var offset = binaryReader.ReadUInt32();

                        fs.Position = offset + sizeof(uint); // contains the architecture
                        var machine = binaryReader.ReadUInt16();

                        if (machine == 0x8664) // IMAGE_FILE_MACHINE_AMD64
                        {
                            yamlDefinition.Installers[0].Arch = YamlArchitecture.x64;
                        }
                        else if (machine == 0x014c) // IMAGE_FILE_MACHINE_I386
                        {
                            yamlDefinition.Installers[0].Arch = YamlArchitecture.x86;
                        }
                        else if (machine == 0x0200) // IMAGE_FILE_MACHINE_IA64
                        {
                            yamlDefinition.Installers[0].Arch = YamlArchitecture.x64;
                        }
                    }
                }
            }

            return yamlDefinition;
        }

        private async Task<YamlDefinition> CreateFromMsix(string filePath, CancellationToken cancellationToken = default)
        {
            var yamlDefinition = new YamlDefinition()
            {
                Installers = new List<YamlInstaller>
                {
                    new YamlInstaller
                    {
                        Scope = YamlScope.user,
                        InstallerType = YamlInstallerType.msix
                    }
                }
            };

            IAppxFileReader reader;

            if (filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                reader = new FileInfoFileReaderAdapter(filePath);
            }
            else
            {
                reader = new ZipArchiveFileReaderAdapter(filePath);

                try
                {
                    yamlDefinition.Installers[0].SignatureSha256 = await this.CalculateSignatureHashAsync(new FileInfo(filePath), cancellationToken).ConfigureAwait(false);
                }
                catch (ArgumentException)
                {
                }
            }

            using (reader)
            {
                var manifestReader = new AppxManifestReader();
                var details = await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);

                yamlDefinition.Name = details.DisplayName;
                yamlDefinition.Publisher = details.PublisherDisplayName;
                yamlDefinition.Version = details.Version;
                yamlDefinition.Description = details.Description;

                if (details.Applications?.Any() == true)
                {
                    // Exclude some unrelated PSF stuff - they are not the right choice for the app moniker.
                    var candidateForAppMoniker = details.Applications.Select(a => a.Executable)
                        .FirstOrDefault(a => 
                            !string.IsNullOrEmpty(a) && !a.StartsWith("psf", StringComparison.OrdinalIgnoreCase) &&
                            !a.StartsWith("AI_stubs", StringComparison.OrdinalIgnoreCase) &&
                            a.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(candidateForAppMoniker))
                    {
                        yamlDefinition.AppMoniker = candidateForAppMoniker.Substring(0, candidateForAppMoniker.Length - ".exe".Length).Split('\\', '/').Last();
                    }
                }

                switch (details.ProcessorArchitecture)
                {
                    case AppxPackageArchitecture.Arm:
                        yamlDefinition.Installers[0].Arch = YamlArchitecture.arm;
                        break;
                    case AppxPackageArchitecture.Neutral:
                        yamlDefinition.Installers[0].Arch = YamlArchitecture.Neutral;
                        break;
                    case AppxPackageArchitecture.Arm64:
                        yamlDefinition.Installers[0].Arch = YamlArchitecture.arm64;
                        break;
                    case AppxPackageArchitecture.x86:
                        yamlDefinition.Installers[0].Arch = YamlArchitecture.x86;
                        break;
                    case AppxPackageArchitecture.x64:
                        yamlDefinition.Installers[0].Arch = YamlArchitecture.x64;
                        break;
                }
            }

            return yamlDefinition;
        }

        public void FillGaps(YamlDefinition definition)
        {
            if (definition == null)
            {
                return;
            }
            
            // propagate installer type from parent to all children without the type
            if (definition.Installers != null && definition.InstallerType != null)
            {
                foreach (var installer in definition.Installers.Where(i => i.InstallerType == null))
                {
                    installer.InstallerType = definition.InstallerType;
                }
            }

            if (definition.Id == null && !string.IsNullOrEmpty(definition.Publisher) && !string.IsNullOrEmpty(definition.Name))
            {
                definition.Id = (definition.Publisher + "." + definition.Name).Replace(" ", string.Empty);
            }
#pragma warning restore 618
        }
    }
}
