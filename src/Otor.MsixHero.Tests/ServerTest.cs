using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.BusinessLayer.State;

namespace Otor.MsixHero.Tests
{
    [TestFixture()]
    public class ServerTest
    {
        //[Test]
        //public void TestCommunication()
        //{
        //    var result = new List<InstalledPackage> {new InstalledPackage() { Name = "ABC" }};

        //    var server = new Server(
        //        new ApplicationStateManager(
        //            new EventAggregator(),
        //            new DummyCommandExecutor(result), 
        //            new LocalConfigurationService()));

        //    var client = new Client(new DummyProcessManager());
        //    var t2 = client.GetExecuted(new GetPackages(), CancellationToken.None);
        //    var t1 = server.Start(1);

        //    Task.WaitAll(t1, t2);
        //    Assert.AreEqual(1, t2.Result.Count);
        //    Assert.AreEqual("ABC", t2.Result[0].Name);
        //}
    }
}
