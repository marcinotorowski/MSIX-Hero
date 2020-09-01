namespace Otor.MsixHero.Lib.Infrastructure.Progress
{
    public class BusyStatusChange : IBusyStatusChange
    {
        public BusyStatusChange(OperationType type, bool isBusy, string message, int progress)
        {
            this.Type = type;
            this.IsBusy = isBusy;
            this.Message = message;
            this.Progress = progress;
        }

        public OperationType Type { get; }

        public bool IsBusy { get; private set; }

        public string Message { get; private set; }

        public int Progress { get; private set; }
    }
}