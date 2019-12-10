[CmdletBinding()]
param(
    [String]$CerFileName
)

Import-Certificate -FilePath $CerFileName -CertStoreLocation cert:\LocalMachine\TrustedPeople;	