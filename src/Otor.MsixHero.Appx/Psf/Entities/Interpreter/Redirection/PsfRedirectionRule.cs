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

namespace Otor.MsixHero.Appx.Psf.Entities.Interpreter.Redirection;

public class PsfRedirectionRule
{
    private readonly List<InterpretedRegex> _include = new();
    private readonly List<InterpretedRegex> _exclude = new();
    
    private PsfRedirectionRule(IEnumerable<InterpretedRegex> include, IEnumerable<InterpretedRegex> exclude = null, PsfRedirectionBase redirectTo = null)
    {
        if (include != null)
        {
            this._include.AddRange(include);
        }

        if (exclude != null)
        {
            this._exclude.AddRange(exclude);
        }
        
        this.RedirectionBase = redirectTo ?? new PsfRedirectionBase();
    }

    public static PsfRedirectionRule CreateRedirectedRoot(string directory, IEnumerable<InterpretedRegex> include, IEnumerable<InterpretedRegex> exclude = null, PsfRedirectionBase redirectTo = null)
    {
        var rule = new PsfRedirectionRule(include, exclude, redirectTo)
        {
            DirectoryPath = "<PkgDir>\\" + directory,
            Root = PsfRedirectionRoot.PackageRoot
        };

        return rule;
    }

    public static PsfRedirectionRule CreateRedirectedPackageDrive(string directory, IEnumerable<InterpretedRegex> include, IEnumerable<InterpretedRegex> exclude = null, PsfRedirectionBase redirectTo = null)
    {
        var rule = new PsfRedirectionRule(include, exclude, redirectTo)
        {
            DirectoryPath = "C:\\" + directory,
            Root = PsfRedirectionRoot.Drive
        };

        return rule;
    }

    public static PsfRedirectionRule CreateRedirectedKnownFolder(string knownFolder, string directory, IEnumerable<InterpretedRegex> include, IEnumerable<InterpretedRegex> exclude = null, PsfRedirectionBase redirectTo = null)
    {
        var rule = new PsfRedirectionRule(include, exclude, redirectTo)
        {
            Root = PsfRedirectionRoot.KnownFolder
        };

        if (string.IsNullOrEmpty(directory))
        {
            rule.DirectoryPath = "<" + knownFolder + ">";
        }
        else
        {
            rule.DirectoryPath = "<" + knownFolder + ">\\" + directory;
        }

        return rule;
    }

    public string DirectoryPath { get; private set; }

    public IReadOnlyList<InterpretedRegex> Include => this._include;

    public IReadOnlyList<InterpretedRegex> Exclude => this._exclude;

    public PsfRedirectionRule Inflate(IEnumerable<InterpretedRegex> additionalIncludes)
    {
        var newIncludes = new List<InterpretedRegex>();
        foreach (var inc in additionalIncludes)
        {
            if (this.Include.Any(r => r.Comparison == inc.Comparison && r.Target == inc.Target))
            {
                continue;
            }

            newIncludes.Add(inc);
        }

        if (newIncludes.Any())
        {
            return new PsfRedirectionRule(this.Include.Concat(newIncludes), this.Exclude?.ToList(), new PsfRedirectionBase(this.RedirectionBase?.Target, this.RedirectionBase?.IsReadOnly == true))
            {
                DirectoryPath = this.DirectoryPath,
                Root = this.Root
            };
        }

        return this;
    }

    public PsfRedirectionBase RedirectionBase { get; private set; }

    public PsfRedirectionRoot Root { get; private set; }
    
    public static IEnumerable<PsfRedirectionRule> EnumerateRules(PsfRedirectedPathConfig redirectionPsfFixup)
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
                    var allRules = GetRules(PsfRedirectionRoot.KnownFolder, relativeGroup.Key, knownGroup.Key, relativeGroup);

                    foreach (var rule in allRules)
                    {
                        yield return rule;
                    }
                }
            }
        }


        if (redirectionPsfFixup.PackageDriveRelative != null)
        {
            foreach (var relativeGroup in redirectionPsfFixup.PackageDriveRelative.GroupBy(pdr => pdr.Base))
            {
                var allRules = GetRules(PsfRedirectionRoot.Drive, relativeGroup.Key, null, relativeGroup);

                foreach (var rule in allRules)
                {
                    yield return rule;
                }
            }
        }

        if (redirectionPsfFixup.PackageRelative != null)
        {
            foreach (var relativeGroup in redirectionPsfFixup.PackageRelative.GroupBy(pr => pr.Base))
            {
                var allRules = GetRules(PsfRedirectionRoot.PackageRoot, relativeGroup.Key, null, relativeGroup);

                foreach (var rule in allRules)
                {
                    yield return rule;
                }
            }
        }
    }

    private static IEnumerable<PsfRedirectionRule> GetRules(PsfRedirectionRoot root, string basePath, string knownPath, IEnumerable<PsfRedirectedPathEntryConfig> rawRules)
    {
        var list = new List<PsfRedirectionRule>();

        var exclusions = new List<InterpretedRegex>();

        foreach (var group in rawRules.GroupBy(r => new { r.RedirectTargetBase, r.IsReadOnly, r.IsExclusion }))
        {
            var inclusions = new List<InterpretedRegex>();

            foreach (var item in group.SelectMany(g => g.Patterns))
            {
                if (!group.Key.IsExclusion)
                {
                    inclusions.Add(InterpretedRegex.CreateInterpretedFileRegex(item));
                }
                else
                {
                    exclusions.Add(InterpretedRegex.CreateInterpretedFileRegex(item));
                }
            }
            
            if (inclusions.Any())
            {
                switch (root)
                {
                    case PsfRedirectionRoot.PackageRoot:
                        list.Add(CreateRedirectedRoot(basePath, inclusions, redirectTo: new PsfRedirectionBase(group.Key.RedirectTargetBase, group.Key.IsReadOnly)));
                        break;
                    case PsfRedirectionRoot.Drive:
                        list.Add(CreateRedirectedPackageDrive(basePath, inclusions, redirectTo: new PsfRedirectionBase(group.Key.RedirectTargetBase, group.Key.IsReadOnly)));
                        break;
                    case PsfRedirectionRoot.KnownFolder:
                        list.Add(CreateRedirectedKnownFolder(knownPath, basePath, inclusions, redirectTo: new PsfRedirectionBase(group.Key.RedirectTargetBase, group.Key.IsReadOnly)));
                        break;
                }
            }
        }

        // Now go back and set exclusions for all
        foreach (var item in list)
        {
            foreach (var excl in exclusions)
            {
                item._exclude.Add(excl);
            }
        }

        Merge(list);
        return list;
    }

    private static void Merge(IList<PsfRedirectionRule> list)
    {
        // do some merging
        for (var i = 0; i < list.Count; i++)
        {
            var currentItem = list[i];

            var findSimilar = list.Skip(i + 1).Where(item =>
            {
                if (item.Root != currentItem.Root)
                {
                    return false;
                }

                if (item.DirectoryPath != currentItem.DirectoryPath)
                {
                    return false;
                }

                if (item.RedirectionBase.Target != currentItem.RedirectionBase.Target)
                {
                    return false;
                }

                if (item.RedirectionBase.IsReadOnly != currentItem.RedirectionBase.IsReadOnly)
                {
                    return false;
                }

                return true;
            }).ToList();

            list[i] = currentItem.Inflate(findSimilar.SelectMany(x => x.Include));

            foreach (var found in findSimilar)
            {
                list.Remove(found);
            }
        }
    }
}