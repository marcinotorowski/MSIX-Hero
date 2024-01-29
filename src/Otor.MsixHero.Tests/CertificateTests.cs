// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System.IO;
using System.Threading;
using NUnit.Framework;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Appx.Signing.TimeStamping;

namespace Otor.MsixHero.Tests
{
    [TestFixture]
    public class CertificateTests
    {
        [Test]
        public void TestReader()
        {
            ISigningManager signManager = new SigningManager(MsixHeroGistTimeStampFeed.CreateCached());
            var certs = signManager.GetCertificatesFromStore(CertificateStoreType.MachineUser).GetAwaiter().GetResult();
        }

        [Test]
        public void TestCertValidator()
        {
            var fileSignedButUntrusted = new FileInfo(Path.Combine("Resources", "ConEmuPack-O2004-M1220.603-P380-F_19.1.8.0_x64__xwfzvwzp69w2e.msix"));
            var fileUnsigned = new FileInfo(Path.Combine("Resources", "nsis.exe"));
            var fileSignedAndTrusted = new FileInfo(Path.Combine("Resources", "inno.exe"));
            var fileNotSignable = new FileInfo(Path.Combine("Resources", "wintrust.dll.ini"));

            var manager = new SigningManager(MsixHeroGistTimeStampFeed.CreateCached());

            var check1 = manager.IsTrusted(fileSignedButUntrusted.FullName, CancellationToken.None).GetAwaiter().GetResult();
            var check2 = manager.IsTrusted(fileSignedAndTrusted.FullName, CancellationToken.None).GetAwaiter().GetResult();
            var check3 = manager.IsTrusted(fileUnsigned.FullName, CancellationToken.None).GetAwaiter().GetResult();
            var check4 = manager.IsTrusted(fileNotSignable.FullName, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(check1.IsTrusted, Is.False);
            Assert.That(check2.IsTrusted, Is.True);
            Assert.That(check3.IsTrusted, Is.False);
            Assert.That(check4.IsTrusted, Is.False);
        }
    }
}
