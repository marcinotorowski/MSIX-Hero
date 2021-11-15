namespace Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest
{
    public class AddCapability : IAppxEditCommand
    {
        public AddCapability()
        {
        }

        public AddCapability(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}
