using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;
using otor.msixhero.lib.BusinessLayer.Models.Manifest;
using otor.msixhero.lib.BusinessLayer.Models.Packages;

namespace otor.msixhero.lib.BusinessLayer.Commands.Grid
{
    [Serializable]
    public class GetManifestedDetails : BaseCommand<AppxManifestSummary>
    {
        public GetManifestedDetails()
        {
        }

        public GetManifestedDetails(string appxManifestFilePath)
        {
            AppxManifestFilePath = appxManifestFilePath;
        }

        public GetManifestedDetails(Package package) : this(package.ManifestLocation)
        {
        }

        [XmlElement]
        public string AppxManifestFilePath { get; set; }
    }
}
