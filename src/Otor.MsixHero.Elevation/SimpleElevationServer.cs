// MIT License
// 
// Copyright(c) 2022 Marcin Otorowski
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// https://github.com/marcinotorowski/simpleelevation/blob/develop/LICENSE.md

using System.IO.Pipes;
using System.Reflection;
using System.Security.Principal;
using Dapplo.Log;
using Otor.MsixHero.Elevation.Ipc;
using Otor.MsixHero.Elevation.Ipc.Helpers;
using Otor.MsixHero.Elevation.Ipc.Native;
using Otor.MsixHero.Elevation.Progress;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Elevation;

public class SimpleElevationServer : SimpleElevationBase
{
    private static readonly LogSource Log = new LogSource();

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <param name="repeat">If to repeat.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that encapsulates the lifetime of the server operation.</returns>
    public async Task StartAsync(bool repeat = true, CancellationToken cancellationToken = default)
    {
        Log.Info().WriteLine("UAC Server | Awaiting client connection...");

        do
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using var stream = NamedPipeWrapper.Create(PipeName);
                await stream.Stream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                var streamHelper = new IpcStreamHelper(stream.Stream);
                var handlingClass = new ProxyMethodHandler(streamHelper);

                var methodPayload = await streamHelper.ReadMethodInfo(cancellationToken).ConfigureAwait(false);
                if (methodPayload.MethodInfo == null)
                {
                    throw new InvalidOperationException("Missing method info.");
                }

                if (methodPayload.MethodInfo.DeclaringType == null)
                {
                    throw new InvalidOperationException("Missing declaring type.");
                }

                var actualObject = this.Resolve(methodPayload.MethodInfo.DeclaringType);

                await handlingClass.HandleMethodAsync(methodPayload.MethodInfo, actualObject, methodPayload.Parameters?.ToList() ?? new List<object?>(), cancellationToken).ConfigureAwait(false);
            }
            catch (IOException e)
            {
                repeat = false;
                Log.Warn().WriteLine(e);
            }
            catch (TargetInvocationException e)
            {
                if (!repeat)
                {
                    Log.Error().WriteLine(e);
                    throw;
                }

                Log.Warn().WriteLine(e.GetBaseException());
            }
            catch (Exception e)
            {
                if (!repeat)
                {
                    Log.Error().WriteLine(e);
                    throw;
                }

                Log.Warn().WriteLine(e);
            }

            if (repeat)
            {
                Log.Info().WriteLine("UAC Server | Request finished, awaiting client connection...");
            }
            else
            {
                Log.Info().WriteLine("UAC Server | Request finished.");
            }

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
        } while (repeat);
    }

    private class NamedPipeWrapper : IDisposable, IAsyncDisposable
    {
        private readonly NamedPipeServerStream _stream;

        private NamedPipeWrapper(NamedPipeServerStream stream)
        {
            _stream = stream;
        }

        public NamedPipeServerStream Stream => this._stream;

        public static NamedPipeWrapper Create(string pipeName)
        {
            var ps = new PipeSecurity();
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            ps.SetAccessRule(new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

            return new NamedPipeWrapper(NamedPipeNative.CreateNamedPipe(pipeName, ps));
        }

        public void Dispose()
        {
            if (this._stream.IsConnected)
            {
                // ReSharper disable once AccessToDisposedClosure
                ExceptionGuard.Guard(() => this._stream.Disconnect());
            }

            this._stream.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (this._stream.IsConnected)
            {
                ExceptionGuard.Guard(() => this._stream.Disconnect());
            }

            await ExceptionGuard.Guard(() => this._stream.DisposeAsync()).ConfigureAwait(false);
        }
    }

    private class ProxyMethodHandler
    {
        private readonly IpcStreamHelper _streamHelper;
        private Type? _progressPayloadType;
        private bool _hasFinished;

        public ProxyMethodHandler(IpcStreamHelper streamHelper)
        {
            this._streamHelper = streamHelper;
        }

        public async Task HandleMethodAsync(MethodInfo methodInfo, object target, IList<object?> parameters, CancellationToken cancellationToken = default)
        {
            if (methodInfo.DeclaringType == null)
            {
                throw new ArgumentException("Missing declaring type.", nameof(methodInfo));
            }

            if (Log.IsVerboseEnabled())
            {
                Log.Verbose().WriteLine("UAC Server | {0}", GetMethodLogInfo(methodInfo, parameters));
            }
            else
            {
                Log.Debug().WriteLine("UAC Server | Executing {0}.{1}...", methodInfo.DeclaringType.Name, methodInfo.Name);
            }

            var methodParamTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();

            for (var i = 0; i < methodParamTypes.Length; i++)
            {
                if (!TypeHelper.IsProgress(methodParamTypes[i], out var genericType))
                {
                    continue;
                }

                if (genericType == null)
                {
                    throw new InvalidOperationException("Could not create proxy progress");
                }

                var dummyProgress = Activator.CreateInstance(typeof(SimpleProgress<>).MakeGenericType(genericType)) ?? throw new InvalidOperationException("Could not create proxy progress");

                var reqType = typeof(SimpleProgress<>).MakeGenericType(genericType);
                this._progressPayloadType = genericType;

                var eventInfo = reqType.GetEvent(nameof(SimpleProgress<object>.ProgressChanged)) ?? throw new MissingMemberException("Missing event ProgressChanged.");

                var eventHandlerMethod = typeof(ProxyMethodHandler).GetMethod(nameof(ProgressHandler), BindingFlags.Instance | BindingFlags.NonPublic);

                var invokedMethodDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType!, this, eventHandlerMethod!);
                eventInfo.AddEventHandler(dummyProgress, invokedMethodDelegate);

                parameters[i] = dummyProgress; // overwrite i-th parameter with a progress
                break;
            }

            try
            {
                var methodInvocationResult = methodInfo.Invoke(target, parameters.ToArray());

                if (methodInfo.ReturnType == typeof(void))
                {
                    this._hasFinished = true;
                    await _streamHelper.WriteComplete(cancellationToken).ConfigureAwait(false);
                }
                else if (methodInfo.ReturnType.IsAssignableTo(typeof(Task)))
                {
                    // A task that does not require a value.

                    if (methodInvocationResult == null)
                    {
                        throw new InvalidOperationException("Invocation result cannot be null.");
                    }

                    await ((Task)methodInvocationResult).ConfigureAwait(false);
                    this._hasFinished = true;

                    if (methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        // Task<T>, returning a value.
                        var returned = methodInvocationResult.GetType().GetProperty(nameof(Task<object>.Result), BindingFlags.Instance | BindingFlags.Public)!.GetValue(methodInvocationResult);
                        await _streamHelper.WriteComplete(returned, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        // Task, not returning a value.
                        await _streamHelper.WriteComplete(cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    this._hasFinished = true;
                    await _streamHelper.WriteComplete(methodInvocationResult, cancellationToken).ConfigureAwait(false);
                }

                await this._streamHelper.GetUnderlyingStream().FlushAsync(cancellationToken).ConfigureAwait(false);
                ((NamedPipeServerStream)this._streamHelper.GetUnderlyingStream()).WaitForPipeDrain();
            }
            catch (IOException)
            {
                this._hasFinished = true;
                Log.Warn().WriteLine("UAC Server | The client is not anymore interested in the results. The pipe is broken.");
                throw;
            }
            catch (TargetInvocationException e)
            {
                this._hasFinished = true;
                await _streamHelper.WriteException(e.GetBaseException(), cancellationToken).ConfigureAwait(false);

                await this._streamHelper.GetUnderlyingStream().FlushAsync(cancellationToken).ConfigureAwait(false);
                ((NamedPipeServerStream)this._streamHelper.GetUnderlyingStream()).WaitForPipeDrain();

                throw;
            }
            catch (Exception e)
            {
                this._hasFinished = true;
                await _streamHelper.WriteException(e, cancellationToken).ConfigureAwait(false);

                await this._streamHelper.GetUnderlyingStream().FlushAsync(cancellationToken).ConfigureAwait(false);
                ((NamedPipeServerStream)this._streamHelper.GetUnderlyingStream()).WaitForPipeDrain();

                throw;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void ProgressHandler(object _, object eventArgs)
        {
            if (this._hasFinished || this._progressPayloadType == null)
            {
                return;
            }

            if (eventArgs is int)
            {
                Log.Debug().WriteLine($"             -> Progress = {eventArgs}%...");
            }
            else
            {
                Log.Debug().WriteLine($"             -> Progress = {eventArgs}...");
            }

            try
            {
                this._streamHelper.WriteProgress(this._progressPayloadType, eventArgs).GetAwaiter().GetResult();
                this._streamHelper.GetUnderlyingStream().Flush();
            }
            catch (IOException)
            {
                Log.Warn().WriteLine("UAC Server | The client is not anymore interested in the results. The pipe is broken.");
                this._hasFinished = true;
            }
        }
    }
}