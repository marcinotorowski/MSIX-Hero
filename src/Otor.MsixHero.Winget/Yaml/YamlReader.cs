using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Winget.Yaml.Entities;
using YamlDotNet.Serialization;

namespace Otor.MsixHero.Winget.Yaml
{
    /// <summary>
    /// A class used to read WinGet definition from a YAML file.
    /// </summary>
    public class YamlReader
    {
        /// <summary>
        /// Reads WinGet definition from a stream and returns a task representing the asynchronous operation.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The asynchronous task that represents the operation.</returns>
        public async Task<YamlDefinition> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using var textReader = new StreamReader(stream, leaveOpen: true);
            return await this.ReadAsync(textReader, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads WinGet definition from a string and returns a text reader representing the asynchronous operation.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The asynchronous task that represents the operation.</returns>
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

        /// <summary>
        /// Reads WinGet definition from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <returns>The WinGet definition.</returns>
        public YamlDefinition Read(Stream stream)
        {
            using (var textReader = new StreamReader(stream, leaveOpen: true))
            {
                return this.Read(textReader);
            }
        }


        /// <summary>
        /// Reads WinGet definition from a text reader.
        /// </summary>
        /// <param name="textReader">The input textReader.</param>
        /// <returns>The WinGet definition.</returns>
        public YamlDefinition Read(TextReader textReader)
        {
            var deserializerBuilder = new DeserializerBuilder().IgnoreUnmatchedProperties();
            var deserializer = deserializerBuilder.Build();
            return deserializer.Deserialize<YamlDefinition>(textReader);
        }
    }
}
