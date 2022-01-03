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

[CmdletBinding()]
param(
    [String]$SourceMsixFile,
    [String]$CerOutputFileName = $null,
	[Switch]$ImportToStore = $false
)

$tempExtract = Join-Path $env:TEMP ([System.Guid]::NewGuid().ToString("B") + ".cer");
try
{
	$fileName = Split-Path $SourceMsixFile -Leaf;
	Write-Progress -Activity "Extracting certificate from $fileName..." -Id "1" -Status "Extracting certificate..." -PercentComplete 5;
	$cert = (Get-AuthenticodeSignature -FilePath $SourceMsixFile).SignerCertificate;
	Write-Progress -Activity "Extracting certificate from $fileName..." -Id "1" -Status "Exporting to a temp file..." -PercentComplete 75;
    Export-Certificate -Cert $cert -FilePath $tempExtract -Force | Out-Null;
	
	if ($ImportToStore)
	{
		Write-Progress -Activity "Extracting certificate from $fileName..." -Id "1" -Status "Importing to TrustedPeople store..." -PercentComplete 95;
		Import-Certificate -FilePath $tempExtract -CertStoreLocation cert:\LocalMachine\TrustedPeople;
	}
	
	if (-not ([String]::IsNullOrEmpty($CerOutputFileName)))
	{
		Write-Progress -Activity "Extracting certificate from $fileName..." -Id "1" -Status "Writing certificate to $CerOutputFileName" -PercentComplete 100;
		$parentPath = Split-Path $CerOutputFileName -Parent;
		if (-not (Test-Path $parentPath))
		{
			New-Item $parentPath -ItemType Directory | Out-Null;
		}
		
		Move-Item $tempExtract $CerOutputFileName -Force | Out-Null;
	}
}
finally
{
	if (Test-Path $tempExtract)	
	{
		Remove-Item $tempExtract -Force;
	}
}