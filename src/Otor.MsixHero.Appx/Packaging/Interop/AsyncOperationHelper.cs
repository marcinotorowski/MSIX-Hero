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
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Management.Deployment;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.Interop
{
    public class AsyncOperationHelper
    {
        public static async Task<T> ConvertToTask<T>(IAsyncOperationWithProgress<T, DeploymentProgress> operation, string caption, CancellationToken token = default, IProgress<ProgressData> progress = default)
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

        public static async Task<T> ConvertToTask<T>(IAsyncOperation<T> operation, CancellationToken token = default)
        {
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
            
            var res = await cts.Task.ConfigureAwait(false);
            return res;
        }
    }
}
