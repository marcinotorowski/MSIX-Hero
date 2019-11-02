using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Management.Deployment;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib.Managers
{
    public class AsyncOperationHelper
    {
        public async Task<T> ConvertToTask<T>(IAsyncOperationWithProgress<T, DeploymentProgress> operation, string caption, CancellationToken token = default, IProgress<ProgressData> progress = default)
        {
            if (progress != null)
            {
                progress.Report(new ProgressData(0, caption));
            }

            await Task.Delay(100, token).ConfigureAwait(false);

            var cts = new TaskCompletionSource<T>();

            operation.Completed += (info, status) =>
            {
                switch (status)
                {
                    case AsyncStatus.Completed:
                        cts.TrySetResult(info.GetResults());
                        break;
                    case AsyncStatus.Error:
                        cts.TrySetException(info.ErrorCode);
                        break;
                    case AsyncStatus.Canceled:
                        cts.TrySetCanceled(token);
                        break;
                    default:
                        cts.TrySetCanceled();
                        break;
                }
            };

            token.Register(() => { cts.TrySetCanceled(token); });

            if (progress != null)
            {
                operation.Progress += (info, progressInfo) =>
                {
                    progress.Report(new ProgressData((int)progressInfo.percentage, caption));
                };
            }

            var res = await cts.Task.ConfigureAwait(false);
            await Task.Delay(100, token).ConfigureAwait(false);
            return res;
        }
    }
}
