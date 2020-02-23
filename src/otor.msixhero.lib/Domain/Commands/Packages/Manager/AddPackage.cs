namespace otor.msixhero.lib.Domain.Commands.Packages.Manager
{
    public class AddPackage : SelfElevatedCommand
    {
        public AddPackage()
        {
            this.KillRunningApps = true;
        }

        public AddPackage(string filePath)
        {
            this.KillRunningApps = true;
            this.FilePath = filePath;
        }

        public string FilePath { get; set; }

        public bool AllUsers { get; set; }

        public bool AllowDowngrade { get; set; }

        public bool KillRunningApps { get; set; }

        public override bool RequiresElevation => this.AllUsers;
    }
}
