using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.Appx.Reader.File.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files.Items
{
    public class AppxFileViewModel : TreeNodeViewModel
    {
        public AppxFileViewModel(AppxFileInfo fileInfo)
        {
            this.Path = fileInfo.FullPath;
            this.Name = fileInfo.Name;
            this.Size = fileInfo.Size;
        }
        
        public long Size { get; }
    }
}
