namespace Otor.MsixHero.Lib.Infrastructure.Progress
{
    public interface IBusyStatusChange
    {
        OperationType Type { get; }

        bool IsBusy { get; }

        string Message { get; }

        int Progress { get; }
    }
}