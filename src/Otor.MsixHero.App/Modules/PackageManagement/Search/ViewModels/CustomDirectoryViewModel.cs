using System.IO;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels
{
    public class CustomDirectoryViewModel(PackageDirectoryConfiguration configEntry) : NotifyPropertyChanged
    {
        public string DisplayName { get; } = Path.GetFileName(configEntry.Path);

        public string FullPath { get; } = configEntry.Path;

        public bool IsRecurse { get; } = configEntry.IsRecurse;
    }
}
