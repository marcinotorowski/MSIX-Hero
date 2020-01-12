using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class ManifestReaderTests
    {
        [Test]
        public void Test()
        {
            var manifest = @"C:\Program Files\WindowsApps\11560RaynetGmbH.RayEval_6.2.3088.0_neutral__t21q0c7n9nf3e\AppxManifest.xml";

            IAppxManifestReader reader = new AppxManifestReader();
            using (var adapter = new FileInfoFileReaderAdapter(manifest))
            {
                var pkg = reader.Read(adapter);
            }

            manifest = @"C:\temp\RayEval-6.2.3067.2071.msix";
            using (var adapter = new ZipArchiveFileReaderAdapter(manifest))
            {
                var pkg = reader.Read(adapter);
            }
        }
    }
}
