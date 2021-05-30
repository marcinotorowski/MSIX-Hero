using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Helpers;
using Prism.Commands;
using ICommand = System.Windows.Input.ICommand;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content.Files
{
    public class AppxFilesViewModel : TreeViewModel<AppxDirectoryViewModel, AppxFileViewModel>
    {
        private readonly IAppxFileViewer fileViewer;
        private readonly FileInvoker fileInvoker;

        public AppxFilesViewModel(
            string packageFile, 
            IAppxFileViewer fileViewer, 
            FileInvoker fileInvoker) : base(packageFile)
        {
            this.fileViewer = fileViewer;
            this.fileInvoker = fileInvoker;
            var rootContainersTask = this.GetRootContainers();
            var nodesCollection = new ObservableCollection<AppxFileViewModel>();
            this.Nodes = nodesCollection;
            this.View = new DelegateCommand(this.OnView);
            
            var containers = new AsyncProperty<IList<AppxDirectoryViewModel>>();
            this.Containers = containers;
#pragma warning disable 4014
            containers.Loaded += this.OnContainersLoaded;
            containers.Load(rootContainersTask);
#pragma warning restore 4014
        }

        private async void OnView()
        {
            var selectedFile = this.SelectedNode;
            if (selectedFile == null)
            {
                return;
            }

            using var reader = FileReaderFactory.CreateFileReader(this.PackageFile);
            var path = await this.fileViewer.GetPath(reader, selectedFile.Path).ConfigureAwait(false);

            ExceptionGuard.Guard(() => { this.fileInvoker.Execute(path, true); });
        }

        private void OnContainersLoaded(object sender, EventArgs e)
        {
            this.SelectedContainer = this.Containers.CurrentValue.FirstOrDefault();
        }

        public override bool IsAvailable => true;

        public override ObservableCollection<AppxFileViewModel> Nodes { get; }

        public override AsyncProperty<IList<AppxDirectoryViewModel>> Containers { get; }
        
        public async Task<IList<AppxDirectoryViewModel>> GetRootContainers()
        {
            var roots = new List<AppxDirectoryViewModel>();
            using var appxReader = this.GetAppxReader();
            var hasChildren = await appxReader.EnumerateDirectories().AnyAsync().ConfigureAwait(false);
            roots.Add(new AppxDirectoryViewModel(this, null, hasChildren));
            return roots;
        }

        public ICommand View { get; }
    }
}
