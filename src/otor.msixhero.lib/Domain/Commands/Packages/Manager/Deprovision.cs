using System;

namespace otor.msixhero.lib.Domain.Commands.Packages.Manager
{
    [Serializable]
    public class Deprovision : SelfElevatedCommand
    {
        public Deprovision()
        {
        }

        public Deprovision(string packageFamilyName) : this()
        {
            PackageFamilyName = packageFamilyName;
        }

        public string PackageFamilyName { get; set;  }

        public override SelfElevationType RequiresElevation => SelfElevationType.RequireAdministrator;
    }
}