<#
.SYNOPSIS
    Creates a Windows Scheduled Task to automatically mount an SFTP drive using rclone.

.DESCRIPTION
    This script configures and registers a Windows Scheduled Task that will automatically mount 
    an SFTP drive at user logon using rclone with VFS caching. The script performs the following:
    - Generates a mount script from template with configuration values
    - Creates a scheduled task that runs at logon with SYSTEM privileges
    - Configures retry policies for reliable mounting
    - Sets up the task to run hidden in the background

.PARAMETER ConfigPath
    Path to the JSON configuration file containing SFTP connection details and mount settings.
    If not specified, uses 'config.json' in the script directory.

.EXAMPLE
    .\Install-SFTPDiskScheduledTask.ps1
    Installs the scheduled task using the default config.json file.

.EXAMPLE
    .\Install-SFTPDiskScheduledTask.ps1 -ConfigPath "C:\MyConfig\nas-config.json"
    Installs the scheduled task using a custom configuration file.

.NOTES
    - Requires administrator privileges to create scheduled tasks
    - Password must be set separately using Manage-Passwords.ps1 before running this script
    - The generated mount script will be created in C:\VFS\Mount-{MountName}-SFTPDisk.ps1
    - Task will retry up to 30 times with 1-minute intervals on failure
#>

param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot 'config.json')
)

function Write-Info { param([string]$m) Write-Host $m -ForegroundColor Cyan }
function Write-Warn { param([string]$m) Write-Host $m -ForegroundColor Yellow }
function Write-Err { param([string]$m) Write-Host $m -ForegroundColor Red }
function Write-Ok { param([string]$m) Write-Host $m -ForegroundColor Green }

# Fail fast & strict mode
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Global trap to ensure we exit on any unhandled error
trap {
    throw "ERROR: $($_.Exception.Message)"
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

# Extract variables from config
$MountName = $config.MountName
$DriveLetter = $config.DriveLetter
$NASAddress = $config.NASAddress
$NASUsername = $config.NASUsername
$NASPort = $config.NASPort
$NASAbsolutePath = $config.NASAbsolutePath
$VFSCacheDir = $config.VFSCacheDir
$RcloneLogs = $config.RcloneLogs
$RcloneConfig = $config.RcloneConfig
$ShellType = $config.ShellType
$MountScriptPath = $config.MountScriptPath
$TaskName = $config.TaskName

Write-Host "======== Generate Mount Script ========" -ForegroundColor Cyan
# Path for the generated mount script invoked by the Scheduled Task
$TemplatePath    = Join-Path $PSScriptRoot 'Mount-TEMPLATE-SFTPDisk.ps1'
if (-not (Test-Path $TemplatePath)) { throw "Template mount script not found at $TemplatePath" }

# Load template and replace variable assignment lines with concrete values
$template = Get-Content -Path $TemplatePath -Raw
$processed = $template `
    -replace '(?m)^\$MountName\s*=.*$', "`$MountName    = '$MountName'" `
    -replace '(?m)^\$DriveLetter\s*=.*$', "`$DriveLetter  = '$DriveLetter'" `
    -replace '(?m)^\$VFSCacheDir\s*=.*$', "`$VFSCacheDir  = '$VFSCacheDir'" `
    -replace '(?m)^\$RcloneLogs\s*=.*$', "`$RcloneLogs   = '$RcloneLogs'" `
    -replace '(?m)^\$RcloneConfig\s*=.*$', "`$RcloneConfig = '$RcloneConfig'"

Set-Content -Path $MountScriptPath -Value $processed -Encoding UTF8
Write-Host "Mount script (from template) created at $MountScriptPath"

Write-Host "======== Register Scheduled Task ========" -ForegroundColor Cyan
# Scheduled Task will now call the generated mount script directly so the logic lives outside the task definition.
$description = "Mount ${MountName}: to ${DriveLetter} using rclone with caching options"

# Action: run PowerShell executing the mount script file
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-WindowStyle Hidden -File `"$MountScriptPath`""
$trigger     = New-ScheduledTaskTrigger -AtStartup
$principal   = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest 

# Retry policy: retry up to 30 times, every 1 minute if the task exits non-success.
$retryCount    = 30
$retryInterval = New-TimeSpan -Minutes 1
$executionTimeLimit = New-TimeSpan -Days 365

# Settings include: allow on battery, don't stop on battery change, ignore new if already running, start if missed, and restart policy.
$settings  = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -MultipleInstances IgnoreNew -StartWhenAvailable -RestartCount $retryCount -RestartInterval $retryInterval -ExecutionTimeLimit $executionTimeLimit -Hidden

if (Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
}

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description $description

Write-Host "Scheduled Task '$TaskName' created/updated with cache dir $VFSCacheDir."