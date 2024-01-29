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

namespace Otor.MsixHero.Elevation.Ipc
{
    internal class TypeHelper
    {
        public static string ToFullNameWithAssembly(Type type)
        {
            return type.AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(type));
        }

        public static Type FromFullNameWithAssembly(string? type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Type.GetType(type, true) ?? throw new ArgumentException($"Could not find type '{type}'.");
        }

        public static bool IsCancellation(Type type)
        {
            return type == typeof(CancellationToken);
        }

        public static bool IsProgress(Type? type, out Type? progressType)
        {
            if (type == default)
            {
                progressType = default;
                return false;
            }

            if (!type.IsGenericType)
            {
                progressType = default;
                return false;
            }

            var args = type.GetGenericArguments();
            if (args.Length != 1)
            {
                progressType = default;
                return false;
            }

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(IProgress<>))
            {
                progressType = args[0];
                return true;
            }

            if (genericTypeDefinition.IsAssignableTo(typeof(IProgress<>)))
            {
                progressType = args[0];
                return true;
            }

            progressType = default;
            return false;
        }
    }
}
