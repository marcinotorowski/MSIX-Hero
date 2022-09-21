using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files.Items;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Helpers;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files
{
    public class PackageFilesViewModel : PackageLazyLoadingViewModel
    {
        private readonly IAppxFileViewer _fileViewer;
        private readonly FileInvoker _fileInvoker;

        public PackageFilesViewModel(IPackageContentItemNavigation navigation, IAppxFileViewer fileViewer, FileInvoker fileInvoker)
        {
            this._fileViewer = fileViewer;
            this._fileInvoker = fileInvoker;
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }
        
        public override PackageContentViewType Type => PackageContentViewType.Files;

        public FileTreeViewModel FileTree { get; private set; }

        public ICommand GoBack { get; }

        protected override Task DoLoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
        {
            this.FileTree = new FileTreeViewModel(filePath, this._fileViewer, this._fileInvoker);
            this.OnPropertyChanged(nameof(this.FileTree));
            return Task.CompletedTask;
        }
    }
}
