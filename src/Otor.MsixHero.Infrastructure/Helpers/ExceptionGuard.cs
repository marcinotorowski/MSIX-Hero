using System;
using System.Runtime.CompilerServices;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    public static class ExceptionGuard
    {
        public static void Guard(Action lambda)
        {
            Guard<Exception>(lambda);
        }
        public static void Guard<T>(Func<T> lambda)
        {
            Guard<Exception, T>(lambda);
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
