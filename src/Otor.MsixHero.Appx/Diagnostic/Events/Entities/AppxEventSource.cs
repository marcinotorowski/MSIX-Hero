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

namespace Otor.MsixHero.Appx.Diagnostic.Events.Entities;

public static class AppxEventSources
{
    public const string PackagingPerformance = "Microsoft-Windows-AppxPackagingNameHelper/Performance";
    public const string PackagingOperational = "Microsoft-Windows-AppxPackagingNameHelper/Operational";
    public const string PackagingDebug = "Microsoft-Windows-AppxPackagingNameHelper/Debug";
    public const string DeploymentServerOperational = "Microsoft-Windows-AppXDeploymentServer/Operational";
    public const string DeploymentServerDiagnostic = "Microsoft-Windows-AppXDeploymentServer/Diagnostic";
    public const string DeploymentServerRestricted = "Microsoft-Windows-AppXDeploymentServer/Restricted";
    public const string DeploymentServerDebug = "Microsoft-Windows-AppXDeploymentServer/Debug";
    public const string DeploymentOperational = " Microsoft-Windows-AppXDeployment/Operational";
    public const string DeploymentDiagnostic = " Microsoft-Windows-AppXDeployment/Diagnostic";
}