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
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedAppContainer.ViewModel
{
    public class SharedAppContainerViewModel : ChangeableAutomatedDialogViewModel<AppAttachVerb>, IDropTarget
    {
        private readonly IInteractionService _interactionService;
        private readonly IAppxPackageQuery _packageQuery;
        private SharedAppViewModel _selectedApp;

        public SharedAppContainerViewModel(
            IInteractionService interactionService,
            IAppxPackageQuery packageQuery) : base("Create shared app container definition", interactionService)
        {
            _interactionService = interactionService;
            _packageQuery = packageQuery;
            this.AddChildren(
                this.Name = new ValidatedChangeableProperty<string>("Container name", true, ValidatorFactory.ValidateNotEmptyField()),
                this.CreationMode = new ChangeableProperty<CreationMode>()
            );

            this.RegisterForCommandLineGeneration(this.Name, this.CreationMode);
            this.CreationMode.ValueChanged += this.CreationModeOnValueChanged;

            this.Add = new DelegateCommand(this.OnAdd, this.CanAdd);
            this.Remove = new DelegateCommand(this.OnRemove, this.CanRemove);
            this.Open = new DelegateCommand(this.OnOpen, this.CanOpen);

            this.CustomValidation += this.OnCustomValidation;
        }

        private void OnCustomValidation(object sender, ContainerValidationArgs e)
        {
            if (!this.Apps.Any())
            {
                e.SetError("At least one app is required.");
            }
        }

        public ICommand Add { get; }

        public ICommand Remove { get; }

        public ICommand Open { get; }

        public ValidatedChangeableCollection<SharedAppViewModel> Apps { get; } = new();

        public SharedAppViewModel SelectedApp
        {
            get => this._selectedApp;
            set => this.SetField(ref this._selectedApp, value);
        }

        public ValidatedChangeableProperty<string> Name { get; }

        public ChangeableProperty<CreationMode> CreationMode { get; }

        public string OkPrompt
        {
            get
            {
                switch (this.CreationMode.CurrentValue)
                {
                    case ViewModel.CreationMode.Xml:
                        return "Create XML file...";
                    case ViewModel.CreationMode.Install:
                        return "Install";
                    default:
                        return Resources.Localization.Dialogs_OK;
                }
            }
        }

        public bool AdminRightsRequired => this.CreationMode.CurrentValue == ViewModel.CreationMode.Install;
        
        public IAppxPackageQuery PackageQuery => this._packageQuery;

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }

            var dialogFilter = new DialogFilterBuilder()
                .WithExtension(".xml", "Shared App Containers")
                .WithAll();

            var opts = new FileDialogSettings(dialogFilter);

            if (!string.IsNullOrEmpty(this.Name.CurrentValue))
            {
                opts.InitialSelection = Regex.Replace(this.Name.CurrentValue, @"[^a-zA-Z_\- .]", string.Empty).ToLowerInvariant() + ".xml";
            }

            if (!this._interactionService.SaveFile(opts, out var path))
            {
                return false;
            }

            var sharedContainer = new Appx.Packaging.SharedAppContainer.SharedAppContainerBuilder(this.Name.CurrentValue);

            foreach (var item in this.Apps)
            {
                sharedContainer.AddFamilyName(item.FamilyName.CurrentValue);
            }

            if (this.CreationMode.CurrentValue == ViewModel.CreationMode.Install)
            {
                throw new NotImplementedException("not there yet.");
            }

            var xml = sharedContainer.ToXml();

            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            else if (fileInfo.Directory?.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            await using var stream = File.OpenWrite(path);
            await using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            await streamWriter.WriteAsync(new StringBuilder(xml), cancellationToken);
            return true;
        }

        protected override void UpdateVerbData()
        {
            this.Verb.JunctionPoint = "C:\\temp\\" + Guid.NewGuid().ToString("N").Substring(0, 4);
            this.Verb.CreateScript = Guid.NewGuid().GetHashCode() % 2 == 1;
            this.Verb.ExtractCertificate = Guid.NewGuid().GetHashCode() % 2 == 1;
            this.Verb.FileType = (AppAttachVolumeType)(Guid.NewGuid().GetHashCode() % 3);
            this.Verb.Directory = "C:\\output\\" + Guid.NewGuid().ToString("N").Substring(0, 4);
            this.Verb.Size = (uint)Math.Abs(Guid.NewGuid().GetHashCode() / 1000);
            this.Verb.Package = new List<string>() { Guid.NewGuid().ToString("N").Substring(0, 4) + ".msix" };
        }

        private void OnAdd()
        {
            this.Apps.Add(SharedAppViewModel.Create(this._packageQuery, "MSIXHero_zxq1da1qqbeze"));

            if (this.Apps.Count == 1)
            {
                this.SelectedApp = this.Apps.Last();

                if (string.IsNullOrEmpty(this.Name.CurrentValue))
                {
                    this.Name.CurrentValue = this.Apps.Last().DisplayName ?? this.Apps.Last().FamilyName.CurrentValue.Split('_').First();
                }
            }
        }

        private void OnOpen()
        {
            var filterBuilder = new DialogFilterBuilder()
                .WithPackages(DialogFilterBuilderPackagesExtensions.PackageTypes.Msix)
                .WithAll();

            var opts = new FileDialogSettings(filterBuilder);
            
            if (!this._interactionService.SelectFiles(opts, out var selections))
            {
                return;
            }

            var failed = new List<string>();

            foreach (var selection in selections)
            {
                var newApp = new SharedAppViewModel();

                if (newApp.SetFromFilePath(selection, CancellationToken.None).GetAwaiter().GetResult())
                {
                    this.Apps.Add(newApp);
                    this.SelectedApp = newApp;
                }
                else
                {
                    failed.Add(selection);
                }
            }

            if (!failed.Any())
            {
                if (this.Apps.Count == 1)
                {
                    this.SelectedApp = this.Apps.Last();
                }

                if (string.IsNullOrEmpty(this.Name.CurrentValue))
                {
                    this.Name.CurrentValue = this.Apps.Last().DisplayName ?? this.Apps.Last().FamilyName.CurrentValue.Split('_').First();
                }

                return;
            }

            if (selections.Length == 1)
            {
                this._interactionService.ShowError("The selected package could not be read.");
            }
            else
            {
                this._interactionService.ShowError("Some package(s) could not be opened.");
            }
        }

        private void OnRemove()
        {
            if (this.SelectedApp != null)
            {
                var currentSelectionIndex = this.Apps.IndexOf(this.SelectedApp);
                if (currentSelectionIndex + 1 < this.Apps.Count)
                {
                    this.SelectedApp = this.Apps[currentSelectionIndex + 1];
                }
                else if (currentSelectionIndex > 0)
                {
                    this.SelectedApp = this.Apps[currentSelectionIndex - 1];
                }
                else
                {
                    this.SelectedApp = null;
                }

                this.Apps.RemoveAt(currentSelectionIndex);
            }
        }

        private bool CanAdd() => true;

        private bool CanRemove() => this.SelectedApp != null;

        private bool CanOpen() => true;

        private void CreationModeOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(AdminRightsRequired));
            this.OnPropertyChanged(nameof(OkPrompt));
        }


        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SharedAppViewModel;
            var targetItem = dropInfo.TargetItem as SharedAppViewModel;

            if (sourceItem == null || targetItem == null || sourceItem == targetItem)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            var indexOfSource = this.Apps.IndexOf(sourceItem);
            var indexOfTarget = this.Apps.IndexOf(targetItem);

            switch (dropInfo.InsertPosition)
            {
                case RelativeInsertPosition.BeforeTargetItem:
                    if (indexOfSource + 1 == indexOfTarget)
                    {
                        return;
                    }

                    break;
                case RelativeInsertPosition.AfterTargetItem:
                    if (indexOfSource == indexOfTarget + 1)
                    {
                        return;
                    }

                    break;
                default:
                    return;
            }


            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.DestinationText = string.Format(Resources.Localization.Dialogs_Settings_Tools_DragDrop_Hint, sourceItem.DisplayName);
            dropInfo.EffectText = dropInfo.DestinationText;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            switch (dropInfo.InsertPosition)
            {
                case RelativeInsertPosition.BeforeTargetItem:
                    this.DropBefore(dropInfo);
                    break;
                case RelativeInsertPosition.AfterTargetItem:
                    this.DropAfter(dropInfo);
                    break;
            }
        }

        private void DropBefore(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SharedAppViewModel;
            var targetItem = dropInfo.TargetItem as SharedAppViewModel;

            if (sourceItem == null || targetItem == null)
            {
                return;
            }

            if (sourceItem == targetItem)
            {
                return;
            }

            var indexOfTarget = this.Apps.IndexOf(targetItem);
            var indexOfSource = this.Apps.IndexOf(sourceItem);

            if (indexOfSource + 1 == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                this.Apps.Insert(indexOfTarget, sourceItem);
                this.Apps.RemoveAt(indexOfSource);
            }
            else
            {
                this.Apps.RemoveAt(indexOfSource);
                this.Apps.Insert(indexOfTarget, sourceItem);
            }

            this.SelectedApp = sourceItem;
        }

        private void DropAfter(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SharedAppViewModel;
            var targetItem = dropInfo.TargetItem as SharedAppViewModel;

            if (sourceItem == null || targetItem == null)
            {
                return;
            }

            if (sourceItem == targetItem)
            {
                return;
            }

            var indexOfTarget = this.Apps.IndexOf(targetItem) + 1;
            var indexOfSource = this.Apps.IndexOf(sourceItem);

            if (indexOfSource == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                this.Apps.Insert(indexOfTarget, sourceItem);
                this.Apps.RemoveAt(indexOfSource);
            }
            else
            {
                this.Apps.RemoveAt(indexOfSource);
                this.Apps.Insert(indexOfTarget, sourceItem);
            }

            this.SelectedApp = sourceItem;
        }

    }

    public enum CreationMode
    {
        Xml,
        Install
    }
}

