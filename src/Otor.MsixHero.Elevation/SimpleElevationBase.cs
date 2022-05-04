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

using System.Reflection;
using System.Text;
using Otor.MsixHero.Elevation.Ipc;
using Prism.Ioc;

namespace Otor.MsixHero.Elevation;

public abstract class SimpleElevationBase
{
    private readonly IDictionary<Type, Lazy<object>> _handlers = new Dictionary<Type, Lazy<object>>();
    
    public static string PipeName { get; set; } = "Otor.SimpleElevation";

    public void RegisterProxy<TInterface, TClass>(TClass singleInstance) where TClass : TInterface
    {
        if (singleInstance == null)
        {
            throw new ArgumentNullException(nameof(singleInstance));
        }

        if (!typeof(TInterface).IsInterface)
        {
            throw new ArgumentException("TInterface must be an interface.");
        }

        var lazyObject = new Lazy<object>(() => singleInstance);
        this._handlers[typeof(TInterface)] = lazyObject;
        this._handlers[typeof(TClass)] = lazyObject;
    }

    public void RegisterProxy<TInterface, TClass>(Func<TClass> factoryMethod) where TClass : TInterface
    {
        if (factoryMethod == null)
        {
            throw new ArgumentNullException(nameof(factoryMethod));
        }

        if (!typeof(TInterface).IsInterface)
        {
            throw new ArgumentException("TInterface must be an interface.");
        }

        var lazyObject = new Lazy<object>(() => factoryMethod()!);

        this._handlers[typeof(TInterface)] = lazyObject;
        this._handlers[typeof(TClass)] = lazyObject;
    }

    public void RegisterProxy<TInterface, TClass>() where TClass : TInterface
    {
        if (!typeof(TInterface).IsInterface)
        {
            throw new ArgumentException("TInterface must be an interface.");
        }

        var lazyObject = new Lazy<object>(() => Activator.CreateInstance<TClass>()!);

        this._handlers[typeof(TInterface)] = lazyObject;
        this._handlers[typeof(TClass)] = lazyObject;
    }

    public void RegisterProxy<TInterface>(IContainerProvider containerProvider)
    {
        if (!typeof(TInterface).IsInterface)
        {
            throw new ArgumentException("TInterface must be an interface.");
        }

        var lazyObject = new Lazy<object>(() => containerProvider.Resolve(typeof(TInterface)));
        this._handlers[typeof(TInterface)] = lazyObject;
    }

    protected static string GetMethodLogInfo(MethodInfo targetMethod, IList<object?>? args)
    {
        if (targetMethod == null)
        {
            throw new ArgumentNullException(nameof(targetMethod));
        }

        if (targetMethod.DeclaringType == null)
        {
            throw new ArgumentException("Missing declaring type in the target method.", nameof(targetMethod));
        }

        var argsFormat = new StringBuilder();
        var allParams = targetMethod.GetParameters().ToArray();

        if (args?.Any() != true)
        {
            argsFormat.Append("<none>");
        }
        else
        {
            for (var i = 0; i < allParams.Length; i++)
            {
                if (argsFormat.Length != 0)
                {
                    argsFormat.Append(", ");
                }

                if (args[i] == null)
                {
                    if (TypeHelper.IsProgress(allParams[i].ParameterType, out var _))
                    {
                        argsFormat.Append("<progress>");
                    }
                    else if (TypeHelper.IsCancellation(allParams[i].ParameterType))
                    {
                        argsFormat.Append("<cancellation>");
                    }
                    else
                    {
                        argsFormat.Append("<null>");
                    }
                }
                else
                {
                    argsFormat.Append(args[i]);
                }
            }
        }

        return $"Executing {targetMethod.DeclaringType.Name}.{targetMethod.Name} with parameters ({argsFormat})...";
    }

    protected T Resolve<T>()
    {
        if (!this._handlers.TryGetValue(typeof(T), out var actualObjectFactory))
        {
            throw new KeyNotFoundException($"The interface {TypeHelper.ToFullNameWithAssembly(typeof(T))} is not supported.");
        }

        return (T)actualObjectFactory.Value;
    }

    protected object Resolve(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (!this._handlers.TryGetValue(type, out var actualObjectFactory))
        {
            throw new KeyNotFoundException($"The interface {TypeHelper.ToFullNameWithAssembly(type)} is not supported.");
        }

        return actualObjectFactory.Value;
    }
}