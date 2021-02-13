using System.Collections.ObjectModel;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items
{
    public class DuplicatedElementViewModel : NotifyPropertyChanged
    {
        public DuplicatedElementViewModel(ComparedDuplicateFile file)
        {
            this.PossibleSizeReduction = file.PossibleSizeReduction;
            this.UpdateImpact = file.PossibleImpactReduction;
            this.Name = file.Name;
        }

        public DuplicatedElementViewModel(ComparedDuplicate file)
        {
            this.PossibleSizeReduction = file.PossibleSizeReduction;
            this.UpdateImpact = file.PossibleImpactReduction;
            this.Name = file.Files.Count + " files";
            this.DefaultSorting = file.Files.Count;
            
            if (file.Files?.Any() == true)
            {
                this.Children = new ObservableCollection<DuplicatedElementViewModel>(file.Files.Select(f => new DuplicatedElementViewModel(f)));
            }
        }
        
        public string Name { get; }
        
        public int DefaultSorting { get; }
        
        public long UpdateImpact { get; }
        
        public long PossibleSizeReduction { get; }

        public ObservableCollection<DuplicatedElementViewModel> Children { get; }
    }
}
