using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using otor.msixhero.lib.BusinessLayer.Winget;

namespace otor.msixhero.lib.tests.Winget
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
                Assert.AreEqual("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
            }
            finally
            {
                File.Delete(testFile);
            }
        }

        [Test]
        public void ReadMsi()
        {
            var msi = @"C:\Users\marci\Downloads\ssl\marcinotorowski\doublecmd-0.9.8.x86_64-win64.msi";
            var m = new Msi();
            var props = m.GetProperties(msi);
        }

        [Test]
        public void TestWebHashing()
        {
            var url = "https://msixhero.net/msix-hero.appinstaller";
            var hash = new YamlUtils().CalculateHashAsync(new Uri(url)).Result;
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
