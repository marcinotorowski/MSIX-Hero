using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Appx
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