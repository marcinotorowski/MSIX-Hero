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
            Assert.IsTrue(originalBytes.SequenceEqual(converted));

            asBase64 += "=22";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetByteArrayFromString(asBase64));

            var asHex = "0x" + Convert.ToHexString(originalBytes);
            converted = RawRegistryValueConverter.GetByteArrayFromString(asHex);
            Assert.IsTrue(converted.SequenceEqual(originalBytes));

            var asDecimal = string.Join(", ", originalBytes.Select(b => b.ToString("0")));
            converted = RawRegistryValueConverter.GetByteArrayFromString(asDecimal);
            Assert.IsTrue(converted.SequenceEqual(originalBytes));
        }

        [Test]
        public void TestDWordConversion()
        {
            var inputAsNumber = "134";
            Assert.AreEqual((uint)134, RawRegistryValueConverter.GetDWordFromString(inputAsNumber));

            inputAsNumber = "0x10";
            Assert.AreEqual((uint)0x10, RawRegistryValueConverter.GetDWordFromString(inputAsNumber));

            inputAsNumber = uint.MaxValue.ToString("0") + "0";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetDWordFromString(inputAsNumber));

            inputAsNumber = "abc";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetDWordFromString(inputAsNumber));

            inputAsNumber = "";
            Assert.AreEqual((uint)0, RawRegistryValueConverter.GetDWordFromString(inputAsNumber));
        }

        [Test]
        public void TestQWordConversion()
        {
            var inputAsNumber = "134";
            Assert.AreEqual((ulong)134, RawRegistryValueConverter.GetQWordFromString(inputAsNumber));

            inputAsNumber = "0x10";
            Assert.AreEqual((ulong)0x10, RawRegistryValueConverter.GetQWordFromString(inputAsNumber));

            inputAsNumber = ulong.MaxValue.ToString("0");
            Assert.AreEqual(ulong.MaxValue, RawRegistryValueConverter.GetQWordFromString(inputAsNumber));

            inputAsNumber = ulong.MaxValue.ToString("0") + "0";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetQWordFromString(inputAsNumber));

            inputAsNumber = "abc";
            Assert.Throws<ArgumentException>(() => RawRegistryValueConverter.GetQWordFromString(inputAsNumber));

            inputAsNumber = "";
            Assert.AreEqual((ulong)0, RawRegistryValueConverter.GetQWordFromString(inputAsNumber));
        }

        [Test]
        public void TestMultiStringConversion()
        {
            var inputAsNumber = "1|22|aaa|";
            var converted = RawRegistryValueConverter.GetMultiValueFromString(inputAsNumber);
            Assert.IsTrue(converted.SequenceEqual(new[] { "1", "22", "aaa", "" }));

            inputAsNumber = "";
            converted = RawRegistryValueConverter.GetMultiValueFromString(inputAsNumber);
            Assert.AreEqual(1, converted.Length);

            inputAsNumber = " ";
            converted = RawRegistryValueConverter.GetMultiValueFromString(inputAsNumber);
            Assert.IsTrue(converted.SequenceEqual(new[] { " " }));
        }
    }
}
