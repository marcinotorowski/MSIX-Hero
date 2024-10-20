// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Otor.MsixHero.Appx.Reader.Psf.Entities.Interpreter.Redirection;

namespace Otor.MsixHero.Appx.Reader.Psf.Entities.Interpreter;

public class InterpretedRegex
{
    private InterpretedRegex()
    {
    }

    public static InterpretedRegex CreateInterpretedFileRegex(string regularExpression)
    {
        var interpreted = new InterpretedRegex();
        interpreted.SetFromRegex(regularExpression, true);
        return interpreted;
    }

    public static InterpretedRegex CreateInterpretedRegex(string regularExpression)
    {
        var interpreted = new InterpretedRegex();
        interpreted.SetFromRegex(regularExpression, false);
        return interpreted;
    }
    
    public string Target { get; private set; }

    public string RegularExpression { get; private set; }

    public RegexInterpretationResult Comparison { get; private set; }

    private void SetFromRegex(string regex, bool asFile)
    {
        this.RegularExpression = regex;
        this.Comparison = RegexInterpretationResult.Custom;

        if (regex == ".*")
        {
            this.Comparison = RegexInterpretationResult.Any;
        }
        else if (Regex.IsMatch(regex, @"^\^[a-zA-Z0-9_]+$\$?"))
        {
            if (regex[^1] == '$')
            {
                this.Target = regex.Substring(1, regex.Length - 2);
                this.Comparison = RegexInterpretationResult.Name;
            }
            else
            {
                this.Target = regex.Substring(1);
                this.Comparison = RegexInterpretationResult.StartsWith;
            }
        }
        else if (Regex.IsMatch(regex, @"^\^?[a-zA-Z0-9_]+\$$"))
        {
            if (regex[0] == '^')
            {
                this.Target = regex.Substring(1, regex.Length - 2);
                this.Comparison = RegexInterpretationResult.Name;
            }
            else
            {
                this.Target = regex.Substring(0, regex.Length - 1);
                this.Comparison = RegexInterpretationResult.EndsWith;
            }
        }
        else if (Regex.IsMatch(regex, @"^[a-zA-Z0-9_\-]+$"))
        {
            this.Target = regex;
            this.Comparison = RegexInterpretationResult.Contains;
        }
        else if (asFile)
        {
            var match = Regex.Match(regex, @"^\^?\.*\\.([a-z0-9]+)");

            if (match.Success)
            {
                this.Target = "*." + match.Groups[1].Value;
                this.Comparison = RegexInterpretationResult.Extension;
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
                        this.Target = "*." + f1[0] + f2[0] + f3[0];
                        this.Comparison = RegexInterpretationResult.Extension;
                    }
                }
                else
                {
                    match = Regex.Match(regex, @"^\^?([^\\?*\.\^\$\(\)\[\]]+)\\.([^\\?*\.\^\$\(\)\[\]]+)\$$");
                    if (match.Success)
                    {
                        if (match.Value[0] == '^')
                        {
                            this.Target = match.Groups[1].Value + "." + match.Groups[2].Value;
                            this.Comparison = RegexInterpretationResult.Name;
                        }
                        else
                        {
                            this.Target = match.Groups[1].Value + "." + match.Groups[2].Value;
                            this.Comparison = RegexInterpretationResult.EndsWith;
                        }
                    }
                }
            }
        }
    }
}