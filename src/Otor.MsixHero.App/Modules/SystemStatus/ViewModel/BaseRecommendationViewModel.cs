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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Localization;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel
{
    public abstract class BaseRecommendationViewModel : NotifyPropertyChanged
    {
        private readonly Lazy<Geometry> _iconProvider;
        private bool _isExpanded;
        private string _summary = Resources.Localization.System_Checking;
        private RecommendationStatus _status;
        private bool _isLoading;
        protected BaseRecommendationViewModel()
        {
            this._iconProvider = new Lazy<Geometry>(this.GetIcon);

            MsixHeroTranslation.Instance.CultureChanged += (_, _) =>
            {
                this.OnCultureChanged();
            };
        }

        protected virtual void OnCultureChanged()
        {
            this.OnPropertyChanged(nameof(Title));
            this.OnPropertyChanged(nameof(Summary));
        }

        public abstract string Title { get; }

        public Geometry Icon => this._iconProvider.Value;

        public bool IsLoading
        {
            get => this._isLoading;
            set => this.SetField(ref this._isLoading, value);
        }

        protected abstract Geometry GetIcon();

        public  string Summary
        {
            get => this._summary;
            protected set => this.SetField(ref this._summary, value);
        }

        public RecommendationStatus Status
        {
            get => _status;
            protected set => this.SetField(ref this._status, value);
        }

        public virtual bool IsEnabled => true;

        public abstract Task Refresh(CancellationToken cancellationToken = default);

        public bool IsExpanded
        {
            get => this._isExpanded;
            set => this.SetField(ref this._isExpanded, value);
        }
    }
}
