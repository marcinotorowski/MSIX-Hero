using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using otor.msixhero.lib.BusinessLayer.Managers.AppAttach;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    [Ignore("Integration")]
    public class AppAttachTests
    {
        [Test]
        public void Setup()
        {
            BundleHelper.SdkPath = @"E:\Visual\Playground\MSIX-Hero\artifacts\redistr\sdk";
            BundleHelper.MsixMgrPath = @"E:\Visual\Playground\MSIX-Hero\artifacts\redistr\msixmgr";
            BundleHelper.TemplatesPath = @"E:\Visual\Playground\MSIX-Hero\artifacts\templates";

            var appAttach = new AppAttachManager(new SelfElevationManagerFactory(new Client(new ProcessManager()), new DummyConfig()));
            appAttach.CreateVolume(
                @"E:\temp\MSIX.Commander_1.0.6.0-x64.msix", 
                @"C:\temp\my-disk.vhd",
                100,
                false,
                true).GetAwaiter().GetResult();
        }

        private class DummyConfig : IConfigurationService
        {
            public Task<Configuration> GetCurrentConfigurationAsync(bool preferCached = true, CancellationToken token = default)
            {
                return Task.FromResult(new Configuration());
            }

            public Configuration GetCurrentConfiguration(bool preferCached = true)
            {
                return new Configuration();
            }

            public Task SetCurrentConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public void SetCurrentConfiguration(Configuration configuration)
            {
            }
        }
    }
}
