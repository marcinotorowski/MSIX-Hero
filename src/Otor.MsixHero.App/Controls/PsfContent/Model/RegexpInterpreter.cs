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

using System.Text.RegularExpressions;
using Otor.MsixHero.Infrastructure.Localization;

namespace Otor.MsixHero.App.Controls.PsfContent.Model
{
    public class RegexpInterpreter
    {
        private readonly bool treatAsFileName;

        public RegexpInterpreter(string regularExpression, bool treatAsFileName)
        {
            this.treatAsFileName = treatAsFileName;
            this.SetFromRegex(regularExpression);
        }

        public string DisplayText { get; private set; }

        public string EditText { get; private set; }

        public string RegularExpression { get; private set; }

        public InterpretationResult Result { get; private set; }

        private void SetFromRegex(string regex)
        {
            this.RegularExpression = regex;
            this.Result = InterpretationResult.Custom;
            this.EditText = this.RegularExpression;

            if (regex == ".*")
            {
                this.DisplayText = "(" + Resources.Localization.Psf_Regex_Any + ")";
                this.Result = InterpretationResult.Any;
            }
            //else if (regex == @".*\..*")
            //{
            //    this.DisplayText = "(any with extension)";
            //    this.EditText = "";
            //    this.Result = InterpretationResult.Any;
            //}
            else if (Regex.IsMatch(regex, @"^\^[a-zA-Z0-9_]+$\$?"))
            {
                if (regex[regex.Length - 1] == '$')
                {
                    this.DisplayText = regex.Substring(1, regex.Length - 2);
                    this.EditText = this.DisplayText;
                    this.Result = InterpretationResult.Name;
                }
                else
                {
                    this.DisplayText = regex.Substring(1);
                    this.EditText = this.DisplayText;
                    this.Result = InterpretationResult.StartsWith;
                }
            }
            else if (Regex.IsMatch(regex, @"^\^?[a-zA-Z0-9_]+\$$"))
            {
                if (regex[0] == '^')
                {
                    this.DisplayText = regex.Substring(1, regex.Length - 2);
                    this.EditText = this.DisplayText;
                    this.Result = InterpretationResult.Name;
                }
                else
                {
                    this.DisplayText = regex.Substring(0, regex.Length - 1);
                    this.EditText = this.DisplayText;
                    this.Result = InterpretationResult.EndsWith;
                }
            }
            else if (this.treatAsFileName)
            {
                var match = Regex.Match(regex, @"^\^?\.*\\.([a-z0-9]+)");
                
                if (match.Success)
                {
                    this.DisplayText = "*." + match.Groups[1].Value;
                    this.EditText = match.Groups[1].Value;
                    this.Result = InterpretationResult.Extension;
                }
                else
                {
                    match = Regex.Match(regex, @"^\^?\.\*\\\.\[(\w{2})\]\[(\w{2})\]\[(\w{2})\]\$$");
                    if (match.Success)
                    {
                        var f1 = match.Groups[1].Value.ToLowerInvariant();
                        var f2 = match.Groups[2].Value.ToLowerInvariant();
                        var f3 = match.Groups[3].Value.ToLowerInvariant();

                        if (f1[0] == f1[1] && f2[0] == f2[1] && f3[0] == f3[1])
                        {
                            this.DisplayText = "*." + f1[0] + f2[0] + f3[0];
                            this.EditText = "" + f1[0] + f2[0] + f3[0];
                            this.Result = InterpretationResult.Extension;
                        }
                    }
                    else
                    {
                        match = Regex.Match(regex, @"^\^?([^\\?*\.\^\$\(\)\[\]]+)\\.([^\\?*\.\^\$\(\)\[\]]+)\$$");
                        if (match.Success)
                        {
                            if (match.Value[0] == '^')
                            {
                                this.DisplayText = match.Groups[1].Value + "." + match.Groups[2].Value;
                                this.EditText = this.DisplayText;
                                this.Result = InterpretationResult.Name;
                            }
                            else
                            {
                                this.DisplayText = match.Groups[1].Value + "." + match.Groups[2].Value;
                                this.EditText = this.DisplayText;
                                this.Result = InterpretationResult.EndsWith;
                            }
                        }
                    }
                }
            }
        }
    }
}
