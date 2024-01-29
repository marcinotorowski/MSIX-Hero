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

using System;
using System.Linq;
using NUnit.Framework;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Tests.Sdk
{
    [TestFixture]
    public class SignToolSdkTests
    {
        [Test]
        public void TestMessage()
        {
            var output = ("The following certificates were considered:\r\n    " +
                         "Issued to: localhost\r\n    Issued by: localhost\r\n    " +
                         "Expires:   Wed May 05 23:39:42 2021\r\n    " +
                         "SHA1 hash: E323874CD3FB890842AB09791CD605F8D3A5340F\r\n    " +
                         "Issued to: ddfe70e5-4d2b-4940-a550-66f4f7d8307c\r\n    " +
                         "Issued by: MS-Organization-Access\r\n    " +
                         "Expires:   Sun May 05 23:57:34 2030\r\n    " +
                         "SHA1 hash: D1AABEE4733FE30E1D6317427E88CA973406025B\r\n    " +
                         "Issued to: Marcin Otorowski\r\n    " +
                         "Issued by: Certum Code Signing CA SHA2\r\n    " +
                         "Expires:   Tue Dec 29 13:55:03 2020\r\n    " +
                         "SHA1 hash: C362045164EFCDA4E40473F0B3B6B1D3E647CA6F\r\n    " +
                         "Issued to: bc64c50169ddad94\r\n    " +
                         "Issued by: Token Signing Public Key\r\n    " +
                         "Expires:   Wed May 13 01:07:14 2020\r\n    " +
                         "SHA1 hash: A33983F854D4C7C6FAC5CC754656DC7CCA5ACEEF\r\n" +
                         "After EKU filter, 1 certs were left.\r\n" +
                         "After expiry filter, 1 certs were left.\r\n" +
                         "After Hash filter, 0 certs were left.\r\n" +
                         "After Private Key filter, 0 certs were left.	\r\n")
                .Split("\r\n").ToList();

            SignToolWrapper.TryGetErrorMessageFromSignToolOutput(output, out var error);
            Assert.That(error, Contains.Substring("EKU"));
        }
    }
}
