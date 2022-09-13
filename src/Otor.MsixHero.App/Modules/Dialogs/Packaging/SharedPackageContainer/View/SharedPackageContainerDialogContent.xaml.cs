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

using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.ViewModel;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.View
{
    public partial class SharedPackageContainerDialogContent
    {

        public SharedPackageContainerDialogContent()
        {
            this.InitializeComponent();
            this.PreviewMouseDown += OnPreviewMouseDown;
            this.DataContextChanged += OnDataContextChanged;
            if (this.DataContext is SharedPackageContainerViewModel dataContext)
            {
                dataContext.Packages.CollectionChanged += this.AppsOnCollectionChanged;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is SharedPackageContainerViewModel oldDataContext)
            {
                oldDataContext.Packages.CollectionChanged -= this.AppsOnCollectionChanged;
            }

            if (e.NewValue is SharedPackageContainerViewModel newDataContext)
            {
                newDataContext.Packages.CollectionChanged += this.AppsOnCollectionChanged;
            }
        }

        private void AppsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count != 1)
            {
                return;
            }

            var addedItem = (SharedPackageViewModel)e.NewItems[0];
            if (addedItem == null)
            {
                return;
            }

            var dataContext = (SharedPackageContainerViewModel)this.DataContext;
            if (addedItem.Type.CurrentValue == SharedPackageItemType.New && !dataContext.Packages.Any(a => a.IsEditing))
            {
                Dispatcher.BeginInvoke(() => addedItem.IsEditing = true, DispatcherPriority.SystemIdle);
            }
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dataContext = (SharedPackageContainerViewModel)this.DataContext;
            var editing = dataContext.SelectedPackage;

            if (editing?.IsEditing != true)
            {
                return;
            }

            var container = (ListBoxItem)this.ListBox.ItemContainerGenerator.ContainerFromItem(editing);
            if (!container.IsMouseOver)
            {
                editing.IsEditing = false;
            }
        }

        private void ListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = (ListBox)sender;
            
            for (var i = 0; i < listBox.Items.Count; i++)
            {
                var container = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                var viewModel = (SharedPackageViewModel)container.DataContext;
                if (container.IsMouseOver)
                {
                    viewModel.IsEditing = true;
                }
                else
                {
                    viewModel.IsEditing = false;
                }
            }
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;
            var viewModel = (SharedPackageViewModel)textBox.DataContext;

            switch (e.Key)
            {
                case Key.Enter:
                    var dataContext = (SharedPackageContainerViewModel)this.DataContext;
                    viewModel.SetFromFamilyName(dataContext.PackageQueryService, textBox.Text, CancellationToken.None).GetAwaiter().GetResult();
                    viewModel.IsEditing = false;
                    break;
                case Key.Escape:
                    viewModel.IsEditing = false;
                    break;
            }
        }

        private void TextBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                return;
            }

            var textBox = (TextBox)sender;
            var viewModel = (SharedPackageViewModel)textBox.DataContext;
            if (viewModel == null)
            {
                return;
            }

            textBox.Text = viewModel.FamilyName.CurrentValue;

            Dispatcher.BeginInvoke(() =>
            {
                var window = Window.GetWindow(textBox);
                if (window == null)
                {
                    return;
                }

                textBox.Focus();
                Keyboard.Focus(textBox);
                FocusManager.SetFocusedElement(window, textBox);
                textBox.SelectAll();
            }, DispatcherPriority.SystemIdle);
        }

        private void ListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F2)
            {
                return;
            }
            
            var dataContext = (SharedPackageContainerViewModel)this.DataContext;
            var editing = dataContext.SelectedPackage;

            if (editing?.IsEditing != false)
            {
                return;
            }
            
            editing.IsEditing = true;
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataContext = (SharedPackageContainerViewModel)this.DataContext;
            if (dataContext.InstalledPackages.Accept.CanExecute(null))
            {
                dataContext.InstalledPackages.Accept.Execute(null);
            }
        }
    }
}
