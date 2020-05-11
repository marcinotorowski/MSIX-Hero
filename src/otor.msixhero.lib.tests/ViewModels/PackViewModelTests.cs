using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;
using otor.msixhero.ui.Domain;
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
            var mockSigning = new Mock<ISigningManager>();
            var mockManager = new Mock<ISelfElevationManagerFactory<ISigningManager>>();
            mockManager.Setup(dep => dep.Get(It.IsAny<SelfElevationLevel>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockSigning.Object));

            var config = new Configuration();

            mockConfiguration.Setup(dep => dep.GetCurrentConfiguration(false)).Returns(config);
            mockConfiguration.Setup(dep => dep.GetCurrentConfiguration(true)).Returns(config);
            mockSigning.Setup(dep => dep.GetCertificatesFromStore(It.IsAny<CertificateStoreType>(), It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(), It.IsAny<IProgress<ProgressData>>()))
                .ReturnsAsync(new List<PersonalCertificate>());

            var viewModel = new PackViewModel(mockPacker.Object, mockManager.Object, mockConfiguration.Object, mockInteraction.Object);

            Assert.AreEqual(ValidationMode.Silent, viewModel.ValidationMode);
            Assert.IsFalse(viewModel.HasError);
            Assert.IsFalse(viewModel.IsValid);

            viewModel.OkCommand.Execute(null);
            Assert.AreEqual(ValidationMode.Default, viewModel.ValidationMode);
            Assert.IsTrue(viewModel.HasError);

            Assert.IsTrue(string.IsNullOrEmpty(viewModel.OutputPath.CurrentValue));
            viewModel.InputPath.CurrentValue = @"c:\myFolder\temp123";
            Assert.AreEqual(@"c:\myFolder\temp123.msix", viewModel.OutputPath.CurrentValue);
        }
    }
}
