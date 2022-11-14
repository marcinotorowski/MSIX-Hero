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
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Commands.ViewModel
{
    public class CommandsSettingsTabViewModel : ChangeableContainer, ISettingsComponent, IDropTarget
    {
        private readonly IInteractionService _interactionService;
        private readonly IEventAggregator _eventAggregator;
        private CommandViewModel _selected;
        private ICommand _newCommand, _deleteCommand, _replaceIconCommand, _deleteIconCommand;
        private bool _toolsChanged;

        public CommandsSettingsTabViewModel(
            IInteractionService interactionService,
            IEventAggregator eventAggregator,
            IConfigurationService configurationService)
        {
            this._eventAggregator = eventAggregator;
            this._interactionService = interactionService;

            var items = configurationService.GetCurrentConfiguration().Packages?.Tools ?? Enumerable.Empty<ToolListConfiguration>();

            this.Items = new ValidatedChangeableCollection<CommandViewModel>(
                this.ValidateItems, 
                items.Select(item => new CommandViewModel(_interactionService, item)));

            this.AddChild(Items);
            this.Selected = Items.FirstOrDefault();
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
            if (_toolsChanged)
            {
                _eventAggregator.GetEvent<ToolsChangedEvent>().Publish(Items.Select(t => (ToolListConfiguration)t).ToArray());
            }
        }

        public bool CanSave()
        {
            return Items.IsTouched && (!Items.IsValidated || Items.IsValid);
        }

        public ValidatedChangeableCollection<CommandViewModel> Items { get; }

        public CommandViewModel Selected
        {
            get => _selected;
            set => SetField(ref _selected, value);
        }

        public ICommand NewCommand => _newCommand ??= new DelegateCommand(NewExecute, CanNewExecute);

        public ICommand DeleteCommand => _deleteCommand ??= new DelegateCommand(DeleteExecute, CanDeleteExecute);

        public ICommand ReplaceIconCommand => _replaceIconCommand ??= new DelegateCommand(ReplaceIconExecute, CanReplaceIconExecute);

        public ICommand DeleteIconCommand => _deleteIconCommand ??= new DelegateCommand(DeleteIconExecute, CanDeleteIconExecute);

        public Task<bool> OnSaving(Configuration newConfiguration)
        {
            if (!IsTouched)
            {
                return Task.FromResult(false);
            }

            _toolsChanged = true;
            newConfiguration.Packages.Tools.Clear();

            foreach (var item in Items)
            {
                newConfiguration.Packages.Tools.Add(item);
            }

            return Task.FromResult(true);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as CommandViewModel;
            var targetItem = dropInfo.TargetItem as CommandViewModel;

            if (sourceItem == null || targetItem == null || sourceItem == targetItem)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            var indexOfSource = Items.IndexOf(sourceItem);
            var indexOfTarget = Items.IndexOf(targetItem);

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
            dropInfo.DestinationText = string.Format(Resources.Localization.Dialogs_Settings_Tools_DragDrop_Hint, sourceItem.Name);
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
            var sourceItem = dropInfo.Data as CommandViewModel;
            var targetItem = dropInfo.TargetItem as CommandViewModel;

            if (sourceItem == null || targetItem == null)
            {
                return;
            }

            if (sourceItem == targetItem)
            {
                return;
            }

            var indexOfTarget = Items.IndexOf(targetItem);
            var indexOfSource = Items.IndexOf(sourceItem);

            if (indexOfSource + 1 == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                Items.Insert(indexOfTarget, sourceItem);
                Items.RemoveAt(indexOfSource);
            }
            else
            {
                Items.RemoveAt(indexOfSource);
                Items.Insert(indexOfTarget, sourceItem);
            }

            Selected = sourceItem;
        }

        private string ValidateItems(IEnumerable<CommandViewModel> collection)
        {
            var invalid = collection.FirstOrDefault(item => !item.IsValid);
            return invalid == null ? null : invalid.ValidationMessage;
        }

        private void DropAfter(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as CommandViewModel;
            var targetItem = dropInfo.TargetItem as CommandViewModel;

            if (sourceItem == null || targetItem == null)
            {
                return;
            }

            if (sourceItem == targetItem)
            {
                return;
            }

            var indexOfTarget = Items.IndexOf(targetItem) + 1;
            var indexOfSource = Items.IndexOf(sourceItem);

            if (indexOfSource == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                Items.Insert(indexOfTarget, sourceItem);
                Items.RemoveAt(indexOfSource);
            }
            else
            {
                Items.RemoveAt(indexOfSource);
                Items.Insert(indexOfTarget, sourceItem);
            }

            Selected = sourceItem;
        }

        private void DeleteExecute()
        {
            if (Selected == null)
            {
                return;
            }

            var indexOf = Items.IndexOf(Selected);
            if (indexOf == -1)
            {
                return;
            }

            Items.RemoveAt(indexOf);
            if (indexOf >= Items.Count)
            {
                indexOf--;
            }

            if (indexOf >= 0)
            {
                Selected = Items[indexOf];
            }
            else
            {
                Selected = null;
            }
        }

        private bool CanDeleteExecute()
        {
            return Selected != null;
        }

        private void NewExecute()
        {
            var newItem = new CommandViewModel(_interactionService, new ToolListConfiguration { Name = Resources.Localization.Dialogs_Settings_Tools_NewTool_Name });
            Items.Add(newItem);
            Selected = newItem;
        }

        private bool CanNewExecute()
        {
            return true;
        }

        private void ReplaceIconExecute()
        {
            var result = _interactionService.SelectFile(FileDialogSettings.FromFilterString(Resources.Localization.Dialogs_Settings_Tools_Filter_Ico + "|*.ico|" + Resources.Localization.Dialogs_Settings_Tools_Filter_Exe + "|*.exe;*.dll"), out var selectedIcon);
            if (result)
            {
                Selected.Icon.CurrentValue = selectedIcon;
            }
        }

        private bool CanReplaceIconExecute()
        {
            return Selected != null;
        }


        private void DeleteIconExecute()
        {
            Selected.Icon.CurrentValue = null;
        }

        private bool CanDeleteIconExecute()
        {
            return !string.IsNullOrEmpty(Selected?.Icon.CurrentValue);
        }
    }
}
