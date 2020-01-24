﻿namespace otor.msixhero.lib.Domain.Commands.AppAttach
{
    public class ConvertToVhd : SelfElevatedCommand
    {
        public ConvertToVhd(string packagePath, string vhdPath)
        {
            PackagePath = packagePath;
            VhdPath = vhdPath;
        }

        public ConvertToVhd()
        {
        }

        public string PackagePath { get; set; }

        public string VhdPath { get; set; }

        public uint SizeInMegaBytes { get; set; }

        public bool GenerateScripts { get; set; }

        public override bool RequiresElevation => true;
    }
}
