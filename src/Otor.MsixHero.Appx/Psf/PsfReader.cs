using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using External.EricZimmerman.Registry;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.Appx.Psf
{
    public class PsfReader
    {
        public PsfApplicationDescriptor Read(string applicationId, string originalEntryPoint, string packageRootFolder)
        {
            if (!Directory.Exists(packageRootFolder))
            {
                throw new ArgumentException("Package folder does not exist.");
            }

            using (IAppxFileReader fileReader = new DirectoryInfoFileReaderAdapter(new DirectoryInfo(packageRootFolder)))
            {
                return this.Read(applicationId, originalEntryPoint, fileReader);
            }
        }
        public PsfApplicationDescriptor Read(string applicationId, string packageRootFolder)
        {
            if (!Directory.Exists(packageRootFolder))
            {
                throw new ArgumentException("Package folder does not exist.");
            }

            return this.Read(applicationId, null, packageRootFolder);
        }

        public PsfApplicationDescriptor Read(string applicationId, string originalEntryPoint, IAppxFileReader fileReader)
        {
            if (
                string.Equals(originalEntryPoint, @"AI_STUBS\AiStub.exe", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(originalEntryPoint, @"AI_STUBS\AiStubElevated.exe", StringComparison.OrdinalIgnoreCase))
            {

                if (fileReader.FileExists(@"AI_STUBS\AiStub.exe") || fileReader.FileExists(@"AI_STUBS\AiStubElevated.exe"))
                {
                    // This is an old Advanced Installer stuff
                    if (fileReader.FileExists("Registry.dat"))
                    {
                        RegistryHiveOnDemand reg;
                        using (var stream = fileReader.GetFile("Registry.dat"))
                        {
                            if (stream is FileStream fileStream)
                            {
                                reg = new RegistryHiveOnDemand(fileStream.Name);
                            }
                            else
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream);
                                    memoryStream.Flush();
                                    reg = new RegistryHiveOnDemand(memoryStream.ToArray(), "Registry.dat");
                                }
                            }
                        }

                        var key = reg.GetKey(@"root\registry\machine\software\caphyon\advanced installer\" + applicationId);
                        if (key?.Values != null)
                        {
                            var psfDef = new PsfApplicationDescriptor();

                            foreach (var item in key.Values.Where(item => item.ValueName != null))
                            {
                                switch (item.ValueName.ToLowerInvariant())
                                {
                                    case "path":
                                        psfDef.Executable = (item.ValueData ?? string.Empty).Replace("[{AppVPackageRoot}]\\", string.Empty);
                                        break;
                                    case "pathai":
                                        psfDef.Executable = (item.ValueData ?? string.Empty).Replace("[{AppVPackageRoot}]\\", string.Empty);
                                        break;
                                    case "workingdirectory":
                                        psfDef.WorkingDirectory = (item.ValueData ?? string.Empty).Replace("[{AppVPackageRoot}]\\", string.Empty);
                                        break;
                                    case "workingdirectoryai":
                                        psfDef.WorkingDirectory = (item.ValueData ?? string.Empty).Replace("[{AppVPackageRoot}]\\", string.Empty);
                                        break;
                                    case "args":
                                        psfDef.Arguments = item.ValueData;
                                        break;
                                }
                            }

                            if (string.IsNullOrWhiteSpace(psfDef.Executable))
                            {
                                psfDef.Executable = null;
                            }
                            else if (psfDef.Executable.StartsWith("[{", StringComparison.OrdinalIgnoreCase))
                            {
                                var indexOfClosing = psfDef.Executable.IndexOf("}]", StringComparison.OrdinalIgnoreCase);
                                if (indexOfClosing != -1)
                                {
                                    var middlePart = psfDef.Executable.Substring(2, indexOfClosing - 2);
                                    var testedPath = "VFS\\" + middlePart + psfDef.Executable.Substring(indexOfClosing + 2);

                                    if (fileReader.FileExists(testedPath))
                                    {
                                        // this is to make sure that a path like [{ProgramFilesX86}]\test is replaced to VFS\ProgramFilesX86\test if present
                                        psfDef.Executable = testedPath;
                                    }
                                }
                            }

                            if (string.IsNullOrWhiteSpace(psfDef.WorkingDirectory))
                            {
                                psfDef.WorkingDirectory = null;
                            }

                            if (string.IsNullOrWhiteSpace(psfDef.Arguments))
                            {
                                psfDef.Arguments = null;
                            }
                            
                            return psfDef;
                        }
                    }
                }
            }

            var dir = Path.GetDirectoryName(originalEntryPoint);
            string configJson;
            if (string.IsNullOrEmpty(dir))
            {
                configJson = "config.json";
            }
            else
            {
                configJson = Path.Combine(dir, "config.json");
            }

            if (fileReader.FileExists(configJson))
            {
                using (var stream = fileReader.GetFile(configJson))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        return this.Read(applicationId, originalEntryPoint, streamReader);
                    }
                }
            }

            return null;
        }

        private PsfApplicationDescriptor Read(string applicationId, string originalEntryPoint, TextReader configJson)
        {
            var jsonSerializer = new PsfConfigSerializer();
            var config = jsonSerializer.Deserialize(configJson.ReadToEnd());

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

            var psfDef = new PsfApplicationDescriptor
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
                        valueBase = "<PackageDirectory>";
                    }
                    else
                    {
                        valueBase = Path.Join("<PackageDirectory>", valueBase).TrimEnd('\\');
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
                        valueBase = "<PackageDrive>";
                    }
                    else
                    {
                        valueBase = Path.Join("<PackageDrive>", valueBase).TrimEnd('\\');
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

            // make sure the list is sorted from the less to the most generic items...
            list = list.OrderBy(x => x.Directory.Split('\\').Length).ToList();

            return list;
        }

        private static string GetNiceRegExpDescription(string regexp)
        {
            if (string.IsNullOrEmpty(regexp) || regexp == ".*")
            {
                return "All files";
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
                    return "Files with extension *." + f1[0] + f2[0] + f3[0];
                }
            }

            match = Regex.Match(regexp, @"^\^?\.*\\.([a-z0-9]+)");
            if (match.Success)
            {
                return "Files with extension *." + match.Groups[1].Value;
            }

            return null;
        }
    }
}
