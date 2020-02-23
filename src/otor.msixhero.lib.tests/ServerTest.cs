using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using NUnit.Framework.Internal;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.ui.Services;
using Prism.Events;
using ICommandExecutor = otor.msixhero.lib.Infrastructure.Commanding.ICommandExecutor;

namespace otor.msixhero.lib.tests
{
    [TestFixture()]
    public class ServerTest
    {
        [Test]
        public void TestCommunication()
        {
            var result = new List<InstalledPackage> {new InstalledPackage() { Name = "ABC" }};

            var server = new Server(
                new ApplicationStateManager(
                    new EventAggregator(),
                    new DummyCommandExecutor(result), 
                    new LocalConfigurationService()));

            var client = new Client(new DummyProcessManager());
            var t2 = client.GetExecuted(new GetPackages(), CancellationToken.None);
            var t1 = server.Start(1);

            Task.WaitAll(t1, t2);
            Assert.AreEqual(1, t2.Result.Count);
            Assert.AreEqual("ABC", t2.Result[0].Name);
        }

        private class DummyCommandExecutor : ICommandExecutor
        {
            private readonly object result;

            public DummyCommandExecutor(object result)
            {
                this.result = result;
            }

            public void SetStateManager(IWritableApplicationStateManager stateManager)
            {
            }

            public void Execute(BaseCommand action)
            {
            }

            public T GetExecute<T>(BaseCommand<T> action)
            {
                return default;
            }

            public Task ExecuteAsync(BaseCommand action, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(true);
            }

            public Task<T> GetExecuteAsync<T>(BaseCommand<T> action, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(default(T));
            }

            public Task<object> GetExecuteAsync(BaseCommand command, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(result);
            }
        }


        private class DummyProcessManager : IProcessManager
        {
            public void Dispose()
            {   
            }

            public async Task Connect(CancellationToken cancellationToken = default)
            {
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
