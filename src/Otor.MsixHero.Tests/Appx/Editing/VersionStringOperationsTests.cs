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
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.2.3.4"), Is.EqualTo("1.2.3.4"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.2.3.4", "1.2.3.0"), Is.EqualTo("1.2.3.4"));
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
            Assert.That(VersionStringOperations.ResolveMaskedVersion("auto"), Is.EqualTo("1.0.0.0"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("auto", "1.0.0.0"), Is.EqualTo("1.0.0.1"));
        }

        [Test]
        public void TestIncrementation()
        {
            Assert.That(VersionStringOperations.ResolveMaskedVersion("^.^.^.^"), Is.EqualTo("2.1.1.1"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("^.^.^.^", "1.2.3.4"), Is.EqualTo("2.3.4.5"));
        }

        [Test]
        public void TestMasking()
        {
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.2.3.*"), Is.EqualTo("1.2.3.0"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.2.*.3"), Is.EqualTo("1.2.0.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.*.2.3"), Is.EqualTo("1.0.2.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("*.1.2.3"), Is.EqualTo("1.1.2.3"));

            Assert.That(VersionStringOperations.ResolveMaskedVersion("x.2.3.*"), Is.EqualTo("1.2.3.0"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.x.*.3"), Is.EqualTo("1.0.0.3"));
                Assert.That(VersionStringOperations.ResolveMaskedVersion("1.*.x.3"), Is.EqualTo("1.0.0.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("*.1.2.x"), Is.EqualTo("1.1.2.0"));

            Assert.That(VersionStringOperations.ResolveMaskedVersion("x.2.3."), Is.EqualTo("1.2.3.0"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.x..3"), Is.EqualTo("1.0.0.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1..x.3"), Is.EqualTo("1.0.0.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion(".1.2.x"), Is.EqualTo("1.1.2.0"));
            
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.2.3.*", "5.6.7.8"), Is.EqualTo("1.2.3.8"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.2.*.3", "5.6.7.8"), Is.EqualTo("1.2.7.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.*.2.3", "5.6.7.8"), Is.EqualTo("1.6.2.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("*.1.2.3", "5.6.7.8"), Is.EqualTo("5.1.2.3"));

            Assert.That(VersionStringOperations.ResolveMaskedVersion("x.2.3.*", "5.6.7.8"), Is.EqualTo("5.2.3.8"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.x.*.3", "5.6.7.8"), Is.EqualTo("1.6.7.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1.*.x.3", "5.6.7.8"), Is.EqualTo("1.6.7.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("*.1.2.x", "5.6.7.8"), Is.EqualTo("5.1.2.8"));

            Assert.That(VersionStringOperations.ResolveMaskedVersion("x..3.*", "5.6.7.8"), Is.EqualTo("5.6.3.8"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("1..*.3", "5.6.7.8"), Is.EqualTo("1.6.7.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion(".*.x.3", "5.6.7.8"), Is.EqualTo("5.6.7.3"));
            Assert.That(VersionStringOperations.ResolveMaskedVersion("*..2.x", "5.6.7.8"), Is.EqualTo("5.6.2.8"));
        }
    }
}
