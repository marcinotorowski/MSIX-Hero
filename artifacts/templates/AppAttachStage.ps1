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
    $parentFolder = "\" + $package.parentFolder + "\";
    $volumeGuid = $package.volumeGuid;
    $msixJunction = $package.msixJunction;
    
    try
    {
        Mount-Diskimage -ImagePath $vhdSrc -NoDriveLetter -Access ReadOnly;
        Write-Host ("Mounting of " + $vhdSrc + " was completed!") -BackgroundColor Green;
    }
    catch
    {
        Write-Host ("Mounting of " + $vhdSrc + " has failed!") -BackgroundColor Red;
    }
    
    $msixDest = "\\?\Volume{" + $volumeGuid + "}\";
    
    if (!(Test-Path $msixJunction))
    {
        New-Item $msixJunction -ItemType Directory;
    }
    
    $msixJunction = Join-Path $msixJunction $packageName;
    if (Test-Path $msixJunction)
    {
        throw "The path $msixJunction already exists!";
    }

    cmd.exe /c mklink /j $msixJunction $msixDest

    $lec = $LASTEXITCODE;
    if (0 -ne $lec)
    {
        throw "mklink returned exit code $lec";
    }

    [Windows.Management.Deployment.PackageManager,Windows.Management.Deployment,ContentType=WindowsRuntime] | Out-Null;
    
    Add-Type -AssemblyName System.Runtime.WindowsRuntime;
    
    $asTask = ([System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { $_.ToString() -eq 'System.Threading.Tasks.Task`1[TResult] AsTask[TResult,TProgress](Windows.Foundation.IAsyncOperationWithProgress`2[TResult,TProgress])'})[0];
    $asTaskAsyncOperation = $asTask.MakeGenericMethod([Windows.Management.Deployment.DeploymentResult], [Windows.Management.Deployment.DeploymentProgress]);
    
    $packageManager = [Windows.Management.Deployment.PackageManager]::new();
    $path = $msixJunction + $parentFolder + $packageName;
    $path = ([System.Uri]$path).AbsoluteUri;
    $asyncOperation = $packageManager.StagePackageAsync($path, $null, "StageInPlace");
    $task = $asTaskAsyncOperation.Invoke($null, @($asyncOperation));
    $task;
}