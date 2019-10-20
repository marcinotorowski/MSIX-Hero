namespace MSI_Hero.Services
{
    public interface IBusyStatusChange
    {
        bool IsBusy { get; }

        string Message { get; }

        int Progress { get; }
    }
}