// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.App.Mvvm.Changeable;

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Redirection
{
    public class PsfContentFolderRedirectionViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<string> knownDir;

        private readonly ChangeableProperty<string> baseDir;
        
        private readonly ChangeableProperty<PsfContentFolderRelationTo> relationTo;

        public PsfContentFolderRedirectionViewModel(PsfContentFolderRelationTo relation, string baseDir, IEnumerable<PsfContentRuleRedirectionViewModel> rules) : this(relation, null, baseDir, rules)
        {
        }

        public PsfContentFolderRedirectionViewModel(string knownDir, string baseDir, IEnumerable<PsfContentRuleRedirectionViewModel> rules) : this(PsfContentFolderRelationTo.KnownFolder, knownDir, baseDir, rules)
        {
        }

        private PsfContentFolderRedirectionViewModel(PsfContentFolderRelationTo relation, string knownDir, string baseDir, IEnumerable<PsfContentRuleRedirectionViewModel> rules)
        {
            this.knownDir = new ChangeableProperty<string>(knownDir);
            this.baseDir = new ChangeableProperty<string>(baseDir);
            this.relationTo = new ChangeableProperty<PsfContentFolderRelationTo>(relation);
            
            this.Rules = new ChangeableCollection<PsfContentRuleRedirectionViewModel>(rules.OrderBy(r => r.Exclude.Any() ? 1 : 0));
            this.Rules.Commit();
            this.AddChildren(this.Rules, this.knownDir, this.relationTo, this.baseDir);

            this.knownDir.ValueChanged += this.DirectoryOnValueChanged;
            this.baseDir.ValueChanged += this.DirectoryOnValueChanged;
            this.relationTo.ValueChanged += this.DirectoryOnValueChanged;
        }

        public string FullDir
        {
            get
            {
                switch (this.RelationTo)
                {
                    case PsfContentFolderRelationTo.KnownFolder when string.IsNullOrEmpty(this.BaseDir):
                        return $@"{{{this.KnownDir}}}\";
                    case PsfContentFolderRelationTo.KnownFolder:
                        return $@"{{{this.KnownDir}}}\{this.BaseDir}\";
                    case PsfContentFolderRelationTo.PackageRoot when string.IsNullOrEmpty(this.BaseDir):
                        return "(Package Directory)";
                    case PsfContentFolderRelationTo.PackageRoot:
                        return $@"(Package Directory)\{this.BaseDir}\";
                    case PsfContentFolderRelationTo.Drive when string.IsNullOrEmpty(this.BaseDir):
                        return "(Root Drive)";
                    case PsfContentFolderRelationTo.Drive:
                        return $@"(Root Drive)\{this.BaseDir}\";
                    default:
                        return this.BaseDir;
                }
            }
        }

        public PsfContentFolderRelationTo RelationTo
        {
            get => this.relationTo.CurrentValue;

            set
            {
                if (this.relationTo.CurrentValue == value)
                {
                    return;
                }

                this.relationTo.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public string BaseDir
        {
            get => this.baseDir.CurrentValue;

            set
            {
                if (this.baseDir.CurrentValue == value)
                {
                    return;
                }

                this.baseDir.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public string KnownDir
        {
            get => this.knownDir.CurrentValue;

            set
            {
                if (this.knownDir.CurrentValue == value)
                {
                    return;
                }

                this.knownDir.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public ChangeableCollection<PsfContentRuleRedirectionViewModel> Rules { get; }

        private void DirectoryOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(this.FullDir));
        }
    }
}
