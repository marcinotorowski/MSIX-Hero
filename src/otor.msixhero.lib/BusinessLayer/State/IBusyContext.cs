namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IBusyContext
    {
        string Message { get; set; }

        int Progress { get; set; }
    }
}