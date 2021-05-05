using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Registry;
using Registry.Abstractions;

namespace Otor.MsixHero.Appx.Packaging.Registry
{
    public class AppxRegistryReader : IAppxRegistryReader
    {
        private readonly Lazy<Stream> lazyStream;
        private readonly bool ownsStream;

        public AppxRegistryReader(Stream stream, bool ownsStream = true)
        {
            this.lazyStream = new Lazy<Stream>(() => GetSupportedStream(stream));
            this.ownsStream = ownsStream;
        }

        public AppxRegistryReader(FileStream stream, bool ownsStream = true)
        {
            this.lazyStream = new Lazy<Stream>(() => stream);
            this.ownsStream = ownsStream;
        }

        public AppxRegistryReader(MemoryStream stream, bool ownsStream = true)
        {
            this.lazyStream = new Lazy<Stream>(() => stream);
            this.ownsStream = ownsStream;
        }

        public AppxRegistryReader(string regDatFilePath)
        {
            this.lazyStream = new Lazy<Stream>(() => File.OpenRead(regDatFilePath));
            this.ownsStream = true;
        }

        public async IAsyncEnumerable<AppxRegistryKey> EnumerateKeys(string registryKeyPath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (registryKeyPath.EndsWith('\\'))
            {
                registryKeyPath = registryKeyPath.TrimEnd('\\');
            }
            
            var key = await this.GetRegistry(registryKeyPath, cancellationToken).ConfigureAwait(false);
            if (key == null)
            {
                yield break;
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            
            foreach (var subKey in key.SubKeys.Select(sk => sk.KeyPath))
            {
                var subKeyInstance = await this.GetRegistry(subKey, cancellationToken).ConfigureAwait(false);
                if (subKeyInstance == null)
                {
                    continue;
                }
                
                yield return new AppxRegistryKey
                {
                    HasSubKeys = subKeyInstance.SubKeys.Any(),
                    HasSubValues = subKeyInstance.Values.Any(),
                    Path = subKeyInstance.KeyPath
                };
            }
        }

        public async IAsyncEnumerable<AppxRegistryValue> EnumerateValues(string registryKeyPath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (registryKeyPath.EndsWith('\\'))
            {
                registryKeyPath = registryKeyPath.TrimEnd('\\');
            }

            var key = await this.GetRegistry(registryKeyPath, cancellationToken).ConfigureAwait(false);
            if (key == null)
            {
                yield break;
            }
            
            foreach (var value in key.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return new AppxRegistryValue
                {
                    Name = value.ValueName,
                    Type = value.ValueType,
                    Data = value.ValueData,
                    Path = registryKeyPath
                };

            }
        }

        public void Dispose()
        {
            if (this.ownsStream && this.lazyStream.IsValueCreated)
            {
                this.lazyStream.Value.Dispose();
            }
        }
        
        private async Task<RegistryKey> GetRegistry(string registryKeyPath, CancellationToken cancellationToken)
        {
            var stream = await Task.Run(() => this.lazyStream.Value, cancellationToken).ConfigureAwait(false);

            RegistryHiveOnDemand reg;

            if (stream is FileStream fileStream)
            {
                reg = new RegistryHiveOnDemand(fileStream.Name);
            }
            else if (stream is MemoryStream memoryStream)
            {
                reg = new RegistryHiveOnDemand(memoryStream.ToArray(), "Registry.dat");
            }
            else
            {
                throw new InvalidOperationException("At this point of time, the stream must be already a FileStream or a MemoryStream.");
            }
            
            return reg.GetKey(registryKeyPath);
        }

        private static Stream GetSupportedStream(Stream stream)
        {
            if (stream is FileStream || stream is MemoryStream)
            {
                return stream;
            }

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
