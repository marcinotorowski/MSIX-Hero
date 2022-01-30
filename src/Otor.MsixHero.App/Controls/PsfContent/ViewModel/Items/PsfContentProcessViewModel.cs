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

using System;
using System.Text.RegularExpressions;
using Otor.MsixHero.App.Controls.PsfContent.Model;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Localization;

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items
{
    public abstract class PsfContentProcessViewModel : PsfContentRegexpViewModel
    {
        private readonly ChangeableProperty<Bitness> is64Bit;

        protected PsfContentProcessViewModel(string processRegularExpression, string fixupName) : base(processRegularExpression)
        {
            Bitness bitness;
            if (fixupName.IndexOf("64", StringComparison.Ordinal) != -1)
            {
                bitness = Bitness.x64;
            }
            else if (fixupName.IndexOf("32", StringComparison.Ordinal) != -1 || fixupName.IndexOf("86", StringComparison.Ordinal) != -1)
            {
                bitness = Bitness.x86;
            }
            else
            {
                bitness = Bitness.Unknown;
            }

            this.is64Bit = new ChangeableProperty<Bitness>(bitness);
            this.AddChildren(this.is64Bit);
        }

        public Bitness Is64Bit
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

        private static (string, string, string) DecomposePsfTranslation(string translation)
        {
            var match = Regex.Match(translation, @"\[(?<before>[^\]]*)\]\[(?<text>[^\]]*)\]\[(?<after>[^\]]*)\]");
            if (!match.Success)
            {
                throw new InvalidOperationException("Invalid value. Expected format [aaaa][bbbb][cccc]");
            }

            var before = match.Groups["after"].Value;
            var text = match.Groups["text"].Value;
            var after = match.Groups["after"].Value;

            if (string.IsNullOrEmpty(before))
            {
                before = null;
            }

            if (string.IsNullOrEmpty(text))
            {
                text = null;
            }

            if (string.IsNullOrEmpty(after))
            {
                text = null;
            }

            return (before, text, after);
        }

        protected sealed override void SetValuesFromRegularExpression(string expr)
        {
            var interpretation = new RegexpInterpreter(expr, false);

            switch (interpretation.Result)
            {
                case InterpretationResult.Any:
                    (this.TextBefore, this.DisplayText, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Process_AnyProcess);
                    break;
                case InterpretationResult.Name:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Process_AnyProcess);
                    // wtf?
                    this.TextBefore = Resources.Localization.Psf_Process_NamedProcess_TextBefore + " ";
                    break;
                case InterpretationResult.StartsWith:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Process_StartsWith);
                    break;
                case InterpretationResult.EndsWith:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Process_EndsWith);
                    break;
                default:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Process_EndsWith);
                    break;
            }
        }
    }
}