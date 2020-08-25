using System;

namespace Otor.MsixHero.Infrastructure.Logging
{
    public interface ILog
    {
        void Error(Exception exception, string message);
        void Error(Exception exception);
        void Error(string message);
        void Error(string pattern, params object[] parameters);
        void Error(Exception exception, string pattern, params object[] parameters);

        void Fatal(Exception exception, string message);
        void Fatal(Exception exception);
        void Fatal(string message);
        void Fatal(string pattern, params object[] parameters);
        void Fatal(Exception exception, string pattern, params object[] parameters);

        void Info(Exception exception, string message);
        void Info(Exception exception);
        void Info(string message);
        void Info(string pattern, params object[] parameters);
        void Info(Exception exception, string pattern, params object[] parameters);

        void Trace(Exception exception, string message);
        void Trace(Exception exception);
        void Trace(string message);
        void Trace(string pattern, params object[] parameters);
        void Trace(Exception exception, string pattern, params object[] parameters);

        void Debug(Exception exception, string message);
        void Debug(Exception exception);
        void Debug(string message);
        void Debug(string pattern, params object[] parameters);
        void Debug(Exception exception, string pattern, params object[] parameters);

        void Warn(Exception exception, string message);
        void Warn(Exception exception);
        void Warn(string message);
        void Warn(string pattern, params object[] parameters);
        void Warn(Exception exception, string pattern, params object[] parameters);
    }
}
