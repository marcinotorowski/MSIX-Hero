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