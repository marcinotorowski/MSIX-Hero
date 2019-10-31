using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.BusinessLayer.Commands.Manager
{
    public class AddPackage : BaseCommand
    {
        public AddPackage()
        {
        }

        public AddPackage(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; set; }
    }
}
