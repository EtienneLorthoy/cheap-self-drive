
<#
.SYNOPSIS
    Installs and configures an rclone SFTP mount as a Windows Scheduled Task.

.DESCRIPTION
    This script sets up an automated SFTP drive mount using rclone and Windows Task Scheduler.
    It performs the following operations:

    1. Loads configuration from a JSON file (config.json by default)
    2. Installs required dependencies (rclone and WinFsp via winget)
    3. Creates necessary directories for VFS caching and logs
    4. Configures rclone remotes for SFTP connection
    5. Generates a mount script from template with concrete configuration values
    6. Creates a Windows Scheduled Task that runs at user logon
    7. Sets up retry policies and execution limits for reliability

    The resulting scheduled task will automatically mount the SFTP share as a network drive
    whenever the user logs in, with full VFS caching for improved performance.

.PARAMETER ConfigPath
    Path to the JSON configuration file. Defaults to 'config.json' in the script directory.
    The configuration file should contain SFTP connection details, mount settings, and paths.

.EXAMPLE
    .\Install-SFTPDiskScheduledTask.ps1
    Installs using the default config.json file in the script directory.

.EXAMPLE
    .\Install-SFTPDiskScheduledTask.ps1 -ConfigPath "C:\MyConfigs\production.json"
    Installs using a custom configuration file.

.NOTES
    - Requires Administrator privileges to create scheduled tasks and install software
    - Depends on winget for installing rclone and WinFsp
    - Creates a mount script in C:\VFS\ that the scheduled task will execute
    - The scheduled task runs with the current user's credentials
    - Supports retry policies: up to 30 retries with 1-minute intervals

.LINK
    https://rclone.org/
    https://github.com/winfsp/winfsp
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
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Warn 'Administrator privileges are required. Restarting as administrator...'

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
        Write-Err "Failed to restart as administrator: $($_.Exception.Message)"
        Write-Err 'Please manually run this script in an elevated PowerShell session.'
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

# Validate drive letter availability
$driveLetterOnly = $config.DriveLetter.TrimEnd(':')
if (Get-PSDrive -Name $driveLetterOnly -ErrorAction SilentlyContinue) {
    Write-Host "ERROR: Drive letter $($config.DriveLetter) is already in use. Please choose a different drive letter in config.json" -ForegroundColor Red
    exit 1
}

# Extract variables from config
$MountName = $config.MountName
$DriveLetter = $config.DriveLetter
$NASAddress = $config.NASAddress
$NASUsername = $config.NASUsername
$NASPassword = $config.NASPassword
$NASPort = $config.NASPort
$NASAbsolutePath = $config.NASAbsolutePath
$VFSCacheDir = $config.VFSCacheDir
$RcloneLogs = $config.RcloneLogs
$RcloneConfig = $config.RcloneConfig
$ShellType = $config.ShellType
$MountScriptPath = $config.MountScriptPath
$TaskName = $config.TaskName

# Helper: uniform section banner (similar style to existing config summary line)
function Write-Section {
    param([Parameter(Mandatory)][string]$Title)
    $pad = '=' * 8
    Write-Host ("$pad $Title $pad") -ForegroundColor Cyan
}

# Create rclone config directory
$RcloneConfigDir = Split-Path $RcloneConfig -Parent
New-Item -ItemType Directory -Path $RcloneConfigDir -Force | Out-Null

# ===================== Configuration Summary ======================
Write-Host ""
Write-Section 'Configuration Summary'
Show-ConfigSummary -Config $config

Write-Host "=======================================================" -ForegroundColor Cyan
$proceed = Read-Host "Proceed with installation and mounting steps? (Y/N)"
if ($proceed -notin @('Y','y')) {
    Write-Host "Aborted by user."
    return
}
Write-Host ""

Write-Section 'Install Dependencies'
winget install rclone --silent
winget install winfsp --silent

Write-Section 'Create VFS Cache Directory'
# Create the folder for the VFS cache to work
New-Item -ItemType Directory -Path $VFSCacheDir -Force | Out-Null

Write-Section 'Create rclone base remote'
# Create the rclone configuration file (include shell_type and force unix shell semantics)
rclone config create "${MountName}_base" sftp host "$NASAddress" user "$NASUsername" pass "$NASPassword" port "$NASPort" shell_type "$ShellType" --config "$RcloneConfig"

Write-Section 'Create rclone alias remote'
# Create the alias remote for mapping the nas path to the root of the future mounted disk
rclone config --config "$RcloneConfig" create "$MountName" alias remote "${MountName}_base:$NASAbsolutePath"

Write-Section 'Generate Mount Script'
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

Write-Section 'Register Scheduled Task'
# Scheduled Task will now call the generated mount script directly so the logic lives outside the task definition.
$description = "Mount ${MountName}: to ${DriveLetter} using rclone with caching options"

# Action: run PowerShell executing the mount script file
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-WindowStyle Hidden -File `"$MountScriptPath`""
$trigger     = New-ScheduledTaskTrigger -AtLogOn
$currentUser = "$env:USERDOMAIN\$env:USERNAME"
$principal   = New-ScheduledTaskPrincipal -UserId $currentUser -LogonType Interactive

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

Write-Host ""
Write-Ok "Installation completed successfully!"
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

