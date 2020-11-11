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
