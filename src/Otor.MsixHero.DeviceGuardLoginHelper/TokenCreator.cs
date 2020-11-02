using System;
using System.IO;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Appx.Signing.DeviceGuard;

namespace Otor.MsixHero.DeviceGuardLoginHelper
{
    public class DgssTokenCreator
    {
        private static readonly string[] Scope = { "https://onestore.microsoft.com/user_impersonation" };


        public async Task<DeviceGuardConfig> SignIn(CancellationToken cancellationToken)
        {
            var factory = new MsixHeroClientFactory();
            string refreshToken = null;
            EventHandler<string> gotRefreshToken = (sender, s) =>
            {
                refreshToken = s;
            };

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
                
                return tokens;
            }
            finally
            {
                factory.GotRefreshToken -= gotRefreshToken;
            }
        }

        public SecureString CreateDeviceGuardJsonToken(DeviceGuardConfig tokens, CancellationToken cancellationToken = default)
        {
            var secureString = new SecureString();

            var jsonObject = new JObject();
            jsonObject["access_token"] = tokens.AccessToken;
            jsonObject["refresh_token"] = tokens.RefreshToken;

            foreach (var c in jsonObject.ToString(Formatting.Indented))
            {
                secureString.AppendChar(c);
            }

            return secureString;
        }

        public async Task<string> CreateDeviceGuardJsonTokenFile(DeviceGuardConfig tokens, CancellationToken cancellationToken = default)
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), "msixhero-dg-" + Guid.NewGuid().ToString("N").Substring(0, 12) + ".json");

            using (var text = File.Create(tmpFile))
            {
                var jsonObject = new JObject();
                jsonObject["access_token"] = tokens.AccessToken;
                jsonObject["refresh_token"] = tokens.RefreshToken;

                using (var streamWriter = new StreamWriter(text))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await streamWriter.WriteAsync(jsonObject.ToString(Formatting.Indented)).ConfigureAwait(false);
                }
            }

            return tmpFile;
        }

        private class MsixHeroClientFactory : IMsalHttpClientFactory
        {
            public event EventHandler<string> GotRefreshToken;

            private void RaiseGotRefreshToken(string token)
            {
                this.GotRefreshToken?.Invoke(this, token);
            }

            public HttpClient GetHttpClient() => new HttpClient(new MsixHeroDelegationHandler(this));

            private class MsixHeroDelegationHandler : DelegatingHandler
            {
                private readonly MsixHeroClientFactory clientFactory;

                public MsixHeroDelegationHandler(MsixHeroClientFactory clientFactory)
                {
                    this.InnerHandler = new HttpClientHandler();
                    this.clientFactory = clientFactory;
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
                        var responseText = await response.Content.ReadAsStringAsync();
                        var responseJson = JObject.Parse(responseText);
                        this.clientFactory.RaiseGotRefreshToken(responseJson["refresh_token"]?.Value<string>());
                    }
                    catch (Exception)
                    {
                        this.clientFactory.RaiseGotRefreshToken(null);
                    }

                    return response;
                }
            }
        }
    }
}