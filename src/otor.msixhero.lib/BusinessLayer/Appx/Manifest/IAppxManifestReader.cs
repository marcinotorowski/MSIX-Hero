using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest
{
    public interface IAppxManifestReader
    {
        AppxPackage Read(IAppxFileReader fileReader);
    }
}