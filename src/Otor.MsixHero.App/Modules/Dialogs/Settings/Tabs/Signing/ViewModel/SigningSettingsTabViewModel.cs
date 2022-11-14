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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Signing.Testing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Signing.ViewModel
{
    public class SigningSettingsTabViewModel : ChangeableContainer, ISettingsComponent, IDropTarget
    {
        private readonly ISigningTestService _signTestService;
        private readonly Configuration _configuration;
        private readonly IInteractionService _interactionService;
        private readonly IUacElevation _uacElevation;
        private readonly ITimeStampFeed _timeStampFeed;
        private SignProfileViewModel _selectedProfile;

        public SigningSettingsTabViewModel(
            ISigningTestService signTestService,
            IConfigurationService configurationService,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            ITimeStampFeed timeStampFeed)
        {
            this._signTestService = signTestService;
            this._configuration = configurationService.GetCurrentConfiguration();
            this._interactionService = interactionService;
            this._uacElevation = uacElevation;
            this._timeStampFeed = timeStampFeed;
            
            this.AddProfile = new DelegateCommand(OnAddProfile);
            this.RemoveSelectedProfile = new DelegateCommand(OnRemoveSelectedProfile, CanRemoveSelectedProfile);
            
            this.Profiles = new ValidatedChangeableCollection<SignProfileViewModel>(
                this.CollectionValidator,
                this._configuration.Signing?.Profiles?.Select(
                    p => new SignProfileViewModel(this._signTestService,
                        this._interactionService,
                        this._uacElevation,
                        this._configuration.Signing,
                        p,
                        this._timeStampFeed)) ?? Enumerable.Empty<SignProfileViewModel>());

            this.SelectedProfile =
                this.Profiles.FirstOrDefault(p => p.IsDefault.CurrentValue)
                ?? this.Profiles.FirstOrDefault();

            this.AddChild(Profiles);
        }

        public void Register(ISettingsContext context)
        {  
            context.Register(this);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public void OnDialogClosed()
        {
        }

        public bool CanSave()
        {
            return !this.Profiles.IsTouched || !this.Profiles.IsValidated || this.Profiles.IsValid;
        }

        public ICommand AddProfile { get; }

        public ICommand RemoveSelectedProfile { get; }

        public ValidatedChangeableCollection<SignProfileViewModel> Profiles { get; }

        public SignProfileViewModel SelectedProfile
        {
            get => _selectedProfile;
            set => SetField(ref _selectedProfile, value);
        }
        
        public Task<bool> OnSaving(Configuration newConfiguration)
        {
            if (!Profiles.IsTouched)
            {
                return Task.FromResult(true);
            }
            
            if (!Profiles.IsValid)
            {
                return Task.FromResult(false);
            }

            newConfiguration.Signing.Profiles = new List<SigningProfile>();
            foreach (var item in Profiles)
            {
                var newProfile = item.ToSigningProfile();
                newConfiguration.Signing.Profiles.Add(newProfile);
            }

            return Task.FromResult(true);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SignProfileViewModel;
            var targetItem = dropInfo.TargetItem as SignProfileViewModel;

            if (sourceItem == null || targetItem == null || sourceItem == targetItem)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            var indexOfSource = Profiles.IndexOf(sourceItem);
            var indexOfTarget = Profiles.IndexOf(targetItem);

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
                    DropBefore(dropInfo);
                    break;
                case RelativeInsertPosition.AfterTargetItem:
                    DropAfter(dropInfo);
                    break;
            }
        }

        private void DropBefore(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SignProfileViewModel;
            var targetItem = dropInfo.TargetItem as SignProfileViewModel;

            if (sourceItem == null || targetItem == null)
            {
                return;
            }

            if (sourceItem == targetItem)
            {
                return;
            }

            var indexOfTarget = Profiles.IndexOf(targetItem);
            var indexOfSource = Profiles.IndexOf(sourceItem);

            if (indexOfSource + 1 == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                Profiles.Insert(indexOfTarget, sourceItem);
                Profiles.RemoveAt(indexOfSource);
            }
            else
            {
                Profiles.RemoveAt(indexOfSource);
                Profiles.Insert(indexOfTarget, sourceItem);
            }

            SelectedProfile = sourceItem;
        }

        private void DropAfter(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as SignProfileViewModel;
            var targetItem = dropInfo.TargetItem as SignProfileViewModel;

            if (sourceItem == null || targetItem == null)
            {
                return;
            }

            if (sourceItem == targetItem)
            {
                return;
            }

            var indexOfTarget = Profiles.IndexOf(targetItem) + 1;
            var indexOfSource = Profiles.IndexOf(sourceItem);

            if (indexOfSource == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                Profiles.Insert(indexOfTarget, sourceItem);
                Profiles.RemoveAt(indexOfSource);
            }
            else
            {
                Profiles.RemoveAt(indexOfSource);
                Profiles.Insert(indexOfTarget, sourceItem);
            }

            SelectedProfile = sourceItem;
        }

        private string CollectionValidator(IEnumerable<SignProfileViewModel> arg)
        {
            var previousNames = new HashSet<string>();
            foreach (var item in arg)
            {
                if (!item.IsValid)
                {
                    return item.ValidationMessage;
                }

                if (!previousNames.Add(item.DisplayName.CurrentValue))
                {
                    return "Profile name " + item.DisplayName.CurrentValue + " is already in use.";
                }
            }

            return null;
        }

        private void OnAddProfile()
        {
            var baseName = "New profile";

            var newModel = new SigningProfile()
            {
                Name = baseName
            };

            var index = 0;
            while (Profiles.Any(p => string.Equals(p.DisplayName.CurrentValue, newModel.Name)))
            {
                if (index > 0)
                {
                    newModel.Name = baseName + " (" + (index + 1) + ")";
                }

                index++;
            }

            Profiles.Add(new SignProfileViewModel(_signTestService, _interactionService, _uacElevation, _configuration.Signing, newModel, _timeStampFeed));
            SelectedProfile = Profiles.Last();
        }

        private void OnRemoveSelectedProfile()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            Profiles.Remove(SelectedProfile);
        }

        private bool CanRemoveSelectedProfile()
        {
            return SelectedProfile != null;
        }
    }
}
