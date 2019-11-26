using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.BusinessLayer.Commands.Signing
{
    public class InstallCertificate : SelfElevatedCommand
    {
        public InstallCertificate(string filePath)
        {
            this.FilePath = filePath;
        }

        public InstallCertificate()
        {
        }

        public string FilePath { get; set; }

        public override bool RequiresElevation => true;
    }
}
