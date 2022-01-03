// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.IO;
using System.Net;
using NUnit.Framework;
using Otor.MsixHero.Winget.Helpers;
using Otor.MsixHero.Winget.Yaml;

namespace Otor.MsixHero.Tests.Winget
{
    [TestFixture]
    public class TestUtils
    {
        [Test]
        public void TestLocalHashing()
        {
            var testFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(testFile, "abc");
                var hash = new YamlUtils().CalculateHashAsync(new FileInfo(testFile)).Result;
                Assert.AreEqual("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad".ToUpperInvariant(), hash.ToUpperInvariant());
            }
            finally
            {
                File.Delete(testFile);
            }
        }
        
        [Test]
        public void TestNegativeCases()
        {
            var nonExistingFile = "J:\\test\\file.msix";

            var util = new YamlUtils();
            Assert.Throws<FileNotFoundException>(() =>
            {
                try
                {
                    util.CalculateHashAsync(new FileInfo(nonExistingFile)).Wait();
                }
                catch (AggregateException e)
                {
                    throw e.GetBaseException();
                }
            });

            nonExistingFile = "https://msixhero2.net/notexisting.fuk";
            Assert.Throws<WebException>(() =>
            {
                try
                {
                    util.CalculateHashAsync(new Uri(nonExistingFile)).Wait();
                }
                catch (AggregateException e)
                {
                    throw e.GetBaseException();
                }
            });
        }
    }
}
