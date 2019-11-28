namespace otor.msixhero.lib.Domain.Commands.Manager
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
