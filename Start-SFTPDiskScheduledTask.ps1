<#
Start-SFTPDiskScheduledTask.ps1

Minimal starter: simply starts the scheduled task that mounts the rclone SFTP drive.
Usage:
    powershell -File .\Start-SFTPDiskScheduledTask.ps1 [-ConfigPath path]
Examples:
    powershell -File .\Start-SFTPDiskScheduledTask.ps1
    powershell -File .\Start-SFTPDiskScheduledTask.ps1 -ConfigPath "C:\MyConfigs\nas.json"
#>

param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot 'config.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Check for administrator privileges
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host 'Administrator privileges are required. Restarting as administrator...'

    $argumentList = @()
    $argumentList += '-File'
    $argumentList += "`"$($MyInvocation.MyCommand.Path)`""
    
    if ($ConfigPath -ne (Join-Path $PSScriptRoot 'config.json')) {
        $argumentList += '-ConfigPath'
        $argumentList += "`"$ConfigPath`""
    }
    
    try {
        Start-Process -FilePath 'powershell.exe' -ArgumentList $argumentList -Verb RunAs -Wait
        exit $LASTEXITCODE
    }
    catch {
        Write-Host "Failed to restart as administrator: $($_.Exception.Message)"
        Write-Host 'Please manually run this script in an elevated PowerShell session.'
        exit 1
    }
}

# Load configuration helper
. (Join-Path $PSScriptRoot 'ConfigHelper.ps1')

# Load configuration
try {
    $config = Get-SFTPDiskConfig -ConfigPath $ConfigPath
}
catch {
    Write-Host "Configuration Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$taskName = $config.TaskName
Write-Host "Starting scheduled task: $taskName" -ForegroundColor Cyan

$task = Get-ScheduledTask -TaskName $taskName
if (-not $task) {
    Write-Host "Task '$taskName' not found. Run Install-SFTPDiskScheduledTask.ps1 first." -ForegroundColor Red
    exit 2
}

try {
    Start-ScheduledTask -TaskName $taskName
    Write-Host 'Start request sent.' -ForegroundColor Green
    exit 0
} catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 3
}