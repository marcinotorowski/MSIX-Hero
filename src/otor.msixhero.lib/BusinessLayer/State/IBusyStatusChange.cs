namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IBusyStatusChange
    {
        bool IsBusy { get; }

        string Message { get; }

        int Progress { get; }
    }
}