using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Interop;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Appx.Packaging.SharedAppContainer
{
    public class SharedAppContainerBuilder
    {
        private readonly List<SharedAppDefinition> _apps = new();

        public SharedAppContainerBuilder(string name = null)
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public SharedAppDefinition AddFamilyName(string familyName)
        {
            this._apps.Add(new SharedAppFamilyName(familyName));
            return this._apps.Last();
        }

        public async Task<SharedAppDefinition> AddPackageFile(string packageFilePath)
        {
            var manifestReader = await AppxManifestSummaryReader.FromInstallLocation(packageFilePath).ConfigureAwait(false);
            this.Add(manifestReader.Name, manifestReader.Publisher);
            return this._apps.Last();
        }

        public SharedAppDefinition AddFullPackageName(string packageFullName)
        {
            this._apps.Add(new SharedAppFullName(packageFullName));
            return this._apps.Last();
        }

        public async Task<SharedAppDefinition> AddFromFilePath(string filePath, CancellationToken cancellationToken)
        {
            using var reader = FileReaderFactory.CreateFileReader(filePath);
            var manifestReader = new AppxManifestReader();
            var manifest = await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);
            this.AddFullPackageName(manifest.FullName);
            return this._apps.Last();
        }

        public void Add(string name, string publisher)
        {
            this.AddFamilyName(AppxPackaging.GetPackageFamilyName(name, publisher));
        }

        public SharedAppContainerDefinition Build()
        {
            var result = new SharedAppContainerDefinition();

            if (this._apps.Any())
            {
                result.PackageFamilies = new List<PackageFamilyDefinition>();

                var alreadyUsed = new HashSet<string>();
                foreach (var fn in this._apps.Select(a => a.FamilyName))
                {
                    if (!alreadyUsed.Add(fn))
                    {
                        continue;
                    }

                    result.PackageFamilies.Add(new PackageFamilyDefinition { FamilyName = fn });
                }
            }

            result.Name = this.Name;
            return result;
        }

        public string ToXml()
        {
            var resultObject = this.Build();
            var serializer = new XmlSerializer(typeof(SharedAppContainerDefinition));

            var xmlBody = new StringBuilder();
            using TextWriter textWriter = new Utf8StringWriter(xmlBody);

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = false,
            };

            using var xmlBodyWriter = XmlWriter.Create(textWriter, settings);
            
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            serializer.Serialize(xmlBodyWriter, resultObject, ns);

            return xmlBody.ToString();
        }
        
        private class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb)
            {
            }

            public override Encoding Encoding => new UTF8Encoding(false);
        }
    }
}
