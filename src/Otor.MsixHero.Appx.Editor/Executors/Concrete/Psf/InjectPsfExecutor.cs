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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapplo.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Psf;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Helpers.Bitness;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Psf
{
    public class InjectPsfExecutor : ExtractedAppxExecutor<InjectPsf>, IValueChangedExecutor
    {
        private static readonly LogSource Logger = new();
        
        public InjectPsfExecutor(DirectoryInfo directory) : base(directory)
        {
        }

        public InjectPsfExecutor(string directory) : base(directory)
        {
        }

        public event EventHandler<CommandValueChanged> ValueChanged;
        
        public override async Task Execute(InjectPsf command, CancellationToken cancellationToken = default)
        {
            var manifest = Path.Combine(this.Directory.FullName, this.ResolvePath("AppxManifest.xml"));
            if (!File.Exists(manifest))
            {
                throw new FileNotFoundException("Manifest not found.");
            }

            Logger.Debug().WriteLine($"Opening manifest file {manifest}...");
            XDocument document;
            await using (var stream = File.OpenRead(manifest))
            {
                document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            }

            if (document.Root == null)
            {
                throw new ArgumentException("Invalid manifest file.");
            }

            var rootNamespace = document.Root.GetDefaultNamespace();
            var identityFullName = rootNamespace + "Applications";

            // ReSharper disable once PossibleNullReferenceException
            var applications = document.Root.Element(identityFullName);
            if (applications == null)
            {
                Logger.Warn().WriteLine("No <Applications /> element. Aborting.");
                return;
            }

            var redirections = new Dictionary<string, string>();
            var bitnessReader = new FileBitnessReader();

            var has32Bit = false;
            var has64Bit = false;

            var filterByApp = command.ApplicationIds?.Any() == true;
            var filterByName = command.FileNames?.Any() == true;

            IEnumerable<string> candidateStrings;

            if (filterByApp)
            {
                Logger.Info().WriteLine("Activating filtering by application ID...");
                candidateStrings = applications
                    .Elements(rootNamespace + "Application")
                    .Select(a => a.Attribute("Id")?.Value)
                    .Where(a => a != null);
            }
            else
            {
                Logger.Info().WriteLine("Activating filtering by application executable...");
                candidateStrings = applications
                    .Elements(rootNamespace + "Application")
                    .Select(a => a.Attribute("Executable")?.Value)
                    .Where(a => a != null);
            }

            var filteredStrings = GetMatches(filterByApp ? command.ApplicationIds : command.FileNames, candidateStrings).ToList();
            
            foreach (var app in applications.Elements(rootNamespace + "Application"))
            {
                var attrId = app.Attribute("Id");
                var attrEntryPoint = app.Attribute("EntryPoint");
                var attrExecutable = app.Attribute("Executable");

                if (string.IsNullOrEmpty(attrId?.Value))
                {
                    Logger.Debug().WriteLine("Ignoring empty ID.");
                    // Ignore if from some reason there was no ID.
                    continue;
                }

                if (string.IsNullOrEmpty(attrExecutable?.Value))
                {
                    Logger.Debug().WriteLine("Ignoring empty executable.");
                    // Ignore if no executable
                    continue;
                }

                if (filterByApp && !filteredStrings.Contains(attrId.Value))
                {
                    Logger.Debug().WriteLine($"Ignoring app {attrId.Value} because of filter settings.");
                    continue;
                }

                if (filterByName && !filteredStrings.Contains(attrExecutable.Value))
                {
                    Logger.Debug().WriteLine($"Ignoring app {attrId.Value} with file name {attrExecutable.Value} because of filter settings.");
                    continue;
                }

                if (!string.Equals("Windows.FullTrustApplication", attrEntryPoint?.Value, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Warn().WriteLine($"Ignoring app {attrId.Value}, because it is not win32 app.");
                    // Ignore non-win32
                    continue;
                }

                var hasPsf = attrExecutable.Value.ToLowerInvariant() switch
                {
                    "psflauncher.exe" => true,
                    "psflauncher32.exe" => true,
                    "psflauncher64.exe" => true,
                    _ => false
                };

                if (hasPsf && !command.Force)
                {
                    continue;
                }

                if (command.PsfLauncherSourcePath == null)
                {
                    var fullPath = Path.Combine(this.Directory.FullName, this.ResolvePath(attrExecutable.Value));
                    var bitness = await bitnessReader.GetBitness(fullPath, cancellationToken).ConfigureAwait(false);
                    has32Bit |= bitness != FileBitness.X64;
                    has64Bit |= bitness == FileBitness.X64;

                    var psfSuffix = bitness switch
                    {
                        FileBitness.X64 => "64",
                        _ => "32"
                    };

                    redirections[attrId.Value] = attrExecutable.Value;
                    attrExecutable.SetValue($"PsfLauncher{psfSuffix}.exe");
                }
                else
                {
                    var launcherName = Path.GetFileName(command.PsfLauncherSourcePath);
                    
                    if (string.Equals(attrExecutable.Value, launcherName, StringComparison.OrdinalIgnoreCase) && !command.Force)
                    {
                        continue;
                    }
                    
                    redirections[attrId.Value] = attrExecutable.Value;
                    attrExecutable.SetValue(launcherName);
                }
            }

            if (command.PsfLauncherSourcePath == null)
            {
                this.CopyPsfFiles(Path.GetDirectoryName(command.PsfLauncherSourcePath));
            }
            else
            {
                if (has64Bit)
                {
                    this.CopyPsfFiles(false);
                }

                if (has32Bit)
                {
                    this.CopyPsfFiles(true);
                }
            }

            var psfConfig = new JObject();
            var apps = new JArray();
            foreach (var item in redirections)
            {
                var jsonApp = new JObject();
                jsonApp["id"] = item.Key;
                jsonApp["executable"] = item.Value;
                apps.Add(jsonApp);
            }

            psfConfig["applications"] = apps;
            psfConfig["processes"] = new JArray();
            var psfConfigPath = Path.Combine(this.Directory.FullName, this.ResolvePath("config.json"));
            if (!File.Exists(psfConfigPath))
            {
                await File.WriteAllTextAsync(psfConfigPath, psfConfig.ToString(Formatting.Indented), Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            }

            await File.WriteAllTextAsync(manifest, document.ToString(SaveOptions.None), cancellationToken);
        }

        private static IEnumerable<string> GetMatches(IEnumerable<string> rules, IEnumerable<string> candidates)
        {
            if (candidates == null)
            {
                yield break;
            }

            // Logic:
            // 1) Empty string matches everything. Otherwise...
            // 2) String that starts and ends with / is a regular expression (assumed). Otherwise...
            // 3) String that contains * and ? is a wildcard. Otherwise...
            // 4) Finally, the string is treated literally.

            var regexpRules = new List<Regex>();

            foreach (var rule in rules)
            {
                if (rule.StartsWith('/') && rule.EndsWith('/'))
                {
                    regexpRules.Add(new Regex(rule.Substring(1, rule.Length - 2)));
                }
                else
                {
                    regexpRules.Add(RegexBuilder.FromWildcard(rule));
                }
            }

            foreach (var candidate in candidates)
            {
                if (regexpRules.Any(r => r.IsMatch(candidate)))
                {
                    yield return candidate;
                }
            }
        }

        private void CopyPsfFiles(bool use32Bit)
        {
            this.CopyPsfFiles(SdkPathHelper.GetPsfDirectory(use32Bit));
        }

        private void CopyPsfFiles(string psfDirectory)
        {
            foreach (var source in System.IO.Directory.EnumerateFiles(psfDirectory))
            {
                var target = new FileInfo(Path.Combine(this.Directory.FullName, Path.GetFileName(source)));

                if (target.Directory?.Exists == false)
                {
                    target.Directory.Create();
                }

                File.Copy(source, target.FullName);
            }
        }
    }
}
