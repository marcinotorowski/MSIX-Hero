using NUnit.Framework;
using Otor.MsixHero.Dependencies;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Proxy;

namespace Otor.MsixHero.Tests.Dependencies
{
    [TestFixture]
    public class TestDependencies
    {
        [Test]
        public void Test()
        {
            var fie = @"C:\Program Files\WindowsApps\Microsoft.Photos.MediaEngineDLC_1.0.0.0_x64__8wekyb3d8bbwe\AppxManifest.xml";
            var dm = new DependencyMapper(new SelfElevationManagerFactory(new Client(new InterProcessCommunicationManager()), new LocalConfigurationService()));

            var mapping = dm.GetGraph(fie).GetAwaiter().GetResult();
        }
    }
}
