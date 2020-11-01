using System;
using System.ComponentModel;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Appx.Signing.DeviceGuard;

namespace Otor.MsixHero.DeviceGuardLoginHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool canClose = false;
        private readonly string serverId;

        public MainWindow()
        {
            this.serverId = "MSIXHero-" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                this.canClose = true;
                try
                {
                    this.SendToServer(new InvalidOperationException("Wrong number of parameters.")).GetAwaiter().GetResult();
                }
                catch
                {
                }

                this.Close();
                return;
            }

            this.serverId = args.Skip(1).First();
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!this.canClose)
            {
                e.Cancel = true;
            }
        }

        private async Task DoClient()
        {
            JObject result;
            try
            {
                var dgss = new DgssTokenCreator();
                var r = await dgss.SignIn(CancellationToken.None).ConfigureAwait(true);
                result = new JObject
                {
                    ["access_token"] = r.AccessToken,
                    ["refresh_token"] = r.RefreshToken,
                };

                await this.SendToServer(result);
                this.Close();
                Environment.Exit(0);
            }
            catch (MsalClientException msalExc)
            {
                await this.SendToServer(msalExc);
                if (msalExc.ErrorCode == null)
                {
                    this.Close();
                    Environment.Exit(1);
                }
                else
                {
                    this.Close();
                    Environment.Exit(int.TryParse(msalExc.ErrorCode, out var exit) ? exit : 1);
                }
            }
            catch (MsalException msalExc)
            {
                await this.SendToServer(msalExc);
                if (msalExc.ErrorCode == null)
                {
                    this.Close();
                    Environment.Exit(1);
                }
                else
                {
                    this.Close();
                    Environment.Exit(int.TryParse(msalExc.ErrorCode, out var exit) ? exit : 1);
                }
            }
            catch (Exception exception)
            {
                await this.SendToServer(exception);
                this.Close();
                Environment.Exit(1);
            }
        }
        
        private async Task SendToServer(JObject objectToSend = null)
        {
            using (var namedPipeClient = new NamedPipeClientStream(".", serverId, PipeDirection.Out))
            {
                await namedPipeClient.ConnectAsync(1).ConfigureAwait(false);

                if (objectToSend == null)
                {
                    var chunk1 = BitConverter.GetBytes(0);
                    await namedPipeClient.WriteAsync(chunk1, 0, chunk1.Length).ConfigureAwait(false);
                }
                else
                {
                    var textToSend = objectToSend.ToString(Formatting.None);
                    var chunk1 = BitConverter.GetBytes(textToSend.Length);
                    var chunk2 = System.Text.Encoding.UTF8.GetBytes(textToSend);
                    await namedPipeClient.WriteAsync(chunk1, 0, chunk1.Length).ConfigureAwait(false);
                    await namedPipeClient.WriteAsync(chunk2, 0, chunk2.Length).ConfigureAwait(false);
                }
            }
        }

        private Task SendToServer(Exception exceptionToSend)
        {
            var result = new JObject
            {
                ["exception"] = new JObject
                {
                    ["type"] = exceptionToSend.GetType().Name,
                    ["message"] = exceptionToSend.Message
                }
            };

            if (exceptionToSend is MsalClientException msalClient)
            {
                result["errorCode"] = msalClient.ErrorCode;
            }
            else if (exceptionToSend is MsalException msal)
            {
                result["errorCode"] = msal.ErrorCode;
            }

            return this.SendToServer(result);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            this.DoClient();
#pragma warning restore 4014
        }
    }
}
