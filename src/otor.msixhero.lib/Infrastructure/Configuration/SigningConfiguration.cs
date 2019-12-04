using System;
using System.IO;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract(Name = "signing")]
    public class SigningConfiguration
    {
        public SigningConfiguration()
        {
            this.DefaultOutFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify), "Certificates");
            this.TimeStampServer = "http://timestamp.globalsign.com/scripts/timstamp.dll";
        }

        [DataMember(Name = "defaultOutputFolder")]
        public ResolvableFolder.ResolvableFolder DefaultOutFolder { get; set; }

        [DataMember(Name="timeStampServer")]
        public string TimeStampServer { get; set; }
    }
}