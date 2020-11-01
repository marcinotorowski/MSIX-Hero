using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Signing.DeviceGuard
{
    public class DgssTokenCreator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DgssTokenCreator));

        private static readonly string[] Scope = { "https://onestore.microsoft.com/user_impersonation" };

        public async Task<DeviceGuardConfig> SignIn(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            Logger.Info("Signing in to AzureAD...");
            progress?.Report(new ProgressData(50, "Waiting for authentication..."));
            var tokens = new DeviceGuardConfig();
            var pipeName = "msixhero-" + Guid.NewGuid().ToString("N");
            using (var namedPipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In))
            {
                var entry = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;

                // ReSharper disable once AssignNullToNotNullAttribute
                var startPath = Path.Combine(Path.GetDirectoryName(entry), "DGSS", "msixhero-dgss.exe");
                Logger.Debug($"Starting {startPath}...");
                var process = new ProcessStartInfo(
                    startPath,
                    pipeName);

                var tcs = new TaskCompletionSource<int>();

                var start = Process.Start(process);
                if (start == null)
                {
                    throw new InvalidOperationException();
                }

                start.EnableRaisingEvents = true;
                start.Exited += (sender, args) =>
                {
                    tcs.SetResult(start.ExitCode);
                };

                await namedPipeServer.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                var bufferInt = new byte[4];
                await namedPipeServer.ReadAsync(bufferInt, 0, bufferInt.Length, cancellationToken).ConfigureAwait(false);

                var bufferObj = new byte[BitConverter.ToInt32(bufferInt, 0)];
                await namedPipeServer.ReadAsync(bufferObj, 0, bufferObj.Length, cancellationToken).ConfigureAwait(false);

                var text = System.Text.Encoding.UTF8.GetString(bufferObj);
                var obj = JObject.Parse(text);

                if (obj["exception"] != null)
                {
                    var type = obj["exception"]["type"]?.Value<string>();
                    var message = obj["exception"]["message"]?.Value<string>();
                    var errorCode = obj["exception"]["errorCode"]?.Value<string>();

                    Logger.Error($"Got exception (type: {type}, error code: {errorCode}, message: {message}");

                    switch (type)
                    {
                        case nameof(AuthenticationException):
                            throw new AuthenticationException(message);
                        case nameof(MsalException):
                            throw new MsalException(errorCode ?? "1", message);
                        case nameof(MsalClientException):
                            throw new MsalClientException(errorCode ?? "1", message);
                        default:
                            throw new Exception(message);
                    }
                }

                tokens.AccessToken = obj["access_token"]?.Value<string>();
                tokens.RefreshToken = obj["refresh_token"]?.Value<string>();

                Logger.Info("Got access and refresh token!");
                await tcs.Task.ConfigureAwait(false);
            }

            string tempJsonFile = null;
            try
            {
                var dgh = new DeviceGuardHelper();
                progress?.Report(new ProgressData(75, "Verifying signing capabilities..."));
                tempJsonFile = await this.CreateDeviceGuardJsonTokenFile(tokens, cancellationToken).ConfigureAwait(false);
                progress?.Report(new ProgressData(98, "Verifying signing capabilities..."));
                var subject = await dgh.GetSubjectFromDeviceGuardSigning(tempJsonFile, false, cancellationToken).ConfigureAwait(false);
                tokens.Subject = subject;
            }
            finally
            {
                if (tempJsonFile != null)
                {
                    Logger.Debug($"Removing {tempJsonFile}...");
                    ExceptionGuard.Guard(() => File.Delete(tempJsonFile));
                }
            }

            return tokens;
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
            var tmpFile = Path.Combine(Path.GetTempPath(), "msix-hero-dg-" + Guid.NewGuid().ToString("N").Substring(0, 12) + ".json");

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
                private static readonly ILog Logger = LogManager.GetLogger(typeof(MsixHeroDelegationHandler));
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
                        this.clientFactory.RaiseGotRefreshToken(responseJson.Property("refresh_token")?.Value<string>());
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Unable to retrieve the refresh token due to the following exception: {0}.", ex.Message);
                        this.clientFactory.RaiseGotRefreshToken(null);
                    }

                    return response;
                }
            }
        }
    }
}