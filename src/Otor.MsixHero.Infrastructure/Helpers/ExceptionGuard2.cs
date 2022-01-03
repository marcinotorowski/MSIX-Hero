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
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    public static partial class ExceptionGuard
    {
        public static void Guard(Action lambda, IInteractionService interactionService)
        {
            Guard<Exception>(lambda, interactionService);
        }

        public static void Guard<T>(Func<T> lambda, IInteractionService interactionService)
        {
            Guard<Exception, T>(lambda, interactionService);
        }
        
        public static void Guard<TException>(Action lambda, IInteractionService interactionService) where TException : Exception
        {
            try
            {
                lambda();
            }
            catch (Exception e)
            {
                if (e is TException)
                {
                    interactionService.ShowError(e.Message, e);
                    return;
                }

                throw;
            }
        }
        
        public static TValue Guard<TException, TValue>(Func<TValue> lambda, IInteractionService interactionService) where TException : Exception
        {
            try
            {
                return lambda();
            }
            catch (Exception e)
            {
                if (e is TException)
                {
                    interactionService.ShowError(e.Message, e);
                    return default;
                }

                throw;
            }
        }
    }
}
