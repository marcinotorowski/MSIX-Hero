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

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [Flags]
    public enum EventFilter
    {
        Warning   = 1 << 0,
        Info      = 1 << 1,
        Error     = 1 << 2,
        Verbose   = 1 << 3,
        Critical  = 1 << 13,
        AllLevels = Warning | Info | Error | Verbose | Critical,

        PackagingPerformance = 1 << 4,
        PackagingOperational = 1 << 5,
        PackagingDebug = 1 << 6,
        AllPackaging = PackagingPerformance | PackagingOperational | PackagingDebug,
        
        DeploymentServerOperational = 1 << 7,
        DeploymentServerDiagnostic = 1 << 8,
        DeploymentServerDebug = 1 << 9,
        DeploymentServerRestricted = 1 << 10,
        AllDeploymentServer = DeploymentServerOperational | DeploymentServerDiagnostic | DeploymentServerDebug | DeploymentServerRestricted,

        DeploymentOperational = 1 << 11,
        DeploymentDiagnostic = 1 << 12,
        AllDeployment = DeploymentOperational | DeploymentDiagnostic,

        AllSources = AllPackaging | AllDeploymentServer | AllDeployment,
            
        All       = AllLevels | AllSources | AllSources,

        Default   = Warning | Error | Info | AllDeployment | DeploymentServerOperational | DeploymentServerDebug | DeploymentServerDiagnostic
    }
}