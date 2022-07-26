using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Helpers;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files
{
    public class PackageFilesViewModel : NotifyPropertyChanged, IPackageContentItem, ILoadPackage
    {
        private bool _isActive;
        private string _pendingFile;
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
        
        public PackageContentViewType Type => PackageContentViewType.Files;

        public bool IsActive
        {
            get => this._isActive;
            set
            {
                if (this.SetField(ref this._isActive, value) && value && this._pendingFile != null)
                {
                    this.LoadFiles(CancellationToken.None);
                }
            }
        }

        public Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this._pendingFile = filePath;
            if (!this.IsActive)
            {
                return Task.CompletedTask;
            }

            this.LoadFiles(cancellationToken);
            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }

        private void LoadFiles(CancellationToken cancellationToken)
        {
            if (this._pendingFile == null)
            {
                return;
            }

            this.FileTree = new FileTreeViewModel(this._pendingFile, this._fileViewer, this._fileInvoker);
            this._pendingFile = null;
        }

        public FileTreeViewModel FileTree { get; private set; }

        public ICommand GoBack { get; }
    }
}
