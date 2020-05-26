using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Winget;
using otor.msixhero.lib.Infrastructure.Hashing;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Winget
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

        public void FillGaps(YamlDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

#pragma warning disable 618
            // determine installer type from any child
            /* This is according to the docu wrong!
            if (!definition.InstallerType.HasValue)
            {
                if (definition.Installers.Any())
                {
                    var deducedType = definition.Installers.Select(t => t.InstallerType).Where(d => d != null).Distinct().Take(2).ToArray();
                    if (deducedType.Length == 1)
                    {
                        definition.InstallerType = deducedType[0];
                    }
                }

                if (!definition.InstallerType.HasValue)
                {
                    var extensions = definition.Installers.Select(s => Path.GetExtension(s.Url)).Where(d => d != null).Distinct().Take(2).ToArray();
                    if (extensions.Length == 1 && extensions[0] != null)
                    {
                        switch (extensions[0].ToLower())
                        {
                            case ".msi":
                                definition.InstallerType = YamlInstallerType.msi;
                                break;
                            case ".exe":
                                definition.InstallerType = YamlInstallerType.exe;
                                break;
                            case ".msix":
                                definition.InstallerType = YamlInstallerType.msix;
                                break;
                        }
                    }
                }
            }*/

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
