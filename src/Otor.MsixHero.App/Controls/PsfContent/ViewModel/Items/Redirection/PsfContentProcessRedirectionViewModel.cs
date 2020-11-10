using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.App.Changeable;
using Otor.MsixHero.Appx.Psf.Entities;

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Redirection
{
    public class PsfContentProcessRedirectionViewModel : PsfContentProcessViewModel
    {
        public PsfContentProcessRedirectionViewModel(string processRegularExpression, string fixupName, PsfRedirectedPathConfig redirectionPsfFixup) : base(processRegularExpression, fixupName)
        {
            this.Rules = new ChangeableCollection<PsfContentFolderRedirectionViewModel>(this.SetRules(redirectionPsfFixup));
            this.Rules.Commit();

            this.AddChildren(this.Rules);
        }

        public ChangeableCollection<PsfContentFolderRedirectionViewModel> Rules { get; }

        private IEnumerable<PsfContentFolderRedirectionViewModel> SetRules(PsfRedirectedPathConfig redirectionPsfFixup)
        {
            if (redirectionPsfFixup == null)
            {
                yield break;
            }

            if (redirectionPsfFixup.KnownFolders != null)
            {
                foreach (var knownGroup in redirectionPsfFixup.KnownFolders.GroupBy(kf => kf.Id))
                {
                    foreach (var relativeGroup in knownGroup.SelectMany(k => k.RelativePaths).GroupBy(pdr => pdr.Base))
                    {
                        yield return new PsfContentFolderRedirectionViewModel(knownGroup.Key, relativeGroup.Key, GetRules(relativeGroup));
                    }
                }
            }


            if (redirectionPsfFixup.PackageDriveRelative != null)
            {
                foreach (var relativeGroup in redirectionPsfFixup.PackageDriveRelative.GroupBy(pdr => pdr.Base))
                {
                    yield return new PsfContentFolderRedirectionViewModel(PsfContentFolderRelationTo.Drive, relativeGroup.Key, GetRules(relativeGroup));
                }
            }

            if (redirectionPsfFixup.PackageRelative != null)
            {
                foreach (var relativeGroup in redirectionPsfFixup.PackageRelative.GroupBy(pr => pr.Base))
                {
                    yield return new PsfContentFolderRedirectionViewModel(PsfContentFolderRelationTo.PackageRoot, relativeGroup.Key, GetRules(relativeGroup));
                }
            }
        }

        private static IEnumerable<PsfContentRuleRedirectionViewModel> GetRules(IEnumerable<PsfRedirectedPathEntryConfig> rawRules)
        {
            var list = new List<PsfContentRuleRedirectionViewModel>();

            var exclusions = new List<string>();

            foreach (var group in rawRules.GroupBy(r => new { r.RedirectTargetBase, r.IsReadOnly, r.IsExclusion }))
            {
                var files = new List<PsfContentFileRedirectionViewModel>();
                
                foreach (var item in group.SelectMany(g => g.Patterns))
                {
                    if (!group.Key.IsExclusion)
                    {
                        files.Add(new PsfContentFileRedirectionViewModel(item, false));
                    }
                    else
                    {
                        exclusions.Add(item);
                    }
                }

                if (files.Any())
                {
                    list.Add(new PsfContentRuleRedirectionViewModel(files.Where(f => !f.IsExclusion), files.Where(f => f.IsExclusion), group.Key.RedirectTargetBase, group.Key.IsReadOnly));
                }
            }

            if (exclusions.Any())
            {
                foreach (var item in list)
                {
                    foreach (var exclusion in exclusions)
                    {
                        item.Exclude.Add(new PsfContentFileRedirectionViewModel(exclusion, true));
                    }

                    item.Commit();
                }
            }
            
            return list;
        }
    }
}
