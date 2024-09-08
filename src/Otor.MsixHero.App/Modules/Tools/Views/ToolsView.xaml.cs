// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Controls.Cards;
using Otor.MsixHero.App.Hero.Commands.Tools;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.Main.Events;
using Prism.Events;
using Prism.Navigation.Regions;

namespace Otor.MsixHero.App.Modules.Tools.Views
{
    public partial class ToolsView : INavigationAware
    {
        private readonly IEventAggregator _eventAggregator;

        public ToolsView(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
            eventAggregator.GetEvent<UiExecutedEvent<SetToolFilterCommand>>().Subscribe(this.OnSetToolFilter, ThreadOption.UIThread);
            InitializeComponent();
        }

        private void OnSetToolFilter(UiExecutedPayload<SetToolFilterCommand> obj)
        {
            var allChildren = this.ColumnLeft.Children.OfType<FrameworkElement>().Union(this.ColumnRight.Children.OfType<FrameworkElement>());

            Label label = null;
            var isLabelVisible = false;

            foreach (var child in allChildren)
            {
                if (child is Label childLabel)
                {
                    if (label != null)
                    {
                        label.Visibility = isLabelVisible ? Visibility.Visible : Visibility.Collapsed;
                        isLabelVisible = false;
                    }

                    label = childLabel;
                }
                else if (child is CardAction childCardAction)
                {
                    var isChildVisible = string.IsNullOrEmpty(obj.Request.SearchKey);
                    if (!isChildVisible && childCardAction.Content is TextBlock textBlock)
                    {
                        var localText = textBlock.Text;
                        isChildVisible |= localText.IndexOf(obj.Request.SearchKey, StringComparison.OrdinalIgnoreCase) != -1;
                    }

                    childCardAction.Visibility = isChildVisible ? Visibility.Visible : Visibility.Collapsed;
                    isLabelVisible |= isChildVisible;
                }
            }

            if (label != null)
            {
                label.Visibility = isLabelVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            var leftVisible = this.ColumnLeft.Children.OfType<FrameworkElement>().Any(fe => fe.Visibility == Visibility.Visible);
            var rightVisible = this.ColumnRight.Children.OfType<FrameworkElement>().Any(fe => fe.Visibility == Visibility.Visible);

            if (leftVisible && !rightVisible)
            {
                this.ColumnDefinition1.Width = new GridLength(1, GridUnitType.Star);
                this.ColumnDefinition2.Width = new GridLength(0, GridUnitType.Pixel);
                this.ColumnDefinition3.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else if (!leftVisible && rightVisible)
            {
                this.ColumnDefinition1.Width = new GridLength(0, GridUnitType.Pixel);
                this.ColumnDefinition2.Width = new GridLength(0, GridUnitType.Pixel);
                this.ColumnDefinition3.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                this.ColumnDefinition1.Width = new GridLength(1, GridUnitType.Star);
                this.ColumnDefinition2.Width = new GridLength(16, GridUnitType.Pixel);
                this.ColumnDefinition3.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this._eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(250.0));
        }
        
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
