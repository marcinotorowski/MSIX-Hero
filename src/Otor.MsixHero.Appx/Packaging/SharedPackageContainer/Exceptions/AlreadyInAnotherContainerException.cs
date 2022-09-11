using System;

namespace Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Exceptions
{
    public class AlreadyInAnotherContainerException : Exception
    {
        public AlreadyInAnotherContainerException(string containerName, string familyName) : base(string.Format("Package {0} is already in container {1}.", familyName, containerName))
        {
            this.FamilyName = familyName;
            this.ContainerName = containerName;
        }

        public string FamilyName { get; }

        public string ContainerName { get; }
    }
}
