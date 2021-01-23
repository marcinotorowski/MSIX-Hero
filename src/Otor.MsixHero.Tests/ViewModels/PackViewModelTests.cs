// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.ViewModel;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Tests.ViewModels
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
            var mockManager = new Mock<ISelfElevationProxyProvider<ISigningManager>>();
            mockManager.Setup(dep => dep.GetProxyFor(It.IsAny<SelfElevationLevel>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockSigning.Object));

            var config = new Configuration();

            mockConfiguration.Setup(dep => dep.GetCurrentConfiguration(false)).Returns(config);
            mockConfiguration.Setup(dep => dep.GetCurrentConfiguration(true)).Returns(config);
            mockSigning.Setup(dep => dep.GetCertificatesFromStore(It.IsAny<CertificateStoreType>(), It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(), It.IsAny<IProgress<ProgressData>>()))
                .ReturnsAsync(new List<PersonalCertificate>());

            var viewModel = new PackViewModel(mockPacker.Object, mockManager.Object, mockConfiguration.Object, mockInteraction.Object);

            Assert.IsFalse(viewModel.HasError);
            Assert.IsFalse(viewModel.IsValid);

            viewModel.OkCommand.Execute(null);
            Assert.IsTrue(viewModel.HasError);

            Assert.IsTrue(string.IsNullOrEmpty(viewModel.OutputPath.CurrentValue));
            viewModel.InputPath.CurrentValue = @"c:\myFolder\temp123";
            Assert.AreEqual(@"c:\myFolder\temp123.msix", viewModel.OutputPath.CurrentValue);
        }
    }
}
