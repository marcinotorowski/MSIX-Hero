using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.ManifestCreator;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.ViewModel
{
    public class PrePackOptionsViewModel : NotifyPropertyChanged
    {
        private readonly AppxManifestCreatorAdviser source;
        private string version;

        private bool createLogo;
        private RegistryCandidateViewModel selectedRegistry;

        public PrePackOptionsViewModel(AppxManifestCreatorAdviser source)
        {
            this.source = source;
            
            this.RegistryCandidates = source.RegistryFiles == null ? new ObservableCollection<RegistryCandidateViewModel>() : new ObservableCollection<RegistryCandidateViewModel>(source.RegistryFiles.Select(r => new RegistryCandidateViewModel(r.FullName)));
            this.RegistryCandidates.Insert(0, new RegistryCandidateViewModel()); // no registry conversion
            this.selectedRegistry = this.RegistryCandidates.Skip(1).FirstOrDefault();

            this.EntryPoints = source.EntryPoints == null ? new ObservableCollection<SelectableItem<string>>() : new ObservableCollection<SelectableItem<string>>(source.EntryPoints.Select(e => new SelectableItem<string>(e,  e)));

            foreach (var item in this.EntryPoints.Take(1))
            {
                item.IsChecked = true;
            }

            this.createLogo = this.CanCreateLogo;
        }

        public bool CanConvert => this.source.Manifest == null && this.EntryPoints.Any();

        public bool ConversionNotPossible => this.source.Manifest == null && !this.EntryPoints.Any();

        public FileInfo ManifestFile => this.source.Manifest;

        public bool ManifestPresent => this.source.Manifest?.Exists ?? false;
        
        public ObservableCollection<RegistryCandidateViewModel> RegistryCandidates { get; }

        public RegistryCandidateViewModel SelectedRegistry
        {
            get => this.selectedRegistry;
            set => this.SetField(ref this.selectedRegistry, value);
        }

        public ObservableCollection<SelectableItem<string>> EntryPoints { get; }

        public bool CanConvertRegistry => this.RegistryCandidates.Count > 1;

        public bool CanCreateLogo => this.source.Logo?.Exists != true;

        public bool CreateLogo
        {
            get => this.createLogo;
            set => this.SetField(ref this.createLogo, value);
        }

        public string Version
        {
            get => this.version;
            set => this.SetField(ref this.version, value);
        }
    }
}
