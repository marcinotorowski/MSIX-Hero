using System;
using System.Collections.Generic;
using System.Linq;
using otor.msixhero.lib.Domain.Appx.Psf;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel.Items
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

                var pkgDriveRelative = new List<PsfRedirectedPathEntryConfig>(config.RedirectedPaths.PackageDriveRelative);
                var pkgDirectoryRelative = new List<PsfRedirectedPathEntryConfig>(config.RedirectedPaths.PackageRelative);
                var pkgKnownFolderRelative = new List<PsfRedirectedPathKnownFolderEntryConfig>(config.RedirectedPaths.KnownFolders);

                var allPackageDriveRelativeArePositive = pkgDriveRelative.All(x => !x.IsExclusion);
                var allKnownFoldersArePositive = pkgKnownFolderRelative.SelectMany(x => x.RelativePaths).All(x => !x.IsExclusion);
                var allPackageDirectoryRelativeArePositive = pkgDirectoryRelative.All(x => !x.IsExclusion);

                if (allPackageDirectoryRelativeArePositive || allKnownFoldersArePositive || allPackageDriveRelativeArePositive)
                {
                    var positiveRules = new List<PsfExpertFilePatternViewModel>();

                    if (allPackageDirectoryRelativeArePositive)
                    {
                        foreach (var item in pkgDirectoryRelative)
                        {
                            foreach (var x in item.Patterns)
                            {
                                positiveRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.PackageRelative, false, item.Base, x, item.IsReadOnly, item.RedirectTargetBase));
                            }
                        }

                        pkgDirectoryRelative.Clear();
                    }

                    if (allPackageDriveRelativeArePositive)
                    {
                        foreach (var item in pkgDriveRelative)
                        {
                            foreach (var x in item.Patterns)
                            {
                                positiveRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.RootRelative, false, item.Base, x, item.IsReadOnly, item.RedirectTargetBase));
                            }
                        }

                        pkgDriveRelative.Clear();
                    }

                    if (allKnownFoldersArePositive)
                    {
                        foreach (var item in pkgKnownFolderRelative)
                        {
                            foreach (var subItem in item.RelativePaths)
                            {
                                foreach (var x in subItem.Patterns)
                                {
                                    positiveRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.KnownFolder, false, item.Id, subItem.Base, x, subItem.IsReadOnly, subItem.RedirectTargetBase));
                                }
                            }
                        }

                        pkgKnownFolderRelative.Clear();
                    }

                    list.Add(new PsfExpertRuleConditionViewModel(positiveRules, Enumerable.Empty<PsfExpertFilePatternViewModel>(), new PsfExpertThenViewModel()));
                }

                if (pkgDirectoryRelative.Any())
                {
                    var positiveRules = new List<PsfExpertFilePatternViewModel>();
                    var negativeRules = new List<PsfExpertFilePatternViewModel>();

                    foreach (var item in pkgDirectoryRelative)
                    {
                        foreach (var x in item.Patterns)
                        {
                            if (item.IsExclusion)
                            {
                                negativeRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.PackageRelative, true, item.Base, x, item.IsReadOnly, item.RedirectTargetBase));
                            }
                            else
                            {
                                positiveRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.PackageRelative, false, item.Base, x, item.IsReadOnly, item.RedirectTargetBase));
                            }
                        }
                    }

                    list.Add(new PsfExpertRuleConditionViewModel(positiveRules, negativeRules, new PsfExpertThenViewModel()));
                }

                if (pkgDriveRelative.Any())
                {
                    var positiveRules = new List<PsfExpertFilePatternViewModel>();
                    var negativeRules = new List<PsfExpertFilePatternViewModel>();

                    foreach (var item in pkgDriveRelative)
                    {
                        foreach (var x in item.Patterns)
                        {
                            if (item.IsExclusion)
                            {
                                negativeRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.RootRelative, true, item.Base, x, item.IsReadOnly, item.RedirectTargetBase));
                            }
                            else
                            {
                                positiveRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.RootRelative, false, item.Base, x, item.IsReadOnly, item.RedirectTargetBase));
                            }
                        }
                    }

                    list.Add(new PsfExpertRuleConditionViewModel(positiveRules, negativeRules, new PsfExpertThenViewModel()));
                }

                if (pkgKnownFolderRelative.Any())
                {
                    var positiveRules = new List<PsfExpertFilePatternViewModel>();
                    var negativeRules = new List<PsfExpertFilePatternViewModel>();

                    foreach (var item in pkgKnownFolderRelative)
                    {
                        foreach (var subItem in item.RelativePaths)
                        {
                            foreach (var x in subItem.Patterns)
                            {
                                if (subItem.IsExclusion)
                                {
                                    negativeRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.KnownFolder, true, item.Id, subItem.Base, x, subItem.IsReadOnly, subItem.RedirectTargetBase));
                                }
                                else
                                {
                                    positiveRules.Add(new PsfExpertFilePatternViewModel(RedirectionType.KnownFolder, false, item.Id, subItem.Base, x, subItem.IsReadOnly, subItem.RedirectTargetBase));
                                }
                            }
                        }
                    }

                    list.Add(new PsfExpertRuleConditionViewModel(positiveRules, negativeRules, new PsfExpertThenViewModel()));
                }

                yield return new PsfExpertRedirectionIfProcessViewModel(fixup.Dll, psfProcess, list);
            }
        }
    }
}
