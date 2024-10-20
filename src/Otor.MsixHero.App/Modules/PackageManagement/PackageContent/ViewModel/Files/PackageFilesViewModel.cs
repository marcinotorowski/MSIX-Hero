using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files.Items;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Otor.MsixHero.Infrastructure.Helpers;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files
{
    public class PackageFilesViewModel : PackageLazyLoadingViewModel
    {
        private readonly IAppxFileViewer _fileViewer;
        private readonly FileInvoker _fileInvoker;
        private bool _isVirtualizationDisabled;

        public PackageFilesViewModel(IPackageContentItemNavigation navigation, IAppxFileViewer fileViewer, FileInvoker fileInvoker)
        {
            this._fileViewer = fileViewer;
            this._fileInvoker = fileInvoker;
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }

        public bool IsVirtualizationDisabled
        {
            get => this._isVirtualizationDisabled;
            private set => this.SetField(ref this._isVirtualizationDisabled, value);
        }

        public override PackageContentViewType Type => PackageContentViewType.Files;

        public FileTreeViewModel FileTree { get; private set; }

        public ICommand GoBack { get; }

        protected override Task DoLoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
        {
            this.IsVirtualizationDisabled = !model.FileVirtualizationEnabled;
            this.FileTree = new FileTreeViewModel(filePath, this._fileViewer, this._fileInvoker);
            this.OnPropertyChanged(nameof(this.FileTree));
            return Task.CompletedTask;
        }
    }
}
