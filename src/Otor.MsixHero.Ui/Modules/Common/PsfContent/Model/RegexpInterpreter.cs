using System.Text.RegularExpressions;

namespace Otor.MsixHero.Ui.Modules.Common.PsfContent.Model
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
                this.DisplayText = "(any)";
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
