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
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;

namespace Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Builder
{
    public class SharedPackageContainerBuilder
    {
        private readonly List<SharedPackage> _apps = new();

        public SharedPackageContainerBuilder(string name = null)
        {
            Name = name;
        }

        public string Name { get; set; }

        public SharedPackage AddFamilyName(string familyName)
        {
            _apps.Add(new SharedPackageFamilyName(familyName));
            return _apps.Last();
        }

        public async Task<SharedPackage> AddPackageFile(string packageFilePath)
        {
            var manifestReader = await AppxManifestSummaryReader.FromInstallLocation(packageFilePath).ConfigureAwait(false);
            Add(manifestReader.Name, manifestReader.Publisher);
            return _apps.Last();
        }

        public SharedPackage AddFullPackageName(string packageFullName)
        {
            _apps.Add(new SharedPackageFullName(packageFullName));
            return _apps.Last();
        }

        public async Task<SharedPackage> AddFromFilePath(string filePath, CancellationToken cancellationToken)
        {
            using var reader = FileReaderFactory.CreateFileReader(filePath);
            var manifestReader = new AppxManifestReader();
            var manifest = await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);
            AddFullPackageName(manifest.FullName);
            return _apps.Last();
        }

        public void Add(string name, string publisher)
        {
            AddFamilyName(AppxPackaging.GetPackageFamilyName(name, publisher));
        }

        public Entities.SharedPackageContainer Build()
        {
            var result = new Entities.SharedPackageContainer();

            if (_apps.Any())
            {
                result.PackageFamilies = new List<SharedPackageFamily>();

                var alreadyUsed = new HashSet<string>();
                foreach (var fn in _apps.Select(a => a.FamilyName))
                {
                    if (!alreadyUsed.Add(fn))
                    {
                        continue;
                    }

                    result.PackageFamilies.Add(new SharedPackageFamily { FamilyName = fn });
                }
            }
            else
            {
                result.PackageFamilies = new List<SharedPackageFamily>();
            }

            result.Name = Name;
            return result;
        }

        public string ToXml()
        {
            var resultObject = Build();
            var serializer = new XmlSerializer(typeof(Entities.SharedPackageContainer));

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
