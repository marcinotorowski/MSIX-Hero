namespace otor.msixhero.lib.Infrastructure.Logging
{
    public enum MsixHeroLogLevel
    {
        /// <summary>
        /// Very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
        /// </summary>
        Trace,

        /// <summary>
        /// Debugging information, less detailed than trace, typically not enabled in production environment.
        /// </summary>
        Debug,

        /// <summary>
        /// Information messages, which are normally enabled in production environment.
        /// </summary>
        Info,

        /// <summary>
        /// Warning messages, typically for non-critical issues, which can be recovered or which are temporary failures.
        /// </summary>
        Warn,

        /// <summary>
        /// Error messages - most of the time these are Exceptions.
        /// </summary>
        Error,

        /// <summary>
        /// Very serious errors!
        /// </summary>
        Fatal
    }
}