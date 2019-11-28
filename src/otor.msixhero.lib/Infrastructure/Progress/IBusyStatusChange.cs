namespace otor.msixhero.lib.Infrastructure.Progress
{
    public interface IBusyStatusChange
    {
        bool IsBusy { get; }

        string Message { get; }

        int Progress { get; }
    }
}