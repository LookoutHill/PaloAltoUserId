#!/usr/local/bin/pwsh
[CmdletBinding()]
Param(
	[Parameter(ParameterSetName='Install', Mandatory=$true)]
	[switch]
	$Install,

	[Parameter(ParameterSetName='Remove', Mandatory=$true)]
	[switch]
	$Remove,

	[Parameter(ParameterSetName='Upgrade', Mandatory=$true)]
	[switch]
	$Upgrade,

	[Parameter(ParameterSetName='Install', Mandatory=$true, Position=0)]
	[Parameter(ParameterSetName='Upgrade', Mandatory=$true, Position=0)]
	[string]
	$ProjectPath
)

Set-StrictMode -Version Latest
if(!(Test-Path variable:PSScriptRoot)) { $PSScriptRoot = Split-Path $script:MyInvocation.MyCommand.Path }

function Install-PaloAltoUserId() {
	[CmdletBinding()]
	Param(
		[string]
		$ProjectPath
	)

	Write-Verbose 'Copying binary...'
	cp -fo "$projectPath\PaloAltoUserId\bin\Release\PaloAltoUserId.exe" "$PSScriptRoot\PaloAltoUserId.exe"

	Write-Verbose 'Installing service...'
	& 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe' "$PSScriptRoot\PaloAltoUserId.exe"

	Write-Verbose 'Configuring service for automatic startup...'
	sc.exe config PaloAltoUserId start= delayed-auto
	sc.exe failure PaloAltoUserId reset= 86400 actions= restart/10000

	Write-Verbose 'Starting service...'
	(Get-Service PaloAltoUserId).Start()
}

function Remove-PaloAltoUserId() {
	[CmdletBinding()]
	Param()

	Write-Verbose 'Stop service...'
	$local:srv = (Get-Service PaloAltoUserId)
	if($srv -is [object] -and $srv.CanStop) {
		$srv.Stop()
		$local:proc = Get-Process PaloAltoUserId -ea SilentlyContinue
		while($proc -is [object]) {
			$proc = Get-Process PaloAltoUserId -ea SilentlyContinue
			sleep 1
		}
	}

	Write-Verbose 'Uninstalling service...'
	& 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe' /u "$PSScriptRoot\PaloAltoUserId.exe"
	sleep 3
}

if($PSCmdlet.ParameterSetName -eq 'Install') {
	Write-Verbose "Installing PaloAltoUserId service from $projectPath"
	Install-PaloAltoUserId -ProjectPath $projectPath
} elseif($PSCmdlet.ParameterSetName -eq 'Remove') {
	Write-Verbose 'Removing PaloAltoUserId service'
	Remove-PaloAltoUserId
} elseif($PSCmdlet.ParameterSetName -eq 'Upgrade') {
	Write-Verbose "Upgrading PaloAltoUserId service from $projectPath"
	Remove-PaloAltoUserId
	Install-PaloAltoUserId -ProjectPath $projectPath
}

