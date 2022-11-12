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
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tabs.Commands
{
    public class CommandsSettingsTabViewModel : ChangeableContainer, ISettingsTabViewModel, IDropTarget
    {
        private readonly IInteractionService _interactionService;
        private readonly IEventAggregator _eventAggregator;
        private CommandViewModel _selected;
        private ICommand _newCommand, _deleteCommand, _replaceIconCommand, _deleteIconCommand;
        private bool _toolsChanged;

        public CommandsSettingsTabViewModel(
            IInteractionService interactionService,
            IEventAggregator eventAggregator,
            Configuration configuration)
        {
            this._eventAggregator = eventAggregator;
            this._interactionService = interactionService;

            var items = configuration?.Packages?.Tools;
            if (items == null)
            {
                this.Items = new ValidatedChangeableCollection<CommandViewModel>();
            }
            else
            {
                this.Items = new ValidatedChangeableCollection<CommandViewModel>(this.ValidateItems, items.Select(item => new CommandViewModel(this._interactionService, item)));
            }

            this.AddChild(this.Items);
            this.Selected = this.Items.FirstOrDefault();
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
            if (this._toolsChanged)
            {
                this._eventAggregator.GetEvent<ToolsChangedEvent>().Publish(this.Items.Select(t => (ToolListConfiguration)t).ToArray());
            }
        }

        public bool CanSave()
        {
            return this.Items.IsTouched && (!this.Items.IsValidated || this.Items.IsValid);
        }

        public ValidatedChangeableCollection<CommandViewModel> Items { get; }

        public CommandViewModel Selected
        {
            get => this._selected;
            set => this.SetField(ref this._selected, value);
        }

        public ICommand NewCommand => this._newCommand ??= new DelegateCommand(this.NewExecute, this.CanNewExecute);

        public ICommand DeleteCommand => this._deleteCommand ??= new DelegateCommand(this.DeleteExecute, this.CanDeleteExecute);

        public ICommand ReplaceIconCommand => this._replaceIconCommand ??= new DelegateCommand(this.ReplaceIconExecute, this.CanReplaceIconExecute);
        
        public ICommand DeleteIconCommand => this._deleteIconCommand ??= new DelegateCommand(this.DeleteIconExecute, this.CanDeleteIconExecute);

        public Task<bool> UpdateConfiguration(Configuration newConfiguration)
        {
            if (!this.IsTouched)
            {
                return Task.FromResult(false);
            }

            this._toolsChanged = true;
            newConfiguration.Packages.Tools.Clear();

            foreach (var item in this.Items)
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

            var indexOfSource = this.Items.IndexOf(sourceItem);
            var indexOfTarget = this.Items.IndexOf(targetItem);
            
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
                    this.DropBefore(dropInfo);
                    break;
                case RelativeInsertPosition.AfterTargetItem:
                    this.DropAfter(dropInfo);
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

            var indexOfTarget = this.Items.IndexOf(targetItem);
            var indexOfSource = this.Items.IndexOf(sourceItem);

            if (indexOfSource + 1 == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                this.Items.Insert(indexOfTarget, sourceItem);
                this.Items.RemoveAt(indexOfSource);
            }
            else
            {
                this.Items.RemoveAt(indexOfSource);
                this.Items.Insert(indexOfTarget, sourceItem);
            }

            this.Selected = sourceItem;
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

            var indexOfTarget = this.Items.IndexOf(targetItem) + 1;
            var indexOfSource = this.Items.IndexOf(sourceItem);

            if (indexOfSource == indexOfTarget)
            {
                return;
            }

            if (indexOfTarget > indexOfSource)
            {
                this.Items.Insert(indexOfTarget, sourceItem);
                this.Items.RemoveAt(indexOfSource);
            }
            else
            {
                this.Items.RemoveAt(indexOfSource);
                this.Items.Insert(indexOfTarget, sourceItem);
            }

            this.Selected = sourceItem;
        }

        private void DeleteExecute()
        {
            if (this.Selected == null)
            {
                return;
            }

            var indexOf = this.Items.IndexOf(this.Selected);
            if (indexOf == -1)
            {
                return;
            }

            this.Items.RemoveAt(indexOf);
            if (indexOf >= this.Items.Count)
            {
                indexOf--;
            }

            if (indexOf >= 0)
            {
                this.Selected = this.Items[indexOf];
            }
            else
            {
                this.Selected = null;
            }
        }

        private bool CanDeleteExecute()
        {
            return this.Selected != null;
        }

        private void NewExecute()
        {
            var newItem = new CommandViewModel(this._interactionService, new ToolListConfiguration { Name = Resources.Localization.Dialogs_Settings_Tools_NewTool_Name });
            this.Items.Add(newItem);
            this.Selected = newItem;
        }

        private bool CanNewExecute()
        {
            return true;
        }

        private void ReplaceIconExecute()
        {
            var result = this._interactionService.SelectFile(FileDialogSettings.FromFilterString(Resources.Localization.Dialogs_Settings_Tools_Filter_Ico + "|*.ico|" + Resources.Localization.Dialogs_Settings_Tools_Filter_Exe + "|*.exe;*.dll"), out var selectedIcon);
            if (result)
            {
                this.Selected.Icon.CurrentValue = selectedIcon;
            }
        }

        private bool CanReplaceIconExecute()
        {
            return this.Selected != null;
        }


        private void DeleteIconExecute()
        {
            this.Selected.Icon.CurrentValue = null;
        }

        private bool CanDeleteIconExecute()
        {
            return !string.IsNullOrEmpty(this.Selected?.Icon.CurrentValue);
        }
    }
}
