// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using DragDropEffects = System.Windows.DragDropEffects;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tools
{
    public class ToolsConfigurationViewModel : ChangeableContainer, IDropTarget
    {
        private readonly IInteractionService interactionService;
        private readonly Configuration configuration;
        private ToolViewModel selected;
        private ICommand newCommand, deleteCommand, replaceIconCommand, deleteIconCommand;

        public ToolsConfigurationViewModel(IInteractionService interactionService, Configuration configuration)
        {
            this.interactionService = interactionService;
            this.configuration = configuration;

            var items = configuration?.Packages?.Tools;
            if (items == null)
            {
                this.Items = new ValidatedChangeableCollection<ToolViewModel>();
            }
            else
            {
                this.Items = new ValidatedChangeableCollection<ToolViewModel>(this.ValidateItems, items.Select(item => new ToolViewModel(this.interactionService, item)));
            }

            this.AddChild(this.Items);
            this.Selected = this.Items.FirstOrDefault();
        }

        public ChangeableCollection<ToolViewModel> Items { get; }

        public ToolViewModel Selected
        {
            get => this.selected;
            set => this.SetField(ref this.selected, value);
        }

        public ICommand NewCommand => this.newCommand ??= new DelegateCommand(this.NewExecute, this.CanNewExecute);

        public ICommand DeleteCommand => this.deleteCommand ??= new DelegateCommand(this.DeleteExecute, this.CanDeleteExecute);

        public ICommand ReplaceIconCommand => this.replaceIconCommand ??= new DelegateCommand(this.ReplaceIconExecute, this.CanReplaceIconExecute);
        
        public ICommand DeleteIconCommand => this.deleteIconCommand ??= new DelegateCommand(this.DeleteIconExecute, this.CanDeleteIconExecute);

        public override void Commit()
        {
            base.Commit();
            
            this.configuration.Packages.Tools.Clear();
            foreach (var item in this.Items)
            {
                this.configuration.Packages.Tools.Add(item);
            }
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as ToolViewModel;
            var targetItem = dropInfo.TargetItem as ToolViewModel;

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
            dropInfo.DestinationText = $"Move {sourceItem.Name} here";
            dropInfo.EffectText = $"Move {sourceItem.Name} here";
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
            var sourceItem = dropInfo.Data as ToolViewModel;
            var targetItem = dropInfo.TargetItem as ToolViewModel;

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

        private string ValidateItems(IEnumerable<ToolViewModel> collection)
        {
            var invalid = collection.FirstOrDefault(item => !item.IsValid);
            return invalid == null ? null : invalid.ValidationMessage;
        }

        private void DropAfter(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as ToolViewModel;
            var targetItem = dropInfo.TargetItem as ToolViewModel;

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
            var newItem = new ToolViewModel(this.interactionService, new ToolListConfiguration { Name = "New tool" });
            this.Items.Add(newItem);
            this.Selected = newItem;
        }

        private bool CanNewExecute()
        {
            return true;
        }

        private void ReplaceIconExecute()
        {
            var result = this.interactionService.SelectFile(FileDialogSettings.FromFilterString("Icon files|*.ico|Executable files|*.exe;*.dll"), out var selectedIcon);
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
