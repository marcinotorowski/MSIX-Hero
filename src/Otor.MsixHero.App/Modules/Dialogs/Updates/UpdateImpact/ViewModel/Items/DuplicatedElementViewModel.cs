using System.Collections.ObjectModel;
using System.IO;
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
            this.Name = GenerateHeader(file);
            this.DefaultSorting = file.Files.Count;
            
            if (file.Files?.Any() == true)
            {
                this.Children = new ObservableCollection<DuplicatedElementViewModel>(file.Files.Select(f => new DuplicatedElementViewModel(f)));
            }
        }

        private static string GenerateHeader(ComparedDuplicate file)
        {
            if (file.Files.Count == 1)
            {
                return file.Files.First().Name;
            }

            if (file.Files.Any())
            {
                var fileNames = file.Files.Select(f => Path.GetFileNameWithoutExtension(f.Name)).Where(f => f != null).ToArray();
                var extensions = file.Files.Select(f => Path.GetExtension(f.Name)).Where(f => f != null).ToArray();

                var commonFileNamePrefix = new string(
                    fileNames.First().Substring(0, fileNames.Min(s => s.Length))
                        .TakeWhile((c, i) => fileNames.All(s => char.ToLowerInvariant(s[i]) == char.ToLowerInvariant(c))).ToArray());

                var commonFileExtensionPrefix = new string(
                    extensions.First().Substring(0, extensions.Min(s => s.Length))
                        .TakeWhile((c, i) => extensions.All(s => char.ToLowerInvariant(s[i]) == char.ToLowerInvariant(c))).ToArray());

                if (commonFileNamePrefix.Length < fileNames.Max(f => f.Length))
                {
                    commonFileNamePrefix += "*";
                }

                if (commonFileExtensionPrefix.Length < extensions.Max(f => f.Length))
                {
                    commonFileExtensionPrefix += "*";
                }

                if (commonFileNamePrefix.Length > 5 || commonFileNamePrefix.LastOrDefault() != '*')
                {
                    return $"{commonFileNamePrefix}{commonFileExtensionPrefix} ({file.Files.Count} files)";
                }

                if (file.Files.Count > 2)
                {
                    return $"{Path.GetFileName(file.Files[0].Name)} (+{file.Files.Count - 1} other files)";
                }

                return $"{Path.GetFileName(file.Files[0].Name)} (+1 another file)";
            }

            return file.Files.Count + " files";
        }
        
        public string Name { get; }
        
        public int DefaultSorting { get; }
        
        public long UpdateImpact { get; }
        
        public long PossibleSizeReduction { get; }

        public ObservableCollection<DuplicatedElementViewModel> Children { get; }
    }
}
