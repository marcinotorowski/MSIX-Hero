using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Otor.MsixHero.App.Helpers;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content.Files
{
    public class AppxFilesViewModel : TreeViewModel<AppxDirectoryViewModel, AppxFileViewModel>
    {
        public AppxFilesViewModel(string packageFile) : base(packageFile)
        {
            var rootContainersTask = this.GetRootContainers();
            var nodesCollection = new ObservableCollection<AppxFileViewModel>();
            this.Nodes = nodesCollection;

            var containers = new AsyncProperty<IList<AppxDirectoryViewModel>>();
            this.Containers = containers;
#pragma warning disable 4014
            containers.Loaded += this.OnContainersLoaded;
            containers.Load(rootContainersTask);
#pragma warning restore 4014
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
    }
}
