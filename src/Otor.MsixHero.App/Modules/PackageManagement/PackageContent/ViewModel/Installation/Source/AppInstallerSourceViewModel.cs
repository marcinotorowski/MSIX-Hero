using System;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation.Source
{
    public class AppInstallerSourceViewModel : PackageSourceViewModel
    {
        public AppInstallerSourceViewModel(string src)
        {
            this.AppInstallerUri = src;
        }

        public AppInstallerSourceViewModel(Uri src) : this(src.ToString())
        {
        }

        public string AppInstallerUri { get; }
    }
}
