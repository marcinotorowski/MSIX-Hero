using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;

namespace Otor.MsixHero.Appx.Diagnostic.Developer
{
    public interface ISideloadingChecker
    {
        SideloadingStatus GetStatus();

        bool SetStatus(SideloadingStatus status);

        WindowsStoreAutoDownload GetStoreAutoDownloadStatus();

        bool SetStoreAutoDownloadStatus(WindowsStoreAutoDownload status);
    }
}
