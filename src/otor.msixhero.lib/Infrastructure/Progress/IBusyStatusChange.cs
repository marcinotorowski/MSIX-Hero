namespace otor.msixhero.lib.Infrastructure.Progress
{
    public enum OperationType
    {
        PackageLoading,
        Other
    }

    public interface IBusyStatusChange
    {
        OperationType Type { get; }

        bool IsBusy { get; }

        string Message { get; }

        int Progress { get; }
    }
}