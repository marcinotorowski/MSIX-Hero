using System.IO;
using System.Linq;
using NUnit.Framework;
using Otor.MsixHero.Infrastructure.Helpers;
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
            var tempReg = System.Guid.NewGuid().ToString("N") + ".reg";
            var tempDir = "reg-out-" + System.Guid.NewGuid().ToString("N");

            Directory.CreateDirectory(tempDir);
            try
            {
                var mfn = typeof(TestConversion).Assembly.GetManifestResourceNames().First(a => a.EndsWith("team.reg"));
                using (var mf = typeof(TestConversion).Assembly.GetManifestResourceStream(mfn))
                {
                    using (var fs = System.IO.File.OpenWrite(tempReg))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        mf.CopyTo(fs);
                        fs.Flush(true);
                    }
                }

                var regConv = new RegConverter();
                var regPars = new RegFileParser();

                regPars.Parse(Path.Combine(tempReg));
                regConv.ConvertFromRegToDat(tempReg, Path.Combine(tempDir, "Registry.dat"), RegistryRoot.HKEY_LOCAL_MACHINE).GetAwaiter().GetResult();
                regConv.ConvertFromRegToDat(tempReg, Path.Combine(tempDir, "UserClasses.dat"), RegistryRoot.HKEY_CLASSES_ROOT).GetAwaiter().GetResult();
                regConv.ConvertFromRegToDat(tempReg, Path.Combine(tempDir, "User.dat"), RegistryRoot.HKEY_CURRENT_USER).GetAwaiter().GetResult();
            }
            finally
            {
                ExceptionGuard.Guard(() =>
                {
                    System.IO.Directory.Delete(tempDir, true);
                    System.IO.File.Delete(tempReg);
                });
            }
        }
    }
}
