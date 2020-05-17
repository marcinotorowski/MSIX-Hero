using System;
using System.IO;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PackageExpert.ViewModel
{
    public class PackageExpertSelection
    {
        public PackageExpertSelection(NavigationParameters dialogParameters)
        {
            if (dialogParameters.TryGetValue("Source", out IAppxFileReader parsed))
            {
                this.Source = parsed;
            }
        }
        public PackageExpertSelection(IDialogParameters dialogParameters)
        {
            if (dialogParameters.TryGetValue("Source", out IAppxFileReader parsed))
            {
                this.Source = parsed;
            }
        }

        public PackageExpertSelection(IAppxFileReader source)
        {
            this.Source = source;
        }

        public PackageExpertSelection(string source)
        {
            if (string.Equals(Path.GetFileName(source), "AppxManifest.xml", StringComparison.OrdinalIgnoreCase))
            {
                this.Source = new FileInfoFileReaderAdapter(source);
            }
            else
            {
                this.Source = new ZipArchiveFileReaderAdapter(source);
            }
        }

        public IAppxFileReader Source { get; set; }

        public IDialogParameters ToDialogParameters()
        {
            return new DialogParameters
            {
                {"Source", this.Source}
            };
        }

        public NavigationParameters ToNavigationParameters()
        {
            return new NavigationParameters()
            {
                {"Source", this.Source}
            };
        }
    }
}
