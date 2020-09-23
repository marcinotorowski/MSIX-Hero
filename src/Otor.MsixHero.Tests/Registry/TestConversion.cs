using NUnit.Framework;
using Otor.MsixHero.Registry.Converter;
using Otor.MsixHero.Registry.Parser;

namespace Otor.MsixHero.Tests.Registry
{
    [TestFixture]
    public class TestConversion
    {
        [Test]
        public void Convert()
        {
            var regConv = new RegConverter();
            regConv.ConvertFromRegToDat(@"C:\Users\marci\Desktop\team.reg", @"C:\Users\marci\Desktop\Registry.dat", RegistryRoot.HKEY_LOCAL_MACHINE).GetAwaiter().GetResult();
            regConv.ConvertFromRegToDat(@"C:\Users\marci\Desktop\team.reg", @"C:\Users\marci\Desktop\UserClasses.dat", RegistryRoot.HKEY_CLASSES_ROOT).GetAwaiter().GetResult();
            regConv.ConvertFromRegToDat(@"C:\Users\marci\Desktop\team.reg", @"C:\Users\marci\Desktop\User.dat", RegistryRoot.HKEY_CURRENT_USER).GetAwaiter().GetResult();
        }
    }
}
