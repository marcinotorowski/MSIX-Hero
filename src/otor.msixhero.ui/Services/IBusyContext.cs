namespace MSI_Hero.Services
{
    public interface IBusyContext
    {
        string Message { get; set; }

        int Progress { get; set; }
    }
}