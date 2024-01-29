// MIT License
// 
// Copyright (C) 2024 Marcin Otorowski
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

using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Elevation.Ipc.Helpers;

namespace Otor.MsixHero.Elevation.Ipc
{
    public class IpcStreamHelper
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        private readonly Stream _stream;

        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(true);

        public IpcStreamHelper(Stream stream)
        {
            this._stream = stream;
        }

        public Stream GetUnderlyingStream() => this._stream;

        /// <summary>
        /// Reads the object using asynchronous methods.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A hot running task that returns the read object.
        /// </returns>
        public async Task<string?> ReadString(CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();
                using var binaryReader = new AsyncBinaryReader(this._stream, true);
                return await ReadStringFromBinaryReader(binaryReader, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }
        }
        
        public async Task WriteComplete<T>(T result, CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();

                using var binaryWriter = new AsyncBinaryWriter(this._stream, true);

                var jsonString = Serializer.Serialize(ResponseType.Completed);
                await WriteStringToBinaryWriter(binaryWriter, jsonString, cancellationToken).ConfigureAwait(false);

                jsonString = Serializer.Serialize(result);
                await WriteStringToBinaryWriter(binaryWriter, jsonString, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }
        }

        public async Task WriteComplete(CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();

                using var binaryWriter = new AsyncBinaryWriter(this._stream, true);

                var jsonString = Serializer.Serialize(ResponseType.Completed);
                await WriteStringToBinaryWriter(binaryWriter, jsonString, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }
        }

        public async Task WriteException(Exception e, CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();

                using var binaryWriter = new AsyncBinaryWriter(this._stream, true);

                var jsonString = Serializer.Serialize(ResponseType.Exception);
                await WriteStringToBinaryWriter(binaryWriter, jsonString, cancellationToken);

                await WriteStringToBinaryWriter(binaryWriter, TypeHelper.ToFullNameWithAssembly(typeof(SerializableExceptionData)), cancellationToken).ConfigureAwait(false);

                jsonString = Serializer.Serialize(new SerializableExceptionData(e));
                await WriteStringToBinaryWriter(binaryWriter, jsonString, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }
        }

        public async Task WriteProgress<T>(T progress, CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();

                using var binaryWriter = new AsyncBinaryWriter(this._stream, true);

                var jsonString = Serializer.Serialize(ResponseType.Progress);
                await WriteStringToBinaryWriter(binaryWriter, jsonString, cancellationToken).ConfigureAwait(false);

                await WriteStringToBinaryWriter(binaryWriter, TypeHelper.ToFullNameWithAssembly(typeof(T)), cancellationToken).ConfigureAwait(false);

                var json = Serializer.Serialize(progress);
                await WriteStringToBinaryWriter(binaryWriter, json, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }

        }

        public async Task WriteProgress(Type progressType, object progress, CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();

                using var binaryWriter = new AsyncBinaryWriter(this._stream, true);

                var jsonString = Serializer.Serialize(ResponseType.Progress);
                await WriteStringToBinaryWriter(binaryWriter, jsonString, cancellationToken).ConfigureAwait(false);

                await WriteStringToBinaryWriter(binaryWriter, TypeHelper.ToFullNameWithAssembly(progressType), cancellationToken).ConfigureAwait(false);

                var json = Serializer.Serialize(progress);
                await WriteStringToBinaryWriter(binaryWriter, json, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }

        }

        public async Task Write(Method signatureAndPayload, CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();

                var argumentsArray = new JArray();
                var json = new JObject
                {
                    ["type"] = TypeHelper.ToFullNameWithAssembly(signatureAndPayload.MethodInfo?.DeclaringType ?? throw new ArgumentNullException(nameof(signatureAndPayload))),
                    ["returns"] = TypeHelper.ToFullNameWithAssembly(signatureAndPayload.MethodInfo.ReturnType),
                    ["name"] = signatureAndPayload.MethodInfo.Name,
                    ["arguments"] = argumentsArray
                };

                var methodParameters = signatureAndPayload.MethodInfo.GetParameters();
                var passedParameters = signatureAndPayload.Parameters ?? Array.Empty<object>();

                for (var index = 0; index < methodParameters.Length; index++)
                {
                    var arg = methodParameters[index];
                    var jsonArg = new JObject();
                    
                    if (TypeHelper.IsCancellation(arg.ParameterType))
                    {
                        jsonArg["type"] = "!CancellationToken";
                    }
                    else if (TypeHelper.IsProgress(arg.ParameterType, out var _))
                    {
                        jsonArg["type"] = "!IProgress";
                    }
                    else
                    {
                        jsonArg["type"] = TypeHelper.ToFullNameWithAssembly(arg.ParameterType);
                        jsonArg["value"] = Serializer.Serialize(passedParameters[index]);
                    }

                    argumentsArray.Add(jsonArg);
                }
                
                using var binaryWriter = new AsyncBinaryWriter(this._stream, true);
                await WriteStringToBinaryWriter(binaryWriter, json.ToString(), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }
        }

        public async Task<Method> ReadMethodInfo(CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();

                using var binaryReader = new AsyncBinaryReader(this._stream, true);

                var str = await ReadStringFromBinaryReader(binaryReader, cancellationToken).ConfigureAwait(false);
                
                var json = JObject.Parse(str ?? throw new InvalidOperationException("Unexpected null value."));
                var type = json["type"]?.Value<string>();
                var returns = json["returns"]?.Value<string>();
                var name = json["name"]?.Value<string>();
                var arguments = json["arguments"]?.ToArray();

                var actualType = TypeHelper.FromFullNameWithAssembly(type);
                var actualReturns = TypeHelper.FromFullNameWithAssembly(returns);

                foreach (var method in actualType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var methodReturns = method.ReturnType;
                    if (methodReturns != actualReturns)
                    {
                        continue;
                    }

                    if (method.Name != name)
                    {
                        continue;
                    }

                    var methodParameters = method.GetParameters();
                    var allMatch = true;

                    if (arguments?.Length == 0)
                    {
                        if (methodParameters.Length == 0)
                        {
                            return new Method(method, Array.Empty<object>());
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (methodParameters.Length != (arguments?.Length ?? 0))
                    {
                        continue;
                    }

                    var objects = new List<object?>();
                    for (var j = 0; j < methodParameters.Length; j++)
                    {
                        var argumentDefinition = arguments![j];

                        var argumentDefinitionType = argumentDefinition["type"]?.Value<string>() ?? throw new InvalidOperationException("Missing <type> property.");

                        switch (argumentDefinitionType)
                        {
                            case "!CancellationToken":
                                if (!TypeHelper.IsCancellation(methodParameters[j].ParameterType))
                                {
                                    allMatch = false;
                                }

                                objects.Add(null); // placeholder, this will be replaced with a proxy cancellation token.

                                break;
                            case "!IProgress":
                                if (!TypeHelper.IsProgress(methodParameters[j].ParameterType, out _))
                                {
                                    allMatch = false;
                                }

                                objects.Add(null); // placeholder, this will be replaced with a proxy progress report.
                                break;

                            default:
                                var actualArgumentType = TypeHelper.FromFullNameWithAssembly(argumentDefinitionType);
                                if (actualArgumentType != methodParameters[j].ParameterType)
                                {
                                    allMatch = false;
                                    break;
                                }
                                
                                objects.Add(Serializer.Deserialize(actualArgumentType, argumentDefinition["value"]?.Value<string>()));
                                break;
                        }

                        if (!allMatch)
                        {
                            break;
                        }
                    }

                    if (allMatch)
                    {
                        return new Method(method, objects.ToArray());
                    }
                }

                throw new MissingMethodException();
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }
        }


        public async Task<T?> Read<T>(CancellationToken cancellationToken = default)
        {
            var gotHandle = false;
            try
            {
                gotHandle = this._autoResetEvent.WaitOne();

                using var binaryReader = new AsyncBinaryReader(this._stream, true);
                var msgLength = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
                if (msgLength == 0)
                {
                    return default;
                }

                var jsonString = await binaryReader.ReadStringAsync(msgLength, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                return Serializer.Deserialize<T>(jsonString);
            }
            finally
            {
                if (gotHandle)
                {
                    this._autoResetEvent.Set();
                }
            }
        }

        public class Method
        {
            // ReSharper disable once UnusedMember.Global
            public Method()
            {
            }

            public Method(MethodInfo methodInfo, object?[]? parameters)
            {
                this.MethodInfo = methodInfo;
                this.Parameters = parameters;
            }

            public MethodInfo? MethodInfo { get; set; }

            public object?[]? Parameters { get; set; }
        }

        private static Task WriteStringToBinaryWriter(AsyncBinaryWriter binaryWriter, string? stringToWrite, CancellationToken cancellationToken = default)
        {
            return WriteStringToBinaryWriter(binaryWriter, stringToWrite, Encoding.UTF8, cancellationToken);
        }

        private static Task<string?> ReadStringFromBinaryReader(AsyncBinaryReader binaryReader, CancellationToken cancellationToken = default)
        {
            return ReadStringFromBinaryReader(binaryReader, Encoding.UTF8, cancellationToken);
        }

        private static async Task<string?> ReadStringFromBinaryReader(AsyncBinaryReader binaryReader, Encoding encoding, CancellationToken cancellationToken = default)
        {
            var msgLength = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
            if (msgLength == 0)
            {
                return null;
            }

            var bytes = await binaryReader.ReadBytesAsync(msgLength, cancellationToken).ConfigureAwait(false);
            return encoding.GetString(bytes);
        }

        private static async Task WriteStringToBinaryWriter(AsyncBinaryWriter binaryWriter, string? stringToWrite, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (stringToWrite == null)
            {
                await binaryWriter.WriteAsync(0, cancellationToken).ConfigureAwait(false);
                return;
            }

            var jsonBytes = encoding.GetBytes(stringToWrite);
            await binaryWriter.WriteAsync(jsonBytes.Length, cancellationToken).ConfigureAwait(false);
            await binaryWriter.WriteAsync(jsonBytes, cancellationToken).ConfigureAwait(false);
        }
    }
}
