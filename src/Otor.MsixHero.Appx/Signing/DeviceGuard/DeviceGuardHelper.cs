// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Helpers;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Signing.DeviceGuard
{
    public class DeviceGuardHelper
    {
        private static readonly LogSource Logger = new();

        public async Task<string> GetSubjectFromDeviceGuardSigning(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
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

                return await this.GetSubjectFromDeviceGuardSigning(tempFile, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (tempFile != null && File.Exists(tempFile))
                {
                    ExceptionGuard.Guard(() => File.Delete(tempFile));
                }
            }
        }

        public async Task<string> GetSubjectFromDeviceGuardSigning(string dgssTokenPath, CancellationToken cancellationToken = default)
        {
            Logger.Info().WriteLine("Getting certificate subject for Device Guard signing…");

            var tempFilePath = Path.Combine(Path.GetTempPath(), "msix-hero-" + Guid.NewGuid().ToString("N") + ".cat");
            try
            {
                var name = typeof(DeviceGuardHelper).Assembly.GetManifestResourceNames().First(n => n.EndsWith("MSIXHeroTest.cat"));
                await using (var manifestResourceStream = typeof(DeviceGuardHelper).Assembly.GetManifestResourceStream(name))
                {
                    if (manifestResourceStream == null)
                    {
                        throw new InvalidOperationException("Cannot extract temporary file.");
                    }

                    Logger.Debug().WriteLine($"Creating temporary file path {tempFilePath}");
                    await using var fileStream = File.Create(tempFilePath);
                    manifestResourceStream.Seek(0L, SeekOrigin.Begin);
                    await manifestResourceStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
                }

                var sdk = new SignToolWrapper();
                Logger.Debug().WriteLine($"Signing temporary file path {tempFilePath}");
                await sdk.SignWithDeviceGuard(new[] {tempFilePath}, "SHA256", dgssTokenPath, null, cancellationToken).ConfigureAwait(false);

                using var fromSignedFile = X509Certificate.CreateFromSignedFile(tempFilePath);
                Logger.Info().WriteLine($"Certificate subject is {tempFilePath}");
                return fromSignedFile.Subject;
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine("Could not read subject from Device Guard certificate.");
                Logger.Error().WriteLine(e);
                throw;
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    Logger.Debug().WriteLine($"Removing {tempFilePath}");
                    ExceptionGuard.Guard(() => File.Delete(tempFilePath));
                }
            }
        }
    }
}
