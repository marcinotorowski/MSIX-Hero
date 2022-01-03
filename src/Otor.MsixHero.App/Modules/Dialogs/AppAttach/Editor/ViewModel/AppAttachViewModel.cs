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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.AppAttach.Editor.ViewModel
{
    public class AppAttachViewModel : ChangeableAutomatedDialogViewModel<AppAttachVerb>, IDialogAware
    {
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<IAppAttachManager> appAttachManagerFactory;
        private ICommand reset;
        
        public AppAttachViewModel(ISelfElevationProxyProvider<IAppAttachManager> appAttachManagerFactory, IInteractionService interactionService) : base("Prepare volume for app attach", interactionService)
        {
            this.appAttachManagerFactory = appAttachManagerFactory;
            this.interactionService = interactionService;
            
            this.Files = new ValidatedChangeableCollection<string>(this.ValidateFiles);
            this.Files.CollectionChanged += (_, _) =>
            {
                this.OnPropertyChanged(nameof(IsOnePackage));
            };

            this.TabPackages = new ChangeableContainer(this.Files);
            this.TabOptions = new ChangeableContainer(
                this.ExtractCertificate = new ChangeableProperty<bool>(),
                this.GenerateScripts = new ChangeableProperty<bool>(true),
                this.VolumeType = new ChangeableProperty<AppAttachVolumeType>(),
                this.SizeMode = new ChangeableProperty<AppAttachSizeMode>(),
                this.FixedSize = new ValidatedChangeableProperty<string>("Fixed size", "100", this.ValidateFixedSize)
            );
            
            this.AddChildren(this.TabPackages, this.TabOptions);
            this.RegisterForCommandLineGeneration(this.Files, this.GenerateScripts, this.VolumeType, this.ExtractCertificate, this.FixedSize, this.SizeMode);
        }

        public void AddPackage(string packagePath = null)
        {
            string[] selection;
            if (string.IsNullOrEmpty(packagePath))
            {
                var interactionResult = this.interactionService.SelectFiles(FileDialogSettings.FromFilterString(new DialogFilterBuilder("*" + FileConstants.MsixExtension).BuildFilter()), out selection);
                if (!interactionResult || !selection.Any())
                {
                    return;
                }
            }
            else
            {
                selection = new[] { packagePath };
            }

            foreach (var selected in selection)
            {
                if (this.Files.Contains(selected, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                this.Files.Add(selected);
            }
        }

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Path", out var file))
            {
                this.Files.Add(file);
            }
            else
            {
                return;
            }

            this.Files.Commit();
        }

        public ChangeableContainer TabPackages { get; }
        
        public ChangeableContainer TabOptions { get; }

        public ICommand ResetCommand
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public bool IsOnePackage => this.Files.Count == 1;

        public List<string> SelectedPackages { get; } = new List<string>();

        public async Task<int> ImportFolder()
        {
            if (!this.interactionService.SelectFolder(out var folder))
            {
                return 0;
            }

            var hasMsixFiles = Directory.EnumerateFiles(folder, "*" + FileConstants.MsixExtension).Any();
            var hasSubfolders = Directory.EnumerateDirectories(folder).Any();

            var recurse = !hasMsixFiles;

            if (!recurse && hasSubfolders)
            {
                IReadOnlyCollection<string> buttons = new List<string>
                {
                    "Only selected folder",
                    "Selected folder " + Path.GetFileName(folder) + " and all its subfolders"
                };

                var userChoice = this.interactionService.ShowMessage("The selected folder contains *" + FileConstants.MsixExtension + " file(s) and subfolders. Do you want to import all *.msix files, also including subfolders?", buttons, systemButtons: InteractionResult.Cancel);
                if (userChoice < 0 || userChoice >= buttons.Count)
                {
                    return 0;
                }

                recurse = userChoice == 1;
            }

            var files = await Task.Run(() => Directory.EnumerateFiles(folder, "*" + FileConstants.MsixExtension, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList()).ConfigureAwait(true);

            if (!files.Any())
            {
                this.interactionService.ShowError("The selected folder contains no MSIX packages.");
                return 0;
            }

            var cnt = this.Files.Count;
            this.Files.AddRange(files.Except(this.Files, StringComparer.OrdinalIgnoreCase));

            return this.Files.Count - cnt;
        }

        protected override void UpdateVerbData()
        {
            if (!this.Files.Any())
            {
                this.Verb.Package = new[] { "<path-to-msix>" };
            }
            else
            {
                this.Verb.Package = this.Files;
            }

            this.Verb.FileType = this.VolumeType.CurrentValue;
            this.Verb.CreateScript = this.GenerateScripts.CurrentValue;
            this.Verb.ExtractCertificate = this.ExtractCertificate.CurrentValue;
            
            if (this.Files.Count == 1)
            {
                if (this.SizeMode.CurrentValue == AppAttachSizeMode.Auto)
                {
                    this.Verb.Size = 0;
                }
                else
                {
                    this.Verb.Size = !uint.TryParse(this.FixedSize.CurrentValue, out var parsed) ? 0 : parsed;
                }
            }
            else
            {
                this.Verb.Size = 0;
            }
            
            if (string.IsNullOrEmpty(this.OutputDirectory))
            {
                this.Verb.Directory = "<output-directory>";
            }
            else
            {
                this.Verb.Directory = this.OutputDirectory;
            }
        }

        public ValidatedChangeableCollection<string> Files { get; }

        public ChangeableProperty<bool> GenerateScripts { get; }

        public ChangeableProperty<bool> ExtractCertificate { get; }
        
        public ChangeableProperty<AppAttachVolumeType> VolumeType { get; }
        
        public ValidatedChangeableProperty<string> FixedSize { get; }

        public ChangeableProperty<AppAttachSizeMode> SizeMode { get; }

        public string OutputDirectory { get; private set; }
        
        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }
            
            string fileName = null;
            
            if (this.Files.Count == 1)
            {
                string filter;
                switch (this.VolumeType.CurrentValue)
                {
                    case AppAttachVolumeType.Vhd:
                        filter = "Virtual disks|*.vhd";
                        break;
                    case AppAttachVolumeType.Cim:
                        filter = "CIM files|*.cim";
                        break;
                    case AppAttachVolumeType.Vhdx:
                        filter = "Virtual disks|*.vhdx";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var settings = 
                    this.OutputDirectory != null
                        ? new FileDialogSettings(filter, Path.Combine(this.OutputDirectory, Path.GetFileName(this.Files[0]) + "." + this.VolumeType.CurrentValue.ToString("G").ToLowerInvariant()))
                        : new FileDialogSettings(filter);
                
                if (!this.interactionService.SaveFile(settings, out var output))
                {
                    return false;
                }

                this.OutputDirectory = Path.GetDirectoryName(output);
                fileName = Path.GetFileName(output);
            }
            else
            {
                if (!this.interactionService.SelectFolder(out var output))
                {
                    return false;
                }

                this.OutputDirectory = output;
            }

            var sizeInMegabytes = this.SizeMode.CurrentValue == AppAttachSizeMode.Auto ? 0 : uint.Parse(this.FixedSize.CurrentValue);

            var appAttach = await this.appAttachManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (this.Files.Count == 1)
            {
                await appAttach.CreateVolume(
                    this.Files[0],
                    // ReSharper disable AssignNullToNotNullAttribute
                    Path.Combine(this.OutputDirectory, fileName),
                    // ReSharper restore AssignNullToNotNullAttribute
                    sizeInMegabytes,
                    this.VolumeType.CurrentValue,
                    this.ExtractCertificate.CurrentValue,
                    this.GenerateScripts.CurrentValue,
                    cancellationToken,
                    progress).ConfigureAwait(false);
            }
            else
            {
                await appAttach.CreateVolumes(
                    this.Files,
                    this.OutputDirectory,
                    this.VolumeType.CurrentValue,
                    this.ExtractCertificate.CurrentValue,
                    this.GenerateScripts.CurrentValue,
                    cancellationToken,
                    progress).ConfigureAwait(false);
            }
            
            return true;
        }

        private void ResetExecuted()
        {
            // this.Files.Clear();
            this.Commit();
            this.State.IsSaved = false;
        }

        private string ValidateFiles(IEnumerable<string> files)
        {
            if (!files.Any())
            {
                return "At least one file is required.";
            }

            return null;
        }

        private string ValidateFixedSize(string value)
        {
            if (this.SizeMode.CurrentValue == AppAttachSizeMode.Auto)
            {
                return null;
            }

            if (string.IsNullOrEmpty(value))
            {
                return "Fixed size cannot be empty.";
            }

            if (!int.TryParse(value, out var parsed))
            {
                return "Fixed size must ba a number";
            }

            return parsed <= 0 ? "Fixed size must be a positive number" : null;
        }
    }
}

