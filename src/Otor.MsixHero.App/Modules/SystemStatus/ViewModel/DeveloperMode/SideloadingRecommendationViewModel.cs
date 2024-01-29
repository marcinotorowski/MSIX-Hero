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

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel.DeveloperMode;

public class SideloadingRecommendationViewModel : BaseRecommendationViewModel
{
    public SideloadingRecommendationViewModel()
    {
        this.Status = RecommendationStatus.Success;
    }

    public override string Title { get; } = Resources.Localization.System_DeveloperOptions_SideLoading;
        
    public override Task Refresh(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
    
    public override bool IsEnabled { get; } = true;

    protected override Geometry GetIcon()
    {
        return Geometry.Empty;
    }

}