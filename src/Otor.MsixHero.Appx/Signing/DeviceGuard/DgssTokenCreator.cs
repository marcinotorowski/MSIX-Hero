﻿// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Signing.DeviceGuard
{
    public class DgssTokenCreator
    {
        private static readonly string[] Scope = ["https://onestore.microsoft.com/user_impersonation"];
        
        public static async Task<DeviceGuardConfig> SignIn(bool validateSubject = false, IProgress<ProgressData> progress = default, CancellationToken cancellationToken = default)
        {
            var factory = new MsixHeroClientFactory();
            string refreshToken = null;
            EventHandler<string> gotRefreshToken = (_, s) =>
            {
                refreshToken = s;
            };

            progress?.Report(new ProgressData(0, Resources.Localization.Signing_DeviceGuard_LoggingIn));

            try
            {
                factory.GotRefreshToken += gotRefreshToken;
                var clientApp = PublicClientApplicationBuilder.Create("4dd963fd-7400-4ce3-bc90-0bed2b65820d")
                    .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                    .WithHttpClientFactory(factory)
                    .Build();

                await clientApp.GetAccountsAsync().ConfigureAwait(true);
                var result = await clientApp.AcquireTokenInteractive(Scope).WithPrompt(Prompt.ForceLogin).ExecuteAsync(cancellationToken).ConfigureAwait(false);
                var tokens = new DeviceGuardConfig(result.AccessToken, refreshToken);

                if (validateSubject)
                {
                    progress?.Report(new ProgressData(50, Resources.Localization.Signing_DeviceGuard_Validation));

                    var json = await CreateDeviceGuardJsonTokenFile(new DeviceGuardConfig(result.AccessToken, refreshToken), cancellationToken).ConfigureAwait(false);
                    try
                    {
                        // set the result subject.
                        tokens.Subject = await DeviceGuardHelper.GetSubjectFromDeviceGuardSigning(json, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (json != null && File.Exists(json))
                        {
                            File.Delete(json);
                        }
                    }
                }

                return tokens;
            }
            finally
            {
                factory.GotRefreshToken -= gotRefreshToken;
            }
        }

        public static async Task<string> CreateDeviceGuardJsonTokenFile(DeviceGuardConfig tokens, CancellationToken cancellationToken = default)
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), "msixhero-dg-" + Guid.NewGuid().ToString("N")[..12] + ".json");

            await using var text = File.Create(tmpFile);
            var jsonObject = new JObject
            {
                ["access_token"] = tokens.AccessToken,
                ["refresh_token"] = tokens.RefreshToken
            };

            await using var streamWriter = new StreamWriter(text);
            cancellationToken.ThrowIfCancellationRequested();
            await streamWriter.WriteAsync(jsonObject.ToString(Formatting.Indented)).ConfigureAwait(false);

            return tmpFile;
        }

        private class MsixHeroClientFactory : IMsalHttpClientFactory
        {
            public event EventHandler<string> GotRefreshToken;

            private void RaiseGotRefreshToken(string token)
            {
                this.GotRefreshToken?.Invoke(this, token);
            }

            public HttpClient GetHttpClient() => new(new MsixHeroDelegationHandler(this));

            private class MsixHeroDelegationHandler : DelegatingHandler
            {
                private readonly MsixHeroClientFactory _clientFactory;

                public MsixHeroDelegationHandler(MsixHeroClientFactory clientFactory)
                {
                    this.InnerHandler = new HttpClientHandler();
                    this._clientFactory = clientFactory;
                }

                protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                {
                    var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                    if (request.Method != HttpMethod.Post)
                    {
                        return response;
                    }
                    
                    try
                    {
                        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        var responseJson = JObject.Parse(responseText);
                        this._clientFactory.RaiseGotRefreshToken(responseJson["refresh_token"]?.Value<string>());
                    }
                    catch (Exception)
                    {
                        this._clientFactory.RaiseGotRefreshToken(null);
                    }

                    return response;
                }
            }
        }
    }
}