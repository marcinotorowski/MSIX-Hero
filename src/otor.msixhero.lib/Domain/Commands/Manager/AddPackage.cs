namespace otor.msixhero.lib.Domain.Commands.Manager
{
    public class AddPackage : SelfElevatedCommand
    {
        public AddPackage()
        {
        }

        public AddPackage(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; set; }

        public bool AllUsers { get; set; }

        public bool AllowDowngrade { get; set; }

        public bool KillRunningApps { get; set; }

        public override bool RequiresElevation => this.AllUsers;
    }
}
