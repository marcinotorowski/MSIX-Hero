# MSIX Hero
# Copyright (C) 2022 Marcin Otorowski
# 
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
# 
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
# 
# Full notice:
# https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

$configFilePath = (Get-ChildItem $PSScriptRoot -Filter *.json | Select-Object -First 1).FullName;
if ($null -eq $configFilePath)
{
    throw "Missing JSON config!";
}

$configFile = Get-Content $configFilePath -Raw | ConvertFrom-Json;

foreach ($package in $configFile)
{
    $vhdSrc =  Join-Path $PSScriptRoot $package.vhdFileName;
    $packageName = $package.packageName;    
    $msixJunction = Join-Path $package.msixJunction $packageName;
    
    Remove-AppxPackage -AllUsers -Package $packageName;
    Dismount-DiskImage -ImagePath $vhdSrc;

    if (Test-Path $msixJunction)
    {
        Remove-Item $msixJunction -Force -Recurse;
    }
}