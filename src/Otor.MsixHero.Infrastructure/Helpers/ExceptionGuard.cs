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
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    public static partial class ExceptionGuard
    {
        public static void Guard(Action lambda)
        {
            Guard<Exception>(lambda);
        }

        public static T Guard<T>(Func<T> lambda)
        {
            return Guard<Exception, T>(lambda);
        }

        public static T Guard<T>(Func<Task<T>> taskDelegate)
        {
            return Guard<Exception, T>(() => taskDelegate().GetAwaiter().GetResult());
        }

        public static void Guard(Func<Task> taskDelegate)
        {
            Guard<Exception>(() => taskDelegate().GetAwaiter().GetResult());
        }

        public static Task Guard(Task taskToGuard)
        {
            return Guard<Exception>(taskToGuard);
        }

        public static Task<T> Guard<T>(Task<T> taskToGuard)
        {
            return Guard<Exception, T>(taskToGuard);
        }

        public static async Task<TResult> Guard<TException, TResult>(Task<TResult> taskToGuard) where TException : Exception
        {
            try
            {
                return await taskToGuard.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is TException)
                {
                    return default;
                }

                throw;
            }
        }

        public static async Task Guard<TException>(Task taskToGuard) where TException : Exception
        {
            try
            {
                await taskToGuard.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is TException)
                {
                    return;
                }

                throw;
            }
        }

        public static void Guard<TException>(Action lambda) where TException : Exception
        {
            try
            {
                lambda();
            }
            catch (Exception e)
            {
                if (e is TException)
                {
                    return;
                }

                throw;
            }
        }
        
        public static TValue Guard<TException, TValue>(Func<TValue> lambda) where TException : Exception
        {
            try
            {
                return lambda();
            }
            catch (Exception e)
            {
                if (e is TException)
                {
                    return default;
                }

                throw;
            }
        }
    }
}
