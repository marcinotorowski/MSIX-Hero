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
using System.Linq;
using NUnit.Framework;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Files.Helpers;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Registry.Parser;

namespace Otor.MsixHero.Tests.Registry
{
    [TestFixture]
    public class TestConversion
    {
        [Test]
        public void Convert()
        {
            var tempReg = System.Guid.NewGuid().ToString("N") + ".reg";
            var tempDir = "reg-out-" + System.Guid.NewGuid().ToString("N");

            Directory.CreateDirectory(tempDir);
            try
            {
                var mfn = typeof(TestConversion).Assembly.GetManifestResourceNames().First(a => a.EndsWith("team.reg"));
                using (var mf = typeof(TestConversion).Assembly.GetManifestResourceStream(mfn))
                {
                    using (var fs = File.OpenWrite(tempReg))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        mf.CopyTo(fs);
                        fs.Flush(true);
                    }
                }

                var regConv = new MsixRegistryFileWriter(tempDir);
                var regPars = new RegFileParser();

                regPars.Parse(Path.Combine(tempReg));
                regConv.ImportRegFile(tempReg);
            }
            finally
            {
                ExceptionGuard.Guard(() =>
                {
                    Directory.Delete(tempDir, true);
                    File.Delete(tempReg);
                });
            }
        }
    }
}
