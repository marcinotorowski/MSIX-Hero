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

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items
{
    public abstract class PsfContentRegexpViewModel : ChangeableContainer
    {
        // ReSharper disable once InconsistentNaming
        protected ChangeableProperty<string> regularExpression;
        private string displayText;
        private string textBefore;
        private string textAfter;

        protected PsfContentRegexpViewModel(string regularExpression)
        {
            this.regularExpression = new ChangeableProperty<string>(regularExpression);
            this.regularExpression.ValueChanged += this.RegularExpressionOnValueChanged;
            this.SetValuesFromRegularExpression(regularExpression);
            this.AddChild(this.regularExpression);
        }

        public string RegularExpression
        {
            get => this.regularExpression.CurrentValue;
            set
            {
                if (this.regularExpression.CurrentValue == value)
                {
                    return;
                }

                this.regularExpression.CurrentValue = value;
                this.OnPropertyChanged(nameof(this.RegularExpression));
            }
        }

        public string DisplayText
        {
            get => this.displayText;
            protected set => this.SetField(ref this.displayText, value);
        }

        public string TextBefore
        {
            get => this.textBefore;
            protected set => this.SetField(ref this.textBefore, value);
        }

        public string TextAfter
        {
            get => this.textAfter;
            protected set => this.SetField(ref this.textAfter, value);
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

        protected virtual void SetValuesFromRegularExpression(string expr)
        {
            var interpretation = new RegexpInterpreter(expr, true);

            switch (interpretation.Result)
            {
                case InterpretationResult.Any:
                    (this.TextBefore, this.DisplayText, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Regex_AllFiles);
                    break;
                case InterpretationResult.Extension:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Regex_FileWithExtension);
                    this.DisplayText = interpretation.DisplayText;
                    break;
                case InterpretationResult.Name:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Regex_File);
                    this.DisplayText = interpretation.DisplayText;
                    break;
                case InterpretationResult.StartsWith:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Regex_FilesStartingWith);
                    this.DisplayText = interpretation.DisplayText;
                    break;
                case InterpretationResult.EndsWith:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Regex_FilesEndingWith);
                    this.DisplayText = interpretation.DisplayText;
                    break;
                default:
                    (this.TextBefore, _, this.TextAfter) = DecomposePsfTranslation(Resources.Localization.Psf_Regex_FilesMatching);
                    this.DisplayText = interpretation.RegularExpression;
                    break;
            }
        }

        private void RegularExpressionOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.SetValuesFromRegularExpression((string)e.NewValue);
        }
    }
}