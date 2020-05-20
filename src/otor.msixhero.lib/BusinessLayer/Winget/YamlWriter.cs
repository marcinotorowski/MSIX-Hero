using System;
using System.IO;
using System.Text;
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

        public Task WriteAsync(YamlDefinition definition, TextWriter textWriter, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                var serializerBuilder = new SerializerBuilder().WithEmissionPhaseObjectGraphVisitor(args => new DefaultExclusiveObjectGraphVisitor(args.InnerVisitor));
                var serializer = serializerBuilder.Build();
                serializer.Serialize(textWriter, definition);
                textWriter.WriteLine();
                textWriter.Write("# Edited with MSIX Hero");
            },
            cancellationToken);
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
            var serializer = new Serializer();
            serializer.Serialize(textWriter, definition);
            textWriter.WriteLine();
            textWriter.Write("# Edited with MSIX Hero");
        }

        public string Write(YamlDefinition definition)
        {
            var stringBuilder = new StringBuilder();
            var serializerBuilder = new SerializerBuilder().WithEmissionPhaseObjectGraphVisitor(args => new DefaultExclusiveObjectGraphVisitor(args.InnerVisitor));
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                var serializer = serializerBuilder.Build();
                serializer.Serialize(stringWriter, definition);
                stringWriter.WriteLine();
                stringWriter.Write("# Edited with MSIX Hero");
            }

            return stringBuilder.ToString();
        }
    }
}