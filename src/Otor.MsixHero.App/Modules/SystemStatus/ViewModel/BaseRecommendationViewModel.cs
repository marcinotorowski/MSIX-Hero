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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel
{
    public abstract class BaseRecommendationViewModel : NotifyPropertyChanged
    {
        private readonly Lazy<Geometry> iconProvider;
        private bool isExpanded;
        private string summary = "Checking your system...";
        private RecommendationStatus status;

        protected BaseRecommendationViewModel()
        {
            this.iconProvider = new Lazy<Geometry>(this.GetIcon);
        }

        public abstract string Title { get; }

        public Geometry Icon
        {
            get => this.iconProvider.Value;
        }

        protected abstract Geometry GetIcon();

        public  string Summary
        {
            get => this.summary;
            protected set => this.SetField(ref this.summary, value);
        }

        public RecommendationStatus Status
        {
            get => status;
            protected set => this.SetField(ref this.status, value);
        }

        public virtual bool IsEnabled => true;

        public abstract Task Refresh(CancellationToken cancellationToken = default);

        public bool IsExpanded
        {
            get => this.isExpanded;
            set => this.SetField(ref this.isExpanded, value);
        }
    }
}
