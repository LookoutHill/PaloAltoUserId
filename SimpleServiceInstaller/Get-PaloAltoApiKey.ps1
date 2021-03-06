#!/usr/local/bin/pwsh
[CmdletBinding()]
Param(
	[Parameter(Mandatory=$true, Position=0)]
	[string]
	$PaloAltoMgmtAddress,

	[Parameter(Mandatory=$true, Position=1)]
	[string]
	$Username,

	[Parameter(Mandatory=$true, Position=2)]
	[string]
	$Password
)

Set-StrictMode -Version Latest
if(!(Test-Path variable:PSScriptRoot)) { $PSScriptRoot = Split-Path $script:MyInvocation.MyCommand.Path }

Invoke-WebRequest -SkipCertificateCheck -Uri ('https://{0}/api/?type=keygen&user={1}&password={2}' -f $paloAltoMgmtAddress, $username, $password)

