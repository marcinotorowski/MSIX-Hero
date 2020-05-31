using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Winget;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace otor.msixhero.lib.BusinessLayer.Winget
{
    public class YamlWriter
    {
        public async Task WriteAsync(YamlDefinition definition, Stream stream, CancellationToken cancellationToken = default)
        {
            using (var textWriter = new StreamWriter(stream, leaveOpen: true))
            {
                await this.WriteAsync(definition, textWriter, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<string> WriteAsync(YamlDefinition definition, CancellationToken cancellationToken = default)
        {
            var stringBuilder = new StringBuilder();

            using (var textWriter = new StringWriter(stringBuilder))
            {
                await this.WriteAsync(definition, textWriter, cancellationToken).ConfigureAwait(false);
            }

            return stringBuilder.ToString();
        }

        public async Task WriteAsync(YamlDefinition definition, TextWriter textWriter, CancellationToken cancellationToken = default)
        {
            var serializerBuilder = new SerializerBuilder().WithEmissionPhaseObjectGraphVisitor(args => new DefaultExclusiveObjectGraphVisitor(args.InnerVisitor));
            var serializer = serializerBuilder.Build();
            
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                serializer.Serialize(stringWriter, definition);
            }
            
            cancellationToken.ThrowIfCancellationRequested();

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Edited with MSIX Hero");

            cancellationToken.ThrowIfCancellationRequested();
            var serialized = stringBuilder.ToString();
            serialized = Regex.Replace(serialized, @"[\r\n]{2,}", Environment.NewLine);
            await textWriter.WriteAsync(serialized).ConfigureAwait(false);
        }

        public void Write(YamlDefinition definition, Stream stream)
        {
            using (var textWriter = new StreamWriter(stream, leaveOpen: true))
            {
                this.Write(definition, textWriter);
            }
        }

        public void Write(YamlDefinition definition, TextWriter textWriter)
        {
            textWriter.Write(this.Write(definition));
        }

        public string Write(YamlDefinition definition)
        {
            var serializer = new Serializer();
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                serializer.Serialize(stringWriter, definition);
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Edited with MSIX Hero");

            var serialized = stringBuilder.ToString();
            serialized = Regex.Replace(serialized, @"[\r\n]{2,}", Environment.NewLine);

            return serialized;
        }
    }
}