using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.Appx.Psf;

public class PackageSupportFrameworkProxyReader(
    string applicationId,
    string originalEntryPoint,
    IAppxFileReader fileReader) : ApplicationProxyReader
{
    public override async Task<BaseApplicationProxy> Inspect(CancellationToken cancellationToken = default)
    {
        var dir = Path.GetDirectoryName(originalEntryPoint);
        var configJson = string.IsNullOrEmpty(dir) ? "config.json" : Path.Combine(dir, "config.json");

        if (!fileReader.FileExists(configJson))
        {
            return null;
        }

        await using var jsonStream = fileReader.GetFile(configJson);
        using var jsonStreamReader = new StreamReader(jsonStream);
        var jsonSerializer = new PsfConfigSerializer();
        var config = jsonSerializer.Deserialize(await jsonStreamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));

        string executable = null;
        string workingDirectory = null;
        string arguments = null;
        var scripts = new List<PsfScriptDescriptor>();

        foreach (var item in config.Applications ?? Enumerable.Empty<PsfApplication>())
        {
            var id = item.Id;
            if (id != applicationId)
            {
                continue;
            }

            executable = item.Executable;
            workingDirectory = item.WorkingDirectory;
            arguments = item.Arguments;

            if (item.StartScript != null)
            {
                scripts.Add(new PsfScriptDescriptor(item.StartScript, PsfScriptDescriptorTiming.Start));
            }

            if (item.EndScript != null)
            {
                scripts.Add(new PsfScriptDescriptor(item.EndScript, PsfScriptDescriptorTiming.Finish));
            }

            break;
        }

        if (executable == null)
        {
            return null;
        }

        var psfDef = new PsfApplicationProxy
        {
            Executable = executable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            OtherFixups = new List<string>(),
            FileRedirections = new List<PsfFolderRedirectionDescriptor>(),
            Tracing = null,
            Scripts = scripts
        };

        foreach (var p in config.Processes ?? Enumerable.Empty<PsfProcess>())
        {
            var exe = p.Executable;
            if (string.IsNullOrEmpty(exe))
            {
                continue;
            }

            Regex regex;
            try
            {
                regex = new Regex(exe);
            }
            catch (Exception)
            {
                continue;
            }

            var indexOf = executable.LastIndexOfAny(new[] { '\\', '/' });
            if (indexOf != -1)
            {
                executable = executable.Substring(indexOf + 1);
            }

            if (!regex.IsMatch(executable))
            {
                executable = Path.GetFileNameWithoutExtension(executable);

                if (!regex.IsMatch(executable))
                {
                    continue;
                }
            }

            foreach (var fixup in p.Fixups ?? Enumerable.Empty<PsfFixup>())
            {
                var dll = fixup.Dll;
                if (dll == null)
                {
                    continue;
                }

                switch (dll.ToLower())
                {
                    case "tracefixup64.dll":
                    {
                        if (fixup.Config is PsfTraceFixupConfig psfFixupConfig)
                        {
                            psfDef.Tracing = new PsfTracingRedirectionDescriptor(psfFixupConfig ?? new PsfTraceFixupConfig(), PsfBitness.x64);
                        }

                        break;
                    }

                    case "electronfixup.dll":
                    {
                        psfDef.Electron = new PsfElectronDescriptor();
                        break;
                    }

                    case "tracefixup32.dll":
                    {
                        if (fixup.Config is PsfTraceFixupConfig psfFixupConfig)
                        {
                            psfDef.Tracing = new PsfTracingRedirectionDescriptor(psfFixupConfig ?? new PsfTraceFixupConfig(), PsfBitness.x86);
                        }
                        break;
                    }

                    case "tracefixup.dll":
                    {
                        if (fixup.Config is PsfTraceFixupConfig psfFixupConfig)
                        {
                            psfDef.Tracing = new PsfTracingRedirectionDescriptor(psfFixupConfig ?? new PsfTraceFixupConfig(), PsfBitness.x64);
                        }
                        break;
                    }

                    case "fileredirectionfixup64.dll":
                    case "fileredirectionfixup32.dll":
                    case "fileredirectionfixup.dll":

                        var redirs = this.GetFolderRedirections(fixup);
                        if (redirs != null)
                        {
                            psfDef.FileRedirections.AddRange(redirs);
                        }

                        break;
                    default:
                        psfDef.OtherFixups.Add(dll);
                        break;
                }
            }
        }

        return psfDef;
    }

    private List<PsfFolderRedirectionDescriptor> GetFolderRedirections(PsfFixup jsonConvert)
    {
        var list = new List<PsfFolderRedirectionDescriptor>();

        if (!(jsonConvert.Config is PsfRedirectionFixupConfig config))
        {
            return list;
        }

        var redirectedPath = config.RedirectedPaths;

        if (redirectedPath == null)
        {
            return list;
        }

        var packageRelative = redirectedPath.PackageRelative;
        if (packageRelative != null)
        {
            foreach (var packageRelativeItem in packageRelative)
            {
                var valueBase = packageRelativeItem.Base;
                if (string.IsNullOrEmpty(valueBase))
                {
                    valueBase = Resources.Localization.Packages_PkgDir_Placeholder;
                }
                else
                {
                    valueBase = Path.Join(Resources.Localization.Packages_PkgDir_Placeholder, valueBase).TrimEnd('\\');
                }

                var def = list.FirstOrDefault(item => string.Equals(valueBase, item.Directory, StringComparison.OrdinalIgnoreCase));

                if (def == null)
                {
                    def = new PsfFolderRedirectionDescriptor
                    {
                        Directory = valueBase,
                        Inclusions = new List<PsfFileRedirectionDescriptor>(),
                        Exclusions = new List<PsfFileRedirectionDescriptor>()
                    };

                    list.Add(def);
                }

                if (packageRelativeItem.IsExclusion)
                {
                    foreach (var pattern in packageRelativeItem.Patterns ?? Enumerable.Empty<string>())
                    {
                        def.Exclusions.Add(new PsfFileRedirectionDescriptor { RegularExpression = pattern, DisplayName = GetNiceRegExpDescription(pattern) });
                    }
                }
                else
                {
                    foreach (var pattern in packageRelativeItem.Patterns ?? Enumerable.Empty<string>())
                    {
                        def.Inclusions.Add(new PsfFileRedirectionDescriptor { RegularExpression = pattern, DisplayName = GetNiceRegExpDescription(pattern) });
                    }
                }
            }
        }

        var packageDriveRelative = redirectedPath.PackageDriveRelative;
        if (packageDriveRelative != null)
        {
            foreach (var packageDriveRelativeItem in packageDriveRelative)
            {
                var valueBase = packageDriveRelativeItem.Base;
                if (string.IsNullOrEmpty(valueBase))
                {
                    valueBase = Resources.Localization.Packages_PkgDrv_Placeholder;
                }
                else
                {
                    valueBase = Path.Join(Resources.Localization.Packages_PkgDrv_Placeholder, valueBase).TrimEnd('\\');
                }

                var def = list.FirstOrDefault(item => string.Equals(valueBase, item.Directory, StringComparison.OrdinalIgnoreCase));
                if (def == null)
                {

                    def = new PsfFolderRedirectionDescriptor
                    {
                        Directory = valueBase,
                        Inclusions = new List<PsfFileRedirectionDescriptor>(),
                        Exclusions = new List<PsfFileRedirectionDescriptor>()
                    };

                    list.Add(def);
                }

                if (packageDriveRelativeItem.IsExclusion)
                {
                    foreach (var pattern in packageDriveRelativeItem.Patterns ?? Enumerable.Empty<string>())
                    {
                        def.Exclusions.Add(new PsfFileRedirectionDescriptor() { RegularExpression = pattern, DisplayName = GetNiceRegExpDescription(pattern) });
                    }
                }
                else
                {
                    foreach (var pattern in packageDriveRelativeItem.Patterns ?? Enumerable.Empty<string>())
                    {
                        def.Inclusions.Add(new PsfFileRedirectionDescriptor() { RegularExpression = pattern, DisplayName = GetNiceRegExpDescription(pattern) });
                    }
                }
            }
        }

        var knownFolders = redirectedPath.KnownFolders;
        if (knownFolders != null)
        {
            foreach (var knownFolderItem in knownFolders)
            {
                var id = knownFolderItem.Id;
                var relativePaths = knownFolderItem.RelativePaths;

                if (relativePaths != null)
                {
                    foreach (var relativePathItem in relativePaths)
                    {
                        var valueBase = relativePathItem.Base;
                        if (string.IsNullOrEmpty(valueBase))
                        {
                            valueBase = "{" + id + "}";
                        }
                        else
                        {
                            valueBase = Path.Join("{" + id + "}", valueBase).TrimEnd('\\');
                        }

                        var def = list.FirstOrDefault(item => string.Equals(valueBase, item.Directory, StringComparison.OrdinalIgnoreCase));

                        if (def == null)
                        {
                            def = new PsfFolderRedirectionDescriptor
                            {
                                Directory = valueBase,
                                Inclusions = new List<PsfFileRedirectionDescriptor>(),
                                Exclusions = new List<PsfFileRedirectionDescriptor>()
                            };

                            list.Add(def);
                        }

                        if (relativePathItem.IsExclusion)
                        {
                            foreach (var pattern in relativePathItem.Patterns ?? Enumerable.Empty<string>())
                            {
                                def.Exclusions.Add(new PsfFileRedirectionDescriptor() { RegularExpression = pattern, DisplayName = GetNiceRegExpDescription(pattern) });
                            }
                        }
                        else
                        {
                            foreach (var pattern in relativePathItem.Patterns ?? Enumerable.Empty<string>())
                            {
                                def.Inclusions.Add(new PsfFileRedirectionDescriptor() { RegularExpression = pattern, DisplayName = GetNiceRegExpDescription(pattern) });
                            }
                        }
                    }
                }
            }
        }

        // make sure the list is sorted from the less to the most generic items…
        list = list.OrderBy(x => x.Directory.Split('\\').Length).ToList();

        return list;
    }

    private static string GetNiceRegExpDescription(string regexp)
    {
        if (string.IsNullOrEmpty(regexp) || regexp == ".*")
        {
            return Resources.Localization.Psf_Descriptor_AllFiles;
        }

        Match match;

        match = Regex.Match(regexp, @"^\^?(\w+)\\.(\w+)\$?$");
        if (match.Success)
        {
            return match.Groups[1].Value + "." + match.Groups[2].Value;
        }

        match = Regex.Match(regexp, @"^\^?\.\*\\\.\[(\w{2})\]\[(\w{2})\]\[(\w{2})\]\$$");
        if (match.Success)
        {
            var f1 = match.Groups[1].Value.ToLowerInvariant();
            var f2 = match.Groups[2].Value.ToLowerInvariant();
            var f3 = match.Groups[3].Value.ToLowerInvariant();

            if (f1[0] == f1[1] && f2[0] == f2[1] && f3[0] == f3[1])
            {
                return string.Format(Resources.Localization.Packages_Psf_FilesWithExt_Format, "*." + f1[0] + f2[0] + f3[0]);
            }
        }

        match = Regex.Match(regexp, @"^\^?\.*\\.([a-z0-9]+)");
        if (match.Success)
        {
            return string.Format(Resources.Localization.Packages_Psf_FilesWithExt_Format, "*." + match.Groups[1].Value);
        }

        return null;
    }
}