using System;
using System.Collections.Generic;
using System.Linq;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Common.PsfContent.View;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items
{
    public class PsfContentProcessViewModel : PsfContentRegexpViewModel
    {
        private readonly ChangeableProperty<bool> is64Bit;

        public PsfContentProcessViewModel(string processRegularExpression, string fixupName, PsfRedirectedPathConfig redirectionPsfFixup) : base(processRegularExpression)
        {
            this.is64Bit = new ChangeableProperty<bool>(fixupName.IndexOf("64", StringComparison.Ordinal) != -1);
            this.Rules = new ChangeableCollection<PsfContentFolderViewModel>(this.SetRules(redirectionPsfFixup));
            this.Rules.Commit();

            this.AddChildren(this.is64Bit, this.Rules);
        }

        public ChangeableCollection<PsfContentFolderViewModel> Rules { get; }

        public bool Is64Bit
        {
            get => this.is64Bit.CurrentValue;
            set
            {
                if (this.is64Bit.CurrentValue == value)
                {
                    return;
                }

                this.is64Bit.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        protected sealed override void SetValuesFromRegularExpression(string expr)
        {
            var interpretation = new RegexpInterpreter(expr, false);

            switch (interpretation.Result)
            {
                case InterpretationResult.Any:
                    this.TextBefore = null;
                    this.DisplayText = "any ";
                    this.TextAfter = "process";
                    break;
                case InterpretationResult.Name:
                    this.TextBefore = "process ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                case InterpretationResult.StartsWith:
                    this.TextBefore = "processes that start with ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                case InterpretationResult.EndsWith:
                    this.TextBefore = "processes that end with ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                default:
                    this.TextBefore = "processes that match pattern ";
                    this.DisplayText = interpretation.RegularExpression;
                    this.TextAfter = null;
                    break;
            }
        }

        private IEnumerable<PsfContentFolderViewModel> SetRules(PsfRedirectedPathConfig redirectionPsfFixup)
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
                        yield return new PsfContentFolderViewModel(knownGroup.Key, relativeGroup.Key, GetRules(relativeGroup));
                    }
                }
            }


            if (redirectionPsfFixup.PackageDriveRelative != null)
            {
                foreach (var relativeGroup in redirectionPsfFixup.PackageDriveRelative.GroupBy(pdr => pdr.Base))
                {
                    yield return new PsfContentFolderViewModel(PsfContentFolderRelationTo.Drive, relativeGroup.Key, GetRules(relativeGroup));
                }
            }

            if (redirectionPsfFixup.PackageRelative != null)
            {
                foreach (var relativeGroup in redirectionPsfFixup.PackageRelative.GroupBy(pr => pr.Base))
                {
                    yield return new PsfContentFolderViewModel(PsfContentFolderRelationTo.PackageRoot, relativeGroup.Key, GetRules(relativeGroup));
                }
            }
        }

        private static IEnumerable<PsfContentRuleViewModel> GetRules(IEnumerable<PsfRedirectedPathEntryConfig> rawRules)
        {
            foreach (var group in rawRules.GroupBy(r => new { r.RedirectTargetBase, r.IsReadOnly, r.IsExclusion }))
            {
                var files = new List<PsfContentFileViewModel>();
                
                foreach (var item in group.SelectMany(g => g.Patterns))
                {
                    files.Add(new PsfContentFileViewModel(item, group.Key.IsExclusion));
                }

                yield return new PsfContentRuleViewModel(files.Where(f => !f.IsExclusion), files.Where(f => f.IsExclusion), group.Key.RedirectTargetBase, group.Key.IsReadOnly);
            }
        }
    }
}
