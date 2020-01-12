using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Appx.Psf;
using Registry;

namespace otor.msixhero.lib.BusinessLayer.Appx
{
    public class PsfReader
    {
        public PsfApplicationDefinition Read(string applicationId, string packageRootFolder)
        {
            if (!System.IO.Directory.Exists(packageRootFolder))
            {
                throw new ArgumentException("Package folder does not exist.");
            }

            var configJson = Path.Combine(packageRootFolder, "config.json");

            using var fs = File.OpenRead(configJson);
            using TextReader tr = new StreamReader(fs);
            return this.Read(applicationId, tr);
        }

        public PsfApplicationDefinition Read(string applicationId, IAppxFileReader fileReader)
        {
            if (fileReader.FileExists("config.json"))
            {
                using (var stream = fileReader.GetFile("config.json"))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        return this.Read(applicationId, streamReader);
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

        public PsfApplicationDefinition Read(string applicationId, TextReader configJson)
        {
            var jsonConvert = JObject.Parse(configJson.ReadToEnd());

            var applications = jsonConvert["applications"];
            if (applications == null)
            {
                return null;
            }

            string executable = null;
            string workingDirectory = null;
            string arguments = null;
            foreach (var item in applications)
            {
                var id = item["id"];
                if (id != null && id.Value<string>() == applicationId)
                {
                    executable = item["executable"]?.Value<string>();
                    workingDirectory = item["workingDirectory"]?.Value<string>();
                    arguments = item["arguments"]?.Value<string>();
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

            var processes = jsonConvert["processes"];
            if (processes == null)
            {
                return psfDef;
            }
            
            foreach (var p in processes)
            {
                var exe = p["executable"];
                if (string.IsNullOrEmpty(exe?.Value<string>()))
                {
                    continue;
                }

                Regex regex;
                try
                {
                    regex = new Regex(exe?.Value<string>());
                }
                catch (Exception)
                {
                    continue;
                }

                var indexOf = executable.LastIndexOfAny(new char[] { '\\', '/' });
                if (indexOf != -1)
                {
                    executable = executable.Substring(indexOf + 1);
                }

                if (!regex.IsMatch(executable))
                {
                    continue;
                }

                var fixups = p["fixups"];
                if (fixups == null)
                {
                    continue;
                }

                foreach (var fixup in fixups.Children())
                {
                    var dll = fixup["dll"]?.Value<string>();
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
                                psfDef.Tracing = PsfBitness.Neutral;
                            }
                            else
                            {
                                psfDef.Tracing |= PsfBitness.Neutral;
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

        private List<PsfFileRedirection> GetFileRedirections(JToken jsonConvert)
        {
            var list = new List<PsfFileRedirection>();

            var config = jsonConvert["config"];
            if (config == null)
            {
                return list;
            }

            var redirectedPath = config["redirectedPaths"];
            if (redirectedPath == null)
            {
                return list;
            }

            var packageRelative = redirectedPath["packageRelative"];
            if (packageRelative != null)
            {
                foreach (var packageRelativeItem in packageRelative.Children())
                {
                    var valueBase = packageRelativeItem["base"]?.Value<string>();
                    var patterns = packageRelativeItem["patterns"];
                    var valueIsExclusion = packageRelativeItem["IsExclusion"]?.Value<bool>();

                    if (patterns != null)
                    {
                        foreach (var patternItem in patterns.Children())
                        {
                            var def = new PsfFileRedirection
                            {
                                Directory = valueBase,
                                RegularExpression = patternItem.Value<string>(),
                                IsExclusion = valueIsExclusion == true
                            };

                            list.Add(def);
                        }
                    }
                }
            }

            var packageDriveRelative = redirectedPath["packageDriveRelative"];
            if (packageDriveRelative != null)
            {
                foreach (var packageDriveRelativeItem in packageDriveRelative.Children())
                {
                    var valueBase = packageDriveRelativeItem["base"]?.Value<string>();
                    var patterns = packageDriveRelativeItem["patterns"];
                    var valueIsExclusion = packageDriveRelativeItem["IsExclusion"]?.Value<bool>();

                    foreach (var patternItem in patterns.Children())
                    {
                        var def = new PsfFileRedirection
                        {
                            Directory = valueBase,
                            RegularExpression = patternItem.Value<string>(),
                            IsExclusion = valueIsExclusion == true
                        };

                        list.Add(def);
                    }
                }
            }

            var knownFolders = redirectedPath["knownFolders"];
            if (knownFolders != null)
            {
                foreach (var knownFolderItem in knownFolders.Children())
                {
                    var id = knownFolderItem["id"]?.Value<string>();
                    var relativePaths = knownFolderItem["relativePaths"];
                    var valueIsExclusion = knownFolderItem["IsExclusion"]?.Value<bool>();

                    if (relativePaths != null)
                    {
                        foreach (var relativePathItem in relativePaths.Children())
                        {
                            var valueBase = relativePathItem["base"]?.Value<string>();
                            var patterns = relativePathItem["patterns"];
                            if (patterns != null)
                            {
                                foreach (var paternItem in patterns)
                                {
                                    var def = new PsfFileRedirection
                                    {
                                        Directory = id + "\\" + valueBase,
                                        IsExclusion = valueIsExclusion == true,
                                        RegularExpression = paternItem.Value<string>()
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
