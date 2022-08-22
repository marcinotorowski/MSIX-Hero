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
    [String]$PublisherFriendlyName,
    [String]$PublisherName = $null,
    [String]$Password,
    [String]$OutputDirectory,
    [String]$PfxOutputFileName = $null,
    [String]$CerOutputFileName = $null,
    [switch]$CreatePasswordFile,
    [System.DateTime]$NotAfter
)

Write-Progress -Activity "Generating certificate for $PublisherFriendlyName…" -Id "1" -Status "Generating certificate" -PercentComplete 10;

$certificate = New-SelfSignedCertificate -NotAfter $NotAfter -Type Custom -KeyUsage DigitalSignature -Subject $PublisherName -FriendlyName $PublisherFriendlyName -CertStoreLocation 'Cert:\CurrentUser\my';
$securePassword = ConvertTo-SecureString -String $Password -Force -AsPlainText;

[String]$pfxFullPath;
[String]$cerFullPath;

if ([String]::IsNullOrEmpty($PfxOutputFileName)) {
    $pfxFullPath = (Join-Path $OutputDirectory $PublisherFriendlyName) + ".pfx";
}
else {
    $pfxFullPath = (Join-Path $OutputDirectory $PfxOutputFileName) + ".pfx";
}

if ([String]::IsNullOrEmpty($CerOutputFileName)) {
    $cerFullPath = (Join-Path $OutputDirectory $PublisherFriendlyName) + ".cer";
}
else {
    $cerFullPath = (Join-Path $OutputDirectory $CerOutputFileName) + ".cer";
}

if (-not (Test-Path $OutputDirectory))
{
    New-Item -Path $OutputDirectory -ItemType Directory | Out-Null;
}

Write-Progress -Activity "Generating certificate for $PublisherFriendlyName…" -Id "1" -Status "Exporting private key" -PercentComplete 70;
$certificate | Export-PfxCertificate -FilePath $pfxFullPath -Password $securePassword | Out-Null;

Write-Progress -Activity "Generating certificate for $PublisherFriendlyName…" -Id "1" -Status "Exporting certificate" -PercentComplete 80;
$certificate | Export-Certificate -Type Cert -FilePath $cerFullPath | Out-Null;

Write-Output $pfxFullPath;
Write-Output $cerFullPath;

if ($CreatePasswordFile)
{
    Write-Progress -Activity "Generating certificate for $PublisherFriendlyName…" -Id "1" -Status "Creating password file" -PercentComplete 90;
    $Password | Out-File -FilePath ($pfxFullPath + ".pwd");
    Write-Output ($pfxFullPath + ".pwd");
}

Write-Progress -Activity "Generating certificate for $PublisherFriendlyName…" -Id "1" -Status "Cleaning up" -PercentComplete 95;
Remove-Item $certificate.PSPath;