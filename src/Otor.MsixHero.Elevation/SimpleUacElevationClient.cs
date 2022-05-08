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
using Dapplo.Log;
using Newtonsoft.Json;
using Otor.MsixHero.Elevation.Handling;
using Otor.MsixHero.Elevation.Ipc;

namespace Otor.MsixHero.Elevation;

public class SimpleUacElevationClient : SimpleElevationBase, IUacElevation, IDisposable, IAsyncDisposable
{
    private static readonly LogSource Log = new LogSource();

    private readonly ClientHandler _handler;

    public SimpleUacElevationClient(ClientHandler handler)
    {
        this._handler = handler;
    }
    
    public T AsAdministrator<T>(bool keepElevation = true) where T : class
    {
        return AsAdministratorProxy<T>.Create(this._handler);
    }

    public T AsCurrentUser<T>() where T : class
    {
        return this.Resolve<T>();
    }

    public T AsHighestAvailable<T>() where T : class
    {
        return AsHighestAvailableProxy<T>.Create(this._handler, this.Resolve<T>);
    }
    
    public virtual void Dispose()
    {
        this._handler.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return this._handler.DisposeAsync();
    }

    private abstract class IpcProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                throw new ArgumentNullException(nameof(targetMethod));
            }

            if (targetMethod.DeclaringType == null)
            {
                throw new ArgumentException("Undefined declaring type.", nameof(targetMethod));
            }

            if (Log.IsVerboseEnabled())
            {
                Log.Verbose().WriteLine("UAC Client -> " + GetMethodLogInfo(targetMethod, args));
            }
            else
            {
                Log.Debug().WriteLine("UAC Client -> Executing {0}.{1}...", targetMethod.DeclaringType.Name, targetMethod.Name);
            }

            try
            {
                if (targetMethod.ReturnType == typeof(void))
                {
                    this.InvokeAsync(targetMethod, args).GetAwaiter().GetResult();
                    return null;
                }

                if (targetMethod.ReturnType == typeof(Task))
                {
                    return this.InvokeAsync(targetMethod, args);
                }

                if (targetMethod.ReturnType.IsGenericType &&
                    targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var obj = this.InvokeAsync(targetMethod, args);

                    var genericType = targetMethod.ReturnType.GetGenericArguments()[0];
                    var type = typeof(TaskCompletionSource<>).MakeGenericType(genericType);

                    var taskCompletionSource = Activator.CreateInstance(type);
                    var taskProp = type.GetProperty(nameof(TaskCompletionSource<object>.Task),
                        BindingFlags.Instance | BindingFlags.Public) ?? throw new MissingMethodException("Missing required property.");
                    var setResultMethod = type.GetMethod(nameof(TaskCompletionSource<object>.SetResult),
                        BindingFlags.Instance | BindingFlags.Public) ?? throw new MissingMethodException("Missing required method.");

                    obj.ContinueWith(r => { setResultMethod.Invoke(taskCompletionSource, new[] { r.Result }); });

                    var getMethod = taskProp.GetGetMethod() ?? throw new MissingMethodException("Missing required get method.");
                    return getMethod.Invoke(taskCompletionSource, Array.Empty<object>());
                }

                return this.InvokeAsync(targetMethod, args).Result;
            }
            catch (AggregateException exc)
            {
                var baseExc = exc.GetBaseException();
                throw new TargetInvocationException(baseExc);
            }
            catch (Exception exc)
            {
                throw new TargetInvocationException(exc);
            }
        }

        private async Task<object?> InvokeAsync(MethodInfo targetMethod, object?[]? args)
        {
            args ??= Array.Empty<object?>();

            var disposables = new List<IDisposable>();

            using var cts = new CancellationTokenSource();
            var privateToken = cts.Token;

            await using var client = new NamedPipeClientStream(PipeName);
            await client.ConnectAsync(cts.Token).ConfigureAwait(false);

            var binaryReader = new IpcStreamHelper(client);
            await binaryReader.Write(new IpcStreamHelper.Method(targetMethod, args), CancellationToken.None).ConfigureAwait(false);

            var findProgress = new Dictionary<int, Type>();

            var parameters = targetMethod.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                if (args.Length > i && args[i] != null)
                {
                    if (TypeHelper.IsProgress(parameters[i].ParameterType, out var _))
                    {
                        findProgress[i] = parameters[i].ParameterType;
                    }
                    else if (TypeHelper.IsCancellation(parameters[i].ParameterType))
                    {
#pragma warning disable CS8605
                        var actualCancellation = (CancellationToken)args[i];

                        // ReSharper disable once AccessToDisposedClosure
                        disposables.Add(actualCancellation.Register(() => cts.Cancel(false)));
#pragma warning restore CS8605
                    }
                }
            }

            object? result = null;
            var complete = false;

            try
            {
                while (!complete)
                {
                    var response = await binaryReader.Read<ResponseType>(cts.Token).ConfigureAwait(false);

                    switch (response)
                    {
                        case ResponseType.Exception:
                            {
                                var fullTypeName = await binaryReader.ReadString(cts.Token).ConfigureAwait(false);
                                if (fullTypeName == null)
                                {
                                    throw new InvalidOperationException("Empty type name.");
                                }

                                var exceptionType = TypeHelper.FromFullNameWithAssembly(fullTypeName);

                                privateToken.ThrowIfCancellationRequested();
                                var serializedObject = await binaryReader.ReadString(cts.Token).ConfigureAwait(false);
                                if (serializedObject == null)
                                {
                                    throw new InvalidOperationException("Missing exception.");
                                }

                                var exception = (Exception)(JsonConvert.DeserializeObject(serializedObject, exceptionType) ?? throw new InvalidOperationException("Missing exception."));
                                throw new TargetInvocationException("Remoting returned an exception", exception);
                            }

                        case ResponseType.Progress:
                            {
                                var receivedProgressPayloadType = await binaryReader.ReadString(cts.Token).ConfigureAwait(false);
                                privateToken.ThrowIfCancellationRequested();
                                var receivedProgressPayloadJson = await binaryReader.ReadString(cts.Token).ConfigureAwait(false);
                                var receivedProgressPayload = receivedProgressPayloadJson == null ? null : JsonConvert.DeserializeObject(receivedProgressPayloadJson, TypeHelper.FromFullNameWithAssembly(receivedProgressPayloadType));

                                if (findProgress.Count == 0 || receivedProgressPayload == null)
                                {
                                    break;
                                }

                                if (args?.Length > 0)
                                {
                                    foreach (var (position, progressType) in findProgress)
                                    {
                                        var progressObject = args[position];
                                        progressType
                                            .GetMethod(nameof(IProgress<object>.Report), BindingFlags.Instance | BindingFlags.Public)!
                                            .Invoke(progressObject, new[] { receivedProgressPayload });
                                    }
                                }

                                break;
                            }

                        case ResponseType.Cancelled:
                            throw new OperationCanceledException();

                        case ResponseType.Completed:
                            {
                                complete = true;

                                if (targetMethod.ReturnType == typeof(void) || targetMethod.ReturnType == typeof(Task))
                                {
                                    break;
                                }

                                var returnedValue = await binaryReader.ReadString(cts.Token).ConfigureAwait(false);
                                privateToken.ThrowIfCancellationRequested();

                                if (targetMethod.ReturnType.IsAssignableTo(typeof(Task)))
                                {
                                    var resultType = targetMethod.ReturnType.GetGenericArguments()[0];
                                    result = returnedValue == null ? null : JsonConvert.DeserializeObject(returnedValue, resultType);
                                }
                                else
                                {
                                    var resultType = targetMethod.ReturnType;
                                    result = returnedValue == null ? null : JsonConvert.DeserializeObject(returnedValue, resultType);
                                }

                                break;
                            }
                    }
                }

                if (targetMethod.ReturnType == typeof(Task))
                {
                    return null;
                }

                if (targetMethod.ReturnType.IsGenericType && targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return result;
                }

                return result;
            }
            finally
            {
                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    private class AsHighestAvailableProxy<T> : IpcProxy where T : class
    {
        private ClientHandler? _clientHandler;
        private Func<T?>? _fallbackFactory;

        public static T Create(ClientHandler clientHandler, Func<T> immediateExecutionFallback)
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException("T must be an interface.");
            }

            if (clientHandler == null)
            {
                throw new ArgumentNullException(nameof(clientHandler));
            }

            if (immediateExecutionFallback == null)
            {
                throw new ArgumentNullException(nameof(immediateExecutionFallback));
            }

            // DispatchProxy.Create creates proxy objects
            var proxy = Create<T, AsHighestAvailableProxy<T>>() as AsHighestAvailableProxy<T>;
            if (proxy == null)
            {
                throw new InvalidOperationException();
            }

            if (proxy is not T result)
            {
                throw new InvalidOperationException();
            }

            proxy._clientHandler = clientHandler;
            proxy._fallbackFactory = immediateExecutionFallback;

            return result;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                throw new ArgumentNullException(nameof(targetMethod));
            }

            if (this._fallbackFactory == null)
            {
                throw new NullReferenceException("Fallback factory was not set.");
            }

            if (this._clientHandler == null)
            {
                throw new NullReferenceException("Client handler was not set.");
            }

            var executeOnFallBack = this._clientHandler.IsRunningAsAdministratorAsync().GetAwaiter().GetResult() || !this._clientHandler.IsUacHelperRunning().GetAwaiter().GetResult();

            if (!executeOnFallBack)
            {
                return base.Invoke(targetMethod, args);
            }

            var fallBack = this._fallbackFactory();
            return targetMethod.Invoke(fallBack, args);
        }
    }

    private class AsAdministratorProxy<T> : IpcProxy where T : class
    {
        private bool _keepElevation;
        private ClientHandler? _clientHandler;

        public static T Create(ClientHandler clientHandler, bool keepElevation = true)
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException("T must be an interface.");
            }

            if (clientHandler == null)
            {
                throw new ArgumentNullException(nameof(clientHandler));
            }

            // DispatchProxy.Create creates proxy objects
            var proxy = Create<T, AsAdministratorProxy<T>>() as AsAdministratorProxy<T>;
            if (proxy == null)
            {
                throw new InvalidOperationException();
            }

            if (proxy is not T result)
            {
                throw new InvalidOperationException();
            }

            proxy._clientHandler = clientHandler;
            proxy._keepElevation = keepElevation;

            return result;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                throw new ArgumentNullException(nameof(targetMethod));
            }

            if (this._clientHandler == null)
            {
                throw new NullReferenceException("Client handler was not set.");
            }

            if (this._clientHandler.IsRunningAsAdministratorAsync().GetAwaiter().GetResult() || this._clientHandler.IsUacHelperRunning().GetAwaiter().GetResult())
            {
                return base.Invoke(targetMethod, args);
            }

            if (!this._clientHandler.StartServerAsync(this._keepElevation).GetAwaiter().GetResult())
            {
                throw new InvalidOperationException("Could not start the server.");
            }

            if (!this._clientHandler.IsUacHelperRunning().GetAwaiter().GetResult())
            {
                throw new InvalidOperationException("Could not start the server.");
            }

            return base.Invoke(targetMethod, args);
        }
    }
}