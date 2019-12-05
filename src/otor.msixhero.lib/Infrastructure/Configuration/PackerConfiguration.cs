using System;
using System.IO;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract(Name = "packer")]
    public class PackerConfiguration
    {public PackerConfiguration()
        {
            this.DefaultOutFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify), "Packages");
        }

        [DataMember(Name = "defaultOutputFolder")]
        public ResolvableFolder.ResolvableFolder DefaultOutFolder { get; set; }
    }
}