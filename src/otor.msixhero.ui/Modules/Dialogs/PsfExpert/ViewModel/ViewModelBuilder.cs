using System;
using System.Collections.Generic;
using System.Linq;
using otor.msixhero.lib.Domain.Appx.Psf;
using Unity.Processors;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel
{
    public class PsfExpertRedirectionViewModelBuilder
    {
        private readonly PsfConfig psfConfig;

        public PsfExpertRedirectionViewModelBuilder(PsfConfig psfConfig)
        {
            this.psfConfig = psfConfig;
        }

        public PsfExpertRedirectionsViewModel Build()
        {
            var results = new PsfExpertRedirectionsViewModel(this.psfConfig.Processes.SelectMany(this.Build));
            return results;
        }

        public PsfConfig Build(PsfExpertRedirectionsViewModel viewModel, PsfConfig originalConfig = null)
        {
            if (originalConfig == null)
            {
                originalConfig = new PsfConfig();
            }

            var redirs = viewModel.Redirections.ToList();
            foreach (var item in originalConfig.Processes)
            {
                var found = false;
                for (var i = item.Fixups.Count - 1; i >= 0; i--)
                {
                    var fixup = item.Fixups[i];
                    if (fixup.Config == null)
                    {
                        continue;
                    }

                    if (fixup.Config is PsfRedirectionFixupConfig)
                    {
                        item.Fixups.RemoveAt(i);

                        if (!found)
                        {
                            var def = this.GetRedirectionConfig(redirs, item);
                            item.Fixups[i] = def;
                        }
                        else
                        {
                            item.Fixups.RemoveAt(i);
                        }

                        found = true;
                    }
                }

                if (!found)
                {
                    var def = this.GetRedirectionConfig(redirs, item);
                    item.Fixups.Add(def);
                }

                for (var i = redirs.Count; i >= 0; i--)
                {
                    if (redirs[i].BaseObject == item)
                    {
                        redirs.RemoveAt(i);
                    }
                }
            }

            return originalConfig;
        }

        private PsfFixup GetRedirectionConfig(List<PsfExpertRedirectionIfProcessViewModel> redirs, PsfProcess item)
        {
            throw new NotImplementedException();

        }

        private IEnumerable<PsfExpertRedirectionIfProcessViewModel> Build(PsfProcess psfProcess)
        {
            var list = new List<PsfExpertRuleConditionViewModel>();

            foreach (var fixup in psfProcess.Fixups)
            {
                var config = fixup.Config as PsfRedirectionFixupConfig;
                if (config == null)
                {
                    continue;
                }

                foreach (var item in config.RedirectedPaths.PackageDriveRelative)
                {
                    var positive = new List<PsfExpertFilePatternViewModel>();
                    var negative = new List<PsfExpertFilePatternViewModel>();
                    
                    foreach (var pattern in item.Patterns)
                    {
                        if (item.IsExclusion)
                        {
                            negative.Add(new PsfExpertFilePatternViewModel(RedirectionType.RootRelative, true, item.Base, pattern));
                        }
                        else
                        {
                            positive.Add(new PsfExpertFilePatternViewModel(RedirectionType.RootRelative, false, item.Base, pattern));
                        }
                    }

                    var then = new PsfExpertThenViewModel(item.IsReadOnly, item.RedirectTargetBase);
                    var cond = new PsfExpertRuleConditionViewModel( positive, negative, then);
                    list.Add(cond);
                }

                foreach (var item in config.RedirectedPaths.KnownFolders)
                {
                    foreach (var relativePath in item.RelativePaths)
                    {
                        var positive = new List<PsfExpertFilePatternViewModel>();
                        var negative = new List<PsfExpertFilePatternViewModel>();

                        foreach (var pattern in relativePath.Patterns)
                        {
                            if (relativePath.IsExclusion)
                            {
                                negative.Add(new PsfExpertFilePatternViewModel(RedirectionType.KnownFolder, true, item.Id, relativePath.Base, pattern));
                            }
                            else
                            {
                                positive.Add(new PsfExpertFilePatternViewModel(RedirectionType.KnownFolder, false, item.Id, relativePath.Base, pattern));
                            }
                        }

                        var then = new PsfExpertThenViewModel(relativePath.IsReadOnly, relativePath.RedirectTargetBase);
                        var cond = new PsfExpertRuleConditionViewModel(positive, negative, then);
                        list.Add(cond);
                    }
                }

                foreach (var item in config.RedirectedPaths.PackageRelative)
                {
                    var positive = new List<PsfExpertFilePatternViewModel>();
                    var negative = new List<PsfExpertFilePatternViewModel>();

                    foreach (var pattern in item.Patterns)
                    {
                        if (item.IsExclusion)
                        {
                            negative.Add(new PsfExpertFilePatternViewModel(RedirectionType.PackageRelative, true, item.Base, pattern));
                        }
                        else
                        {
                            positive.Add(new PsfExpertFilePatternViewModel(RedirectionType.PackageRelative, false, item.Base, pattern));
                        }
                    }

                    var then = new PsfExpertThenViewModel(item.IsReadOnly, item.RedirectTargetBase);
                    var cond = new PsfExpertRuleConditionViewModel(positive, negative, then);

                    list.Add(cond);
                }

            var grouped = list.GroupBy(e => new {a = e.Then.IsReadOnly, b = e.Then.RedirectTargetBase});

            IList<PsfExpertRuleConditionViewModel> directoryConditions = new List<PsfExpertRuleConditionViewModel>();
            foreach (var group in grouped)
            {
                directoryConditions.Add(new PsfExpertRuleConditionViewModel(group.SelectMany(g => g.Positive), group.SelectMany(g => g.Negative), group.First().Then));
            }

            foreach (var d in directoryConditions)
            {
                // if (d.Then.IsReadOnly || !string.IsNullOrEmpty(d.Then.RedirectTargetBase))
                {
                    var extraNegations = directoryConditions.Where(x => x != d).SelectMany(x => x.Negative).Where(n => d.Negative.All(y => y.FullPath != n.FullPath)).ToList();
                    foreach (var en in extraNegations)
                    {
                        d.Negative.Add(en);
                    }
                }
            }

                yield return new PsfExpertRedirectionIfProcessViewModel(fixup.Dll, psfProcess, directoryConditions);
            }
        }
    }
}
