using System;
using NUnit.Framework;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;

namespace Otor.MsixHero.Tests.Appx.Editing
{
    public class VersionStringOperationsTests
    {
        [Test]
        public void TestSimpleMasks()
        {
            Assert.AreEqual("1.2.3.4", VersionStringOperations.ResolveMaskedVersion("1.2.3.4"));
            Assert.AreEqual("1.2.3.4", VersionStringOperations.ResolveMaskedVersion("1.2.3.4", "1.2.3.0"));
        }

        [Test]
        public void TestWrongInputs()
        {
            Assert.Throws<ArgumentException>(() => VersionStringOperations.ResolveMaskedVersion("auto2"));
            Assert.Throws<ArgumentException>(() => VersionStringOperations.ResolveMaskedVersion("1.2.3.a"));
            Assert.Throws<ArgumentException>(() => VersionStringOperations.ResolveMaskedVersion("auto", "1.2.3.a"));
            Assert.Throws<ArgumentException>(() => VersionStringOperations.ResolveMaskedVersion("1.2.3.4", "abc"));
        }

        [Test]
        public void TestAutoMask()
        {
            Assert.AreEqual("1.0.0.0", VersionStringOperations.ResolveMaskedVersion("auto"));
            Assert.AreEqual("1.0.0.1", VersionStringOperations.ResolveMaskedVersion("auto", "1.0.0.0"));
        }

        [Test]
        public void TestIncrementation()
        {
            Assert.AreEqual("2.1.1.1", VersionStringOperations.ResolveMaskedVersion("^.^.^.^"));
            Assert.AreEqual("2.3.4.5", VersionStringOperations.ResolveMaskedVersion("^.^.^.^", "1.2.3.4"));
        }

        [Test]
        public void TestMasking()
        {
            Assert.AreEqual("1.2.3.0", VersionStringOperations.ResolveMaskedVersion("1.2.3.*"));
            Assert.AreEqual("1.2.0.3", VersionStringOperations.ResolveMaskedVersion("1.2.*.3"));
            Assert.AreEqual("1.0.2.3", VersionStringOperations.ResolveMaskedVersion("1.*.2.3"));
            Assert.AreEqual("1.1.2.3", VersionStringOperations.ResolveMaskedVersion("*.1.2.3"));

            Assert.AreEqual("1.2.3.0", VersionStringOperations.ResolveMaskedVersion("x.2.3.*"));
            Assert.AreEqual("1.0.0.3", VersionStringOperations.ResolveMaskedVersion("1.x.*.3"));
            Assert.AreEqual("1.0.0.3", VersionStringOperations.ResolveMaskedVersion("1.*.x.3"));
            Assert.AreEqual("1.1.2.0", VersionStringOperations.ResolveMaskedVersion("*.1.2.x"));

            Assert.AreEqual("1.2.3.0", VersionStringOperations.ResolveMaskedVersion("x.2.3."));
            Assert.AreEqual("1.0.0.3", VersionStringOperations.ResolveMaskedVersion("1.x..3"));
            Assert.AreEqual("1.0.0.3", VersionStringOperations.ResolveMaskedVersion("1..x.3"));
            Assert.AreEqual("1.1.2.0", VersionStringOperations.ResolveMaskedVersion(".1.2.x"));
            
            Assert.AreEqual("1.2.3.8", VersionStringOperations.ResolveMaskedVersion("1.2.3.*", "5.6.7.8"));
            Assert.AreEqual("1.2.7.3", VersionStringOperations.ResolveMaskedVersion("1.2.*.3", "5.6.7.8"));
            Assert.AreEqual("1.6.2.3", VersionStringOperations.ResolveMaskedVersion("1.*.2.3", "5.6.7.8"));
            Assert.AreEqual("5.1.2.3", VersionStringOperations.ResolveMaskedVersion("*.1.2.3", "5.6.7.8"));

            Assert.AreEqual("5.2.3.8", VersionStringOperations.ResolveMaskedVersion("x.2.3.*", "5.6.7.8"));
            Assert.AreEqual("1.6.7.3", VersionStringOperations.ResolveMaskedVersion("1.x.*.3", "5.6.7.8"));
            Assert.AreEqual("1.6.7.3", VersionStringOperations.ResolveMaskedVersion("1.*.x.3", "5.6.7.8"));
            Assert.AreEqual("5.1.2.8", VersionStringOperations.ResolveMaskedVersion("*.1.2.x", "5.6.7.8"));

            Assert.AreEqual("5.6.3.8", VersionStringOperations.ResolveMaskedVersion("x..3.*", "5.6.7.8"));
            Assert.AreEqual("1.6.7.3", VersionStringOperations.ResolveMaskedVersion("1..*.3", "5.6.7.8"));
            Assert.AreEqual("5.6.7.3", VersionStringOperations.ResolveMaskedVersion(".*.x.3", "5.6.7.8"));
            Assert.AreEqual("5.6.2.8", VersionStringOperations.ResolveMaskedVersion("*..2.x", "5.6.7.8"));
        }
    }
}
