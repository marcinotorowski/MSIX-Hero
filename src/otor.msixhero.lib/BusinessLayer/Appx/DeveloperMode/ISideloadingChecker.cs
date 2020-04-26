namespace otor.msixhero.lib.BusinessLayer.Appx.DeveloperMode
{
    public interface ISideloadingChecker
    {
        SideloadingStatus GetStatus();

        bool SetStatus(SideloadingStatus status);
    }
}
