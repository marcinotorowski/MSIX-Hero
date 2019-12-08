using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Modules.Dialogs.Pack.ViewModel;

namespace otor.msixhero.lib.tests.ViewModels
{
    [TestFixture]
    public class PackViewModelTests
    {
        [Test]
        public void Test()
        {
            var mockPacker = new Mock<IAppxPacker>();
            var mockInteraction = new Mock<IInteractionService>();
            var mockConfiguration = new Mock<IConfigurationService>();

            var config = new Configuration();
            mockConfiguration.Setup(dep => dep.GetCurrentConfiguration(false)).Returns(config);
            mockConfiguration.Setup(dep => dep.GetCurrentConfiguration(true)).Returns(config);

            var viewModel = new PackViewModel(mockPacker.Object, mockInteraction.Object, mockConfiguration.Object);

            Assert.IsFalse(viewModel.ChangeableContainer.IsValidated);
            Assert.IsTrue(viewModel.ChangeableContainer.IsValid);
            viewModel.Save().GetAwaiter().GetResult();
            Assert.IsFalse(viewModel.ChangeableContainer.IsValid);
            Assert.IsTrue(viewModel.ChangeableContainer.IsValidated);
        }
    }
}
