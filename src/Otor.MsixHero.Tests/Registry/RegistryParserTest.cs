using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Otor.MsixHero.Registry;
using Otor.MsixHero.Registry.Parser;

namespace Otor.MsixHero.Tests.Registry
{
    [TestFixture]
    public class RegistryParserTest
    {
        [Test]
        public void TestImport()
        {
            var registryParser = new RegFileParser();
            // ReSharper disable once AssignNullToNotNullAttribute
            var parsed = registryParser.Parse(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry", "team.reg"));
        }
    }
}
