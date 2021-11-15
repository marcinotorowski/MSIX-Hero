using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Otor.MsixHero.Appx.Editor.Helpers
{
    public class AppxDocumentWriter
    {
        private readonly XDocument document;

        public AppxDocumentWriter(XDocument document)
        {
            this.document = document;
        }

        public async Task WriteAsync(string filePath)
        {
            var sb = new StringBuilder();
            await using TextWriter textWriter = new Utf8StringWriter(sb);
            await document.SaveAsync(textWriter, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);

            var fi = new FileInfo(filePath);
            if (fi.Directory?.Exists == false)
            {
                fi.Directory.Create();
            }

            await File.WriteAllTextAsync(filePath, sb.ToString()).ConfigureAwait(false);
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb)
            {
            }

            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
