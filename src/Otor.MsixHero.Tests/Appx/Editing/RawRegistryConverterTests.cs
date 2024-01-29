using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Otor.MsixHero.Cli.Executors.Edit.Registry;

namespace Otor.MsixHero.Tests.Appx.Editing
{
    public class RawRegistryConverterTests
    {
        [Test]
        public void TestBinaryConversion()
        {
            var originalText = "hello-world";
            var originalBytes = Encoding.UTF8.GetBytes(originalText);
            var asBase64 = Convert.ToBase64String(originalBytes);

            var converted = RawRegistryValueConverter.GetByteArrayFromString(asBase64);
            Assert.That(originalBytes.SequenceEqual(converted), Is.True);

            asBase64 += "=22";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetByteArrayFromString(asBase64));

            var asHex = "0x" + Convert.ToHexString(originalBytes);
            converted = RawRegistryValueConverter.GetByteArrayFromString(asHex);
            Assert.That(converted.SequenceEqual(originalBytes), Is.True);

            var asDecimal = string.Join(", ", originalBytes.Select(b => b.ToString("0")));
            converted = RawRegistryValueConverter.GetByteArrayFromString(asDecimal);
            Assert.That(converted.SequenceEqual(originalBytes), Is.True);
        }

        [Test]
        public void TestDWordConversion()
        {
            var inputAsNumber = "134";
            Assert.That(RawRegistryValueConverter.GetDWordFromString(inputAsNumber), Is.EqualTo((uint)134));

            inputAsNumber = "0x10";
            Assert.That(RawRegistryValueConverter.GetDWordFromString(inputAsNumber), Is.EqualTo((uint)0x10));

            inputAsNumber = uint.MaxValue.ToString("0") + "0";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetDWordFromString(inputAsNumber));

            inputAsNumber = "abc";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetDWordFromString(inputAsNumber));

            inputAsNumber = "";
            Assert.That(RawRegistryValueConverter.GetDWordFromString(inputAsNumber), Is.EqualTo((uint)0));
        }

        [Test]
        public void TestQWordConversion()
        {
            var inputAsNumber = "134";
            Assert.That(RawRegistryValueConverter.GetQWordFromString(inputAsNumber), Is.EqualTo((ulong)134));

            inputAsNumber = "0x10";
            Assert.That(RawRegistryValueConverter.GetQWordFromString(inputAsNumber), Is.EqualTo((ulong)0x10));

            inputAsNumber = ulong.MaxValue.ToString("0");
            Assert.That(RawRegistryValueConverter.GetQWordFromString(inputAsNumber), Is.EqualTo(ulong.MaxValue));

            inputAsNumber = ulong.MaxValue.ToString("0") + "0";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetQWordFromString(inputAsNumber));

            inputAsNumber = "abc";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetQWordFromString(inputAsNumber));

            inputAsNumber = "";
            Assert.That(RawRegistryValueConverter.GetQWordFromString(inputAsNumber), Is.EqualTo((ulong)0));
        }

        [Test]
        public void TestMultiStringConversion()
        {
            var inputAsNumber = "1|22|aaa|";
            var converted = RawRegistryValueConverter.GetMultiValueFromString(inputAsNumber);
            Assert.That(converted.SequenceEqual(new[] { "1", "22", "aaa", "" }), Is.True);

            inputAsNumber = "";
            converted = RawRegistryValueConverter.GetMultiValueFromString(inputAsNumber);
            Assert.That(converted.Length, Is.EqualTo(1));

            inputAsNumber = " ";
            converted = RawRegistryValueConverter.GetMultiValueFromString(inputAsNumber);
            Assert.That(converted.SequenceEqual(new[] { " " }), Is.True);
        }
    }
}
