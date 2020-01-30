using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Appx.Psf;
using Registry;

namespace otor.msixhero.lib.BusinessLayer.Appx
{
    public class PsfReader
    {
        public PsfApplicationDefinition Read(string applicationId, string packageRootFolder)
        {
            if (!Directory.Exists(packageRootFolder))
            {
                throw new ArgumentException("Package folder does not exist.");
            }

            using (IAppxFileReader fileReader = new DirectoryInfoFileReaderAdapter(new DirectoryInfo(packageRootFolder)))
            {
                return this.Read(applicationId, fileReader);
            }
        }

        public PsfApplicationDefinition Read(string applicationId, IAppxFileReader fileReader)
        {
            if (fileReader.FileExists("config.json"))
            {
                using (var stream = fileReader.GetFile("config.json"))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        return this.Read(applicationId, fileReader, streamReader);
                    }
                }
            }

            if (fileReader.FileExists(@"AI_STUBS\AiStub.exe"))
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
                        var psfDef = new PsfApplicationDefinition();

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

            return null;
        }

        private PsfApplicationDefinition Read(string applicationId, IAppxFileReader fileReader, TextReader configJson)
        {
            var jsonSerializer = new PsfConfigSerializer();
            var config = jsonSerializer.Deserialize(configJson.ReadToEnd());

            string executable = null;
            string workingDirectory = null;
            string arguments = null;
            foreach (var item in config.Applications ?? Enumerable.Empty<PsfApplication>())
            {
                var id = item.Id;
                if (id == applicationId)
                {
                    executable = item.Executable;
                    workingDirectory = item.WorkingDirectory;
                    arguments = item.Arguments;
                    break;
                }
            }

            if (executable == null)
            {
                return null;
            }

            var psfDef = new PsfApplicationDefinition
            {
                Executable = executable,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                OtherFixups = new List<string>(),
                FileRedirections = new List<PsfFileRedirection>(),
                Tracing = null,
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
                    continue;
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
                            if (psfDef.Tracing == null)
                            {
                                psfDef.Tracing = PsfBitness.x64;
                            }
                            else
                            {
                                psfDef.Tracing |= PsfBitness.x64;
                            }

                            break;
                        case "tracefixup32.dll":
                            if (psfDef.Tracing == null)
                            {
                                psfDef.Tracing = PsfBitness.x86;
                            }
                            else
                            {
                                psfDef.Tracing |= PsfBitness.x86;
                            }

                            break;
                        case "tracefixup.dll":
                            if (psfDef.Tracing == null)
                            {
                                psfDef.Tracing = fileReader.FileExists("tracefixup32.dll") ? PsfBitness.x86 : PsfBitness.x64;
                            }
                            else
                            {
                                psfDef.Tracing |= fileReader.FileExists("tracefixup32.dll") ? PsfBitness.x86 : PsfBitness.x64;
                            }

                            break;
                        case "fileredirectionfixup64.dll":
                        case "fileredirectionfixup32.dll":
                        case "fileredirectionfixup.dll":
                            
                            var redirs = this.GetFileRedirections(fixup);
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

        private List<PsfFileRedirection> GetFileRedirections(PsfFixup jsonConvert)
        {
            var list = new List<PsfFileRedirection>();

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
                    var patterns = packageRelativeItem.Patterns;
                    var valueIsExclusion = packageRelativeItem.IsExclusion;

                    if (patterns != null)
                    {
                        foreach (var patternItem in patterns)
                        {
                            var def = new PsfFileRedirection
                            {
                                Directory = valueBase,
                                RegularExpression = patternItem,
                                IsExclusion = valueIsExclusion
                            };

                            list.Add(def);
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
                    var patterns = packageDriveRelativeItem.Patterns;
                    var valueIsExclusion = packageDriveRelativeItem.IsExclusion;

                    foreach (var patternItem in patterns ?? Enumerable.Empty<string>())
                    {
                        var def = new PsfFileRedirection
                        {
                            Directory = valueBase,
                            RegularExpression = patternItem,
                            IsExclusion = valueIsExclusion
                        };

                        list.Add(def);
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
                            var valueIsExclusion = relativePathItem.IsExclusion;
                            var valueBase = relativePathItem.Base;
                            var patterns = relativePathItem.Patterns;
                            if (patterns != null)
                            {
                                foreach (var paternItem in patterns)
                                {
                                    var def = new PsfFileRedirection
                                    {
                                        Directory = id + "\\" + valueBase,
                                        IsExclusion = valueIsExclusion,
                                        RegularExpression = paternItem
                                    };

                                    list.Add(def);
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }
    }
}
