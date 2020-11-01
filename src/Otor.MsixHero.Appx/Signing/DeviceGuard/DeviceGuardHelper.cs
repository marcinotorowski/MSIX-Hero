using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Signing.DeviceGuard
{
    public class DeviceGuardHelper
    {
        public async Task<string> GetSubjectFromDeviceGuardSigning(string dgssTokenPath, bool useDgssV1, CancellationToken cancellationToken = default)
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), "msix-hero-" + Guid.NewGuid().ToString("N") + ".cat");
            try
            {
                using (var manifestResourceStream = typeof(DeviceGuardHelper).Assembly.GetManifestResourceStream("Otor.MsixHero.Appx.Signing.DeviceGuard.MSIXHeroTest.cat"))
                {
                    if (manifestResourceStream == null)
                    {
                        throw new InvalidOperationException("Cannot extract temporary file.");
                    }

                    using (var fileStream = File.Create(tempFilePath))
                    {
                        manifestResourceStream.Seek(0L, SeekOrigin.Begin);
                        await manifestResourceStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
                    }
                }

                var sdk = new MsixSdkWrapper();
                await sdk.SignPackageWithDeviceGuard(new[] {tempFilePath}, "SHA256", dgssTokenPath, useDgssV1, null, cancellationToken).ConfigureAwait(false);

                using (var fromSignedFile = X509Certificate.CreateFromSignedFile(tempFilePath))
                {
                    return fromSignedFile.Subject;
                }
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    ExceptionGuard.Guard(() => File.Delete(tempFilePath));
                }
            }
        }
    }
}
