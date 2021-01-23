// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using Otor.MsixHero.App.Mvvm.Changeable;

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Redirection
{
    public class PsfContentRuleRedirectionViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<string> targetRedirection;
        private readonly ChangeableProperty<bool> isReadOnly;

        public PsfContentRuleRedirectionViewModel(IEnumerable<string> include, IEnumerable<string> exclude, string redirectBase, bool isReadOnly)
        {
            this.targetRedirection = new ChangeableProperty<string>(redirectBase);
            this.isReadOnly = new ChangeableProperty<bool>(isReadOnly);

            this.Include = new ChangeableCollection<PsfContentFileRedirectionViewModel>();
            this.Exclude = new ChangeableCollection<PsfContentFileRedirectionViewModel>();

            foreach (var item in include)
            {
                this.Include.Add(new PsfContentFileRedirectionViewModel(item));
            }

            foreach (var item in exclude)
            {
                this.Exclude.Add(new PsfContentFileRedirectionViewModel(item, true));
            }

            this.Include.Commit();
            this.Exclude.Commit();

            this.AddChildren(this.Include, this.Exclude, this.isReadOnly, this.targetRedirection);
        }

        public PsfContentRuleRedirectionViewModel(IEnumerable<PsfContentFileRedirectionViewModel> include, IEnumerable<PsfContentFileRedirectionViewModel> exclude, string redirectBase, bool isReadOnly)
        {
            this.targetRedirection = new ChangeableProperty<string>(redirectBase);
            this.isReadOnly = new ChangeableProperty<bool>(isReadOnly);

            this.Include = new ChangeableCollection<PsfContentFileRedirectionViewModel>(include);
            this.Exclude = new ChangeableCollection<PsfContentFileRedirectionViewModel>(exclude);

            this.Include.Commit();
            this.Exclude.Commit();

            this.AddChildren(this.Include, this.Exclude, this.isReadOnly, this.targetRedirection);
        }

        public ChangeableCollection<PsfContentFileRedirectionViewModel> Include { get; }

        public ChangeableCollection<PsfContentFileRedirectionViewModel> Exclude { get; }

        public string TargetRedirection
        {
            get => this.targetRedirection.CurrentValue;
            set
            {
                if (this.targetRedirection.CurrentValue == value)
                {
                    return;
                }

                this.targetRedirection.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsReadOnly
        {
            get => this.isReadOnly.CurrentValue;
            set
            {
                if (this.isReadOnly.CurrentValue == value)
                {
                    return;
                }

                this.isReadOnly.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }
    }
}
