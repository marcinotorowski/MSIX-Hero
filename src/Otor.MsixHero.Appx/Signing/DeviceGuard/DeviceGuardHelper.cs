using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Signing.DeviceGuard
{
    public class DeviceGuardHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DeviceGuardHelper));


        public async Task<string> GetSubjectFromDeviceGuardSigning(string accessToken, string refreshToken, bool useDgssV1, CancellationToken cancellationToken = default)
        {
            string tempFile = null;
            try
            {
                var cfg = new DeviceGuardConfig
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                var helper = new DgssTokenCreator();
                tempFile = await helper.CreateDeviceGuardJsonTokenFile(cfg, cancellationToken).ConfigureAwait(false);

                return await this.GetSubjectFromDeviceGuardSigning(tempFile, useDgssV1, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (tempFile != null && File.Exists(tempFile))
                {
                    ExceptionGuard.Guard(() => File.Delete(tempFile));
                }
            }
        }

        public async Task<string> GetSubjectFromDeviceGuardSigning(string dgssTokenPath, bool useDgssV1, CancellationToken cancellationToken = default)
        {
            Logger.Info("Getting certificate subject for Device Guard signing...");

            var tempFilePath = Path.Combine(Path.GetTempPath(), "msix-hero-" + Guid.NewGuid().ToString("N") + ".cat");
            try
            {
                var name = typeof(DeviceGuardHelper).Assembly.GetManifestResourceNames().First(n => n.EndsWith("MSIXHeroTest.cat"));
                using (var manifestResourceStream = typeof(DeviceGuardHelper).Assembly.GetManifestResourceStream(name))
                {
                    if (manifestResourceStream == null)
                    {
                        throw new InvalidOperationException("Cannot extract temporary file.");
                    }

                    Logger.Debug($"Creating temporary file path {tempFilePath}");
                    using (var fileStream = File.Create(tempFilePath))
                    {
                        manifestResourceStream.Seek(0L, SeekOrigin.Begin);
                        await manifestResourceStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
                    }
                }

                var sdk = new MsixSdkWrapper();
                Logger.Debug($"Signing temporary file path {tempFilePath}");
                await sdk.SignPackageWithDeviceGuard(new[] {tempFilePath}, "SHA256", dgssTokenPath, useDgssV1, null, cancellationToken).ConfigureAwait(false);

                using (var fromSignedFile = X509Certificate.CreateFromSignedFile(tempFilePath))
                {
                    Logger.Info($"Certificate subject is {tempFilePath}");
                    return fromSignedFile.Subject;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Could not read subject from Device Guard certificate.", e);
                throw;
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    Logger.Debug($"Removing {tempFilePath}");
                    ExceptionGuard.Guard(() => File.Delete(tempFilePath));
                }
            }
        }
    }
}
