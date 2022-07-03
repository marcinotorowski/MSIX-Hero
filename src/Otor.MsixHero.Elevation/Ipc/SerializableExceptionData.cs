namespace Otor.MsixHero.Elevation.Ipc
{
    public class SerializableExceptionData
    {
        // ReSharper disable once UnusedMember.Global
        public SerializableExceptionData()
        {
        }

        public SerializableExceptionData(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.OriginalType = exception.GetType();
            this.Message = exception.Message;
            this.StackTrace = exception.StackTrace;
            this.InnerException = exception.InnerException == null ? null : new SerializableExceptionData(exception.InnerException);
        }

        public string? Message { get; set; }

        public Type? OriginalType { get; set; }

        public string? StackTrace { get; set; }
        
        public string? HelpLink { get; set; }

        public string? Source { get; set; }

        public SerializableExceptionData? InnerException { get; set; }
    }
}
