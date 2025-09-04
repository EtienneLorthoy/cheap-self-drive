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

function Msg { param([string]$t,[string]$c='White'); Write-Host $t -ForegroundColor $c }

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

$task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
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