using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.Managers
{
    public class AppxPackageManagerFactory : IAppxPackageManagerFactory
    {
        private readonly IProcessManager processManager;
        private readonly IAppxSigningManager signingManager;
        private IAppxPackageManager local, remote;

        public AppxPackageManagerFactory(IProcessManager processManager, IAppxSigningManager signingManager)
        {
            this.processManager = processManager;
            this.signingManager = signingManager;
        }

        public IAppxPackageManager GetLocal()
        {
            return this.local ??= new CurrentUserAppxPackageManager(this.signingManager);
        }

        public IAppxPackageManager GetRemote()
        {
            return this.remote ??= new RemoteAppxPackageManager(this.processManager);
        }
    }
}