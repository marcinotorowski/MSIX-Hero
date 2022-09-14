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
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Builder;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Exceptions;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.ViewModel
{
    public class SharedPackageContainerViewModel : ChangeableAutomatedDialogViewModel<SharedPackageContainerVerb>, IDropTarget
    {
        private readonly IUacElevation _uacElevation;
        private readonly IInteractionService _interactionService;
        private readonly IAppxPackageQueryService _packageQueryService;
        private SharedPackageViewModel _selectedPackage;

        public SharedPackageContainerViewModel(
            IMsixHeroApplication application,
            IUacElevation uacElevation,
            IInteractionService interactionService,
            IAppxPackageQueryService packageQueryService,
            IBusyManager busyManager) : base(Resources.Localization.Dialogs_SharedContainer_Title, interactionService)
        {
            this._uacElevation = uacElevation;
            this._interactionService = interactionService;
            this._packageQueryService = packageQueryService;
            this.AddChildren(
                this.Name = new ValidatedChangeableProperty<string>(Resources.Localization.Dialogs_SharedContainer_ContainerName, true, ValidatorFactory.ValidateNotEmptyField()),
                this.CreationMode = new ChangeableProperty<CreationMode>(),
                this.Packages = new ValidatedChangeableCollection<SharedPackageViewModel>()
            );

            this.RegisterForCommandLineGeneration(this.Name, this.CreationMode, this.Packages, this.Output, this.Resolution);
            this.CreationMode.ValueChanged += this.CreationModeOnValueChanged;

            this.Add = new DelegateCommand(this.OnAdd, this.CanAdd);
            this.Remove = new DelegateCommand(this.OnRemove, this.CanRemove);
            this.Open = new DelegateCommand(this.OnOpen, this.CanOpen);
            
            this.CustomValidation += this.OnCustomValidation;
            this.InstalledPackages = new InstalledPackages(this, application, busyManager);
        }

        public InstalledPackages InstalledPackages { get; }

        private void OnCustomValidation(object sender, ContainerValidationArgs e)
        {
            if (!this.Packages.Any())
            {
                e.SetError("At least one package is required.");
            }
        }

        public ICommand Add { get; }

        public ICommand Remove { get; }

        public ICommand Open { get; }

        public ValidatedChangeableCollection<SharedPackageViewModel> Packages { get; }

        public SharedPackageViewModel SelectedPackage
        {
            get => this._selectedPackage;
            set => this.SetField(ref this._selectedPackage, value);
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
                        return Resources.Localization.Dialogs_SharedContainer_Button_Xml;
                    case ViewModel.CreationMode.Deploy:
                        return Resources.Localization.Dialogs_SharedContainer_Button_Deploy;
                    default:
                        return Resources.Localization.Dialogs_OK;
                }
            }
        }

        public bool AdminRightsRequired => this.CreationMode.CurrentValue == ViewModel.CreationMode.Deploy;
        
        public IAppxPackageQueryService PackageQueryService => this._packageQueryService;

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }

            switch (this.CreationMode.CurrentValue)
            {
                case ViewModel.CreationMode.Xml:
                    return await this.SaveXml(cancellationToken, progress).ConfigureAwait(false);
                    
                case ViewModel.CreationMode.Deploy:
                    if (!this._uacElevation.AsCurrentUser<ISharedPackageContainerService>().IsSharedPackageContainerSupported())
                    {
                        throw new NotSupportedException(Resources.Localization.Dialogs_SharedContainer_NotSupported);
                    }

                    return await this.SaveDeploy(cancellationToken, progress).ConfigureAwait(false);

                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task<bool> SaveDeploy(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            this.Output.CurrentValue = null;

            progress.Report(new ProgressData(50, Resources.Localization.Dialogs_SharedContainer_Verifying));

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);

            var getExisting = await this._uacElevation.AsCurrentUser<ISharedPackageContainerService>()
                .GetByName(this.Name.CurrentValue, cancellationToken)
                .ConfigureAwait(false);

            var containerBuilder = new SharedPackageContainerBuilder(this.Name.CurrentValue);
            this.Resolution.CurrentValue = ContainerConflictResolution.Default;

            if (getExisting != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (getExisting.PackageFamilies?.Any() == true)
                {
                    // We may have a merging issue. Let's create some small help for the user.
                    var existingPackageFamilies = getExisting.PackageFamilies.Select(p => p.FamilyName).Distinct().OrderBy(f => f).ToList();
                    var currentPackageFamilies = this.Packages.Select(a => a.FamilyName.CurrentValue).Distinct().OrderBy(f => f).ToList();

                    var extraToBeAdded = currentPackageFamilies.Except(existingPackageFamilies).ToList();
                    if (!extraToBeAdded.Any())
                    {
                        this._interactionService.ShowInfo(Resources.Localization.Dialogs_SharedContainer_CannotDeploy_AllPackagesThere);
                        return false;
                    }

                    var extendedInfo = new StringBuilder();
                    extendedInfo.AppendLine(Resources.Localization.Dialogs_SharedContainer_Compare_InstalledContainer);
                    foreach (var item in existingPackageFamilies)
                    {
                        extendedInfo.AppendLine(" * " + item);
                    }

                    extendedInfo.AppendLine();
                    extendedInfo.AppendLine(Resources.Localization.Dialogs_SharedContainer_Compare_CurrentContainer);
                    foreach (var item in currentPackageFamilies)
                    {
                        if (extraToBeAdded.Contains(item))
                        {
                            extendedInfo.AppendLine(" * " + item + " " + Resources.Localization.Dialogs_SharedContainer_Compare_New);
                        }
                        else
                        {
                            extendedInfo.AppendLine(" * " + item);
                        }
                    }

                    var interactionResult = this._interactionService.ShowMessage(
                        string.Format(Resources.Localization.Dialogs_SharedContainer_ContainerExists, this.Name.CurrentValue, existingPackageFamilies.Count),
                        new List<string>
                        {
                            Resources.Localization.Dialogs_SharedContainer_ContainerExists_Replace,
                            Resources.Localization.Dialogs_SharedContainer_ContainerExists_Extend,
                            Resources.Localization.Button_Cancel
                        },
                        Resources.Localization.Dialogs_SharedContainer_ContainerExists_Title,
                        extendedInfo.ToString());

                    cancellationToken.ThrowIfCancellationRequested();
                    switch (interactionResult)
                    {
                        case 0:
                            this.Resolution.CurrentValue = ContainerConflictResolution.Replace;
                            
                            foreach (var familyName in this.Packages.Select(a => a.FamilyName.CurrentValue).Distinct())
                            {
                                containerBuilder.AddFamilyName(familyName);
                            }

                            break;

                        case 1:
                            this.Resolution.CurrentValue = ContainerConflictResolution.Merge;

                            foreach (var familyName in existingPackageFamilies.Concat(extraToBeAdded))
                            {
                                containerBuilder.AddFamilyName(familyName);
                            }
                            
                            break;
                        default:
                            return false;
                    }
                }
                else
                {
                    foreach (var familyName in this.Packages.Select(a => a.FamilyName.CurrentValue).Distinct())
                    {
                        containerBuilder.AddFamilyName(familyName);
                    }
                }
            }
            else
            {
                foreach (var familyName in this.Packages.Select(a => a.FamilyName.CurrentValue).Distinct())
                {
                    containerBuilder.AddFamilyName(familyName);
                }
            }

            progress.Report(new ProgressData(70, Resources.Localization.Dialogs_SharedContainer_Building));
            
            var container = containerBuilder.Build();
            
            progress.Report(new ProgressData(85, Resources.Localization.Dialogs_SharedContainer_Deploying));

            cancellationToken.ThrowIfCancellationRequested();
            
            try
            {
                await this._uacElevation.AsAdministrator<ISharedPackageContainerService>().Add(container, true, this.Resolution.CurrentValue, cancellationToken).ConfigureAwait(false);
            }
            catch (AlreadyInAnotherContainerException e)
            {
                this._interactionService.ShowError(string.Format(Resources.Localization.Dialogs_SharedContainer_PackageInMultipleContainers, e.FamilyName, e.ContainerName), e);
                return false;
            }

            progress.Report(new ProgressData(100, Resources.Localization.Dialogs_SharedContainer_Deploying));

            if (getExisting == null)
            {
                var _ = this._interactionService.ShowToast(Resources.Localization.Dialogs_SharedContainer_Result_Deployed, string.Format("Container with {0} packages has been added to container '{1}'.", container.PackageFamilies.Count, container.Name));
            }
            else if (this.Resolution.CurrentValue == ContainerConflictResolution.Merge)
            {
                var _ = this._interactionService.ShowToast(Resources.Localization.Dialogs_SharedContainer_Result_Merged, string.Format("Additional {0} packages have been added to an existing container '{1}'.", container.PackageFamilies.Count, container.Name)) ;
            }
            else
            {
                var _ = this._interactionService.ShowToast(Resources.Localization.Dialogs_SharedContainer_Result_Replaced, string.Format("Container with {0} packages has replaced previously added container '{1}'.", container.PackageFamilies.Count, container.Name));
            }
            
            return true;
        }

        private async Task<bool> SaveXml(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            var dialogFilter = new DialogFilterBuilder()
                .WithExtension(".xml", "Shared Package Containers")
                .WithAll();

            var opts = new FileDialogSettings(dialogFilter);

            if (!string.IsNullOrEmpty(this.Name.CurrentValue))
            {
                opts.InitialSelection = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Regex.Replace(this.Name.CurrentValue, @"[^a-zA-Z_\- .]", string.Empty).ToLowerInvariant() + ".xml");
            }

            if (!this._interactionService.SaveFile(opts, out var path))
            {
                return false;
            }

            this.Output.CurrentValue = path;

            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(new ProgressData(100, "Creating XML file..."));
            var sharedContainer = new SharedPackageContainerBuilder(this.Name.CurrentValue);

            foreach (var item in this.Packages)
            {
                sharedContainer.AddFamilyName(item.FamilyName.CurrentValue);
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

            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = File.OpenWrite(path);
            await using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            await streamWriter.WriteAsync(new StringBuilder(xml), cancellationToken);
            return true;
        }

        public ChangeableProperty<ContainerConflictResolution> Resolution { get; private set; } = new(ContainerConflictResolution.Default);

        public ChangeableProperty<string> Output { get; private set; } = new();

        protected override void UpdateVerbData()
        {
            this.Verb.Name = this.Verb.Name;

            switch (this.CreationMode.CurrentValue)
            {
                case ViewModel.CreationMode.Xml:
                    this.Verb.Output = this.Output.CurrentValue ?? Resources.Localization.Dialogs_SharedContainer_Output_Placeholder;
                    this.Verb.Force = false;
                    this.Verb.Merge = false;
                    this.Verb.ForceApplicationShutdown = false;
                    break;
                case ViewModel.CreationMode.Deploy:
                    this.Verb.Output = null;

                    switch (this.Resolution.CurrentValue)
                    {
                        case ContainerConflictResolution.Merge:
                            this.Verb.Force = false;
                            this.Verb.Merge = true;
                            break;

                        case ContainerConflictResolution.Replace:
                            this.Verb.Force = true;
                            this.Verb.Merge = false;
                            break;
                    }

                    this.Verb.ForceApplicationShutdown = true;
                    this.Verb.Output = this.Output.CurrentValue;
                    break;
            }

            this.Verb.Packages = this.Packages.Select(p =>
            {
                switch (p.Type.CurrentValue)
                {
                    case SharedPackageItemType.FilePath:
                        return p.FilePath.CurrentValue;
                    case SharedPackageItemType.Installed:
                        return p.FullName.CurrentValue;
                    default:
                        return p.FamilyName.CurrentValue;
                }
            });
        }

        private void OnAdd()
        {
            var package = SharedPackageViewModel.FromFamilyName(this._packageQueryService, "MSIXHero_zxq1da1qqbeze");
            package.Type.CurrentValue = SharedPackageItemType.New;

            this.Packages.Add(package);
            this.SelectedPackage = package;

            if (string.IsNullOrEmpty(this.Name.CurrentValue))
            {
                var addedPackage = this.Packages.Last();
                if (addedPackage.DisplayName != null)
                {
                    this.Name.CurrentValue = string.Format(Resources.Localization.Dialogs_SharedContainer_ContainerForPkg, addedPackage.DisplayName);
                }
                else
                {
                    this.Name.CurrentValue = string.Format(Resources.Localization.Dialogs_SharedContainer_ContainerForPkg, addedPackage.FamilyName.CurrentValue.Split('_').First());
                }
            }
        }

        private void OnOpen()
        {
            var filterBuilder = new DialogFilterBuilder()
                .WithPackages(DialogFilterBuilderPackagesExtensions.PackageTypes.Msix)
                .WithAll();

            var opts = new FileDialogSettings(filterBuilder);
            
            if (!this._interactionService.SelectFiles(opts, out var selections) || !selections.Any())
            {
                return;
            }

            var failed = new List<string>();

            foreach (var selection in selections)
            {
                var newPackage = new SharedPackageViewModel();
                newPackage.Type.CurrentValue = SharedPackageItemType.FilePath;

                if (newPackage.SetFromFilePath(selection, CancellationToken.None).GetAwaiter().GetResult())
                {
                    newPackage.Commit();
                    this.Packages.Add(newPackage);
                    this.SelectedPackage = newPackage;
                }
                else
                {
                    failed.Add(selection);
                }
            }

            if (!failed.Any())
            {
                if (this.Packages.Count == 1)
                {
                    this.SelectedPackage = this.Packages.Last();
                }

                if (string.IsNullOrEmpty(this.Name.CurrentValue))
                {
                    this.Name.CurrentValue = this.Packages.Last().DisplayName ?? this.Packages.Last().FamilyName.CurrentValue.Split('_').First();
                }

                return;
            }

            var addedPackage = this.Packages.Last();
            if (addedPackage.DisplayName != null)
            {
                this.Name.CurrentValue = string.Format(Resources.Localization.Dialogs_SharedContainer_ContainerForPkg, addedPackage.DisplayName);
            }
            else
            {
                this.Name.CurrentValue = string.Format(Resources.Localization.Dialogs_SharedContainer_ContainerForPkg, addedPackage.FamilyName.CurrentValue.Split('_').First());
            }

            if (selections.Length == 1)
            {
                this._interactionService.ShowError(Resources.Localization.Dialogs_SharedContainer_Selection_Error);
            }
            else
            {
                this._interactionService.ShowError(Resources.Localization.Dialogs_SharedContainer_Selections_Error);
            }
        }

        private void OnRemove()
        {
            if (this.SelectedPackage != null)
            {
                var currentSelectionIndex = this.Packages.IndexOf(this.SelectedPackage);
                if (currentSelectionIndex + 1 < this.Packages.Count)
                {
                    this.SelectedPackage = this.Packages[currentSelectionIndex + 1];
                }
                else if (currentSelectionIndex > 0)
                {
                    this.SelectedPackage = this.Packages[currentSelectionIndex - 1];
                }
                else
                {
                    this.SelectedPackage = null;
                }

                this.Packages.RemoveAt(currentSelectionIndex);
            }
        }

        private bool CanAdd() => true;

        private bool CanRemove() => this.SelectedPackage != null;

        private bool CanOpen() => true;

        private void CreationModeOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(AdminRightsRequired));
            this.OnPropertyChanged(nameof(OkPrompt));
        }


        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SharedPackageViewModel;
            var targetItem = dropInfo.TargetItem as SharedPackageViewModel;

            if (sourceItem == null || targetItem == null || sourceItem == targetItem)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            var indexOfSource = this.Packages.IndexOf(sourceItem);
            var indexOfTarget = this.Packages.IndexOf(targetItem);

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
            var sourceItem = dropInfo.Data as SharedPackageViewModel;
            var targetItem = dropInfo.TargetItem as SharedPackageViewModel;

            if (sourceItem == null || targetItem == null)
            {
                return;
            }

            if (sourceItem == targetItem)
            {
                return;
            }

            var indexOfTarget = this.Packages.IndexOf(targetItem);
            var indexOfSource = this.Packages.IndexOf(sourceItem);

            if (indexOfSource + 1 == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                this.Packages.Insert(indexOfTarget, sourceItem);
                this.Packages.RemoveAt(indexOfSource);
            }
            else
            {
                this.Packages.RemoveAt(indexOfSource);
                this.Packages.Insert(indexOfTarget, sourceItem);
            }

            this.SelectedPackage = sourceItem;
        }

        private void DropAfter(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SharedPackageViewModel;
            var targetItem = dropInfo.TargetItem as SharedPackageViewModel;

            if (sourceItem == null || targetItem == null)
            {
                return;
            }

            if (sourceItem == targetItem)
            {
                return;
            }

            var indexOfTarget = this.Packages.IndexOf(targetItem) + 1;
            var indexOfSource = this.Packages.IndexOf(sourceItem);

            if (indexOfSource == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                this.Packages.Insert(indexOfTarget, sourceItem);
                this.Packages.RemoveAt(indexOfSource);
            }
            else
            {
                this.Packages.RemoveAt(indexOfSource);
                this.Packages.Insert(indexOfTarget, sourceItem);
            }

            this.SelectedPackage = sourceItem;
        }

    }

    public enum CreationMode
    {
        Xml,
        Deploy
    }
}

