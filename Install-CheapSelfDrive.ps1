
<#
.SYNOPSIS
    Bootstrap installer for CheapSelfDrive SFTP mount system with automatic dependency management.

.DESCRIPTION
    This is the main installer script for CheapSelfDrive that handles the complete setup process:
    - Installs required dependencies (rclone, WinFsp) via winget
    - Manages password retrieval from Windows Credential Manager
    - Creates rclone configuration for SFTP connection
    - Sets up VFS cache directories
    - Launches the scheduled task installer directly
    
    This script automatically elevates to administrator privileges if needed and ensures
    all dependencies are properly installed before proceeding with the main installation.

.PARAMETER ConfigPath
    Path to the JSON configuration file containing SFTP connection details and mount settings.
    If not specified, uses 'config.json' in the script directory.

.EXAMPLE
    .\Install-CheapSelfDrive.ps1
    Performs complete installation using the default config.json file.

.EXAMPLE
    .\Install-CheapSelfDrive.ps1 -ConfigPath "C:\MyConfig\nas-config.json"
    Performs complete installation using a custom configuration file.

.NOTES
    - Automatically elevates to administrator privileges if needed
    - Password must be stored in Windows Credential Manager before running (use Set-Password.ps1)
    - Requires internet connection to download dependencies via winget
    - Creates rclone remotes for both base SFTP connection and alias for the target path
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

# Check for administrator privileges
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Warn 'Administrator privileges are required. Restarting as administrator...'

    $argumentList = @(
        '-File'
        $MyInvocation.MyCommand.Path
    )
    
    if ($ConfigPath -ne (Join-Path $PSScriptRoot 'config.json')) {
        $argumentList += '-ConfigPath'
        $argumentList += $ConfigPath
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

Write-Host "Cheap Self-Drive Script Installer" -ForegroundColor Cyan

# ===================== Configuration Summary ======================
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

Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host 'Configuration Summary'
Show-ConfigSummary -Config $config

# Step 1: Install dependencies
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host ""
Write-Info "Step 1: Installing required dependencies..."

# Install rclone
Write-Info "Installing rclone..."
try {
    winget install rclone --silent --scope machine --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -eq 0) {
        Write-Ok "rclone installed successfully"
    } elseif ($LASTEXITCODE -eq -1978335189) {
        Write-Info "rclone already installed (or newer version available)"
    } else {
        Write-Warn "rclone installation returned exit code: $LASTEXITCODE"
    }
}
catch {
    Write-Err "Failed to install rclone: $($_.Exception.Message)"
    throw
}

# Install winfsp
Write-Info "Installing winfsp..."
try {
    winget install winfsp --silent --scope machine --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -eq 0) {
        Write-Ok "winfsp installed successfully"
    } elseif ($LASTEXITCODE -eq -1978335189) {
        Write-Info "winfsp already installed (or newer version available)"
    } else {
        Write-Warn "winfsp installation returned exit code: $LASTEXITCODE"
    }
}
catch {
    Write-Err "Failed to install winfsp: $($_.Exception.Message)"
    throw
}

# Refresh PATH to ensure tools are available
$env:PATH = [System.Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH", "User")

Write-Host ""
Write-Info "Step 2: Retrieving the password from Windows Credential Manager..."
# Retrieve password from Windows Credential Manager
$securePassword = Get-SecurePassword -MountName $config.MountName
if (!$securePassword) {
    Write-Host "No password found for mount '$($config.MountName)'. Please set password using Set-SecurePassword function." -BackgroundColor Red
    Read-Host "Press Enter to exit..."
    exit 1
}
# Convert SecureString to plain text for rclone config (temporary, will be cleared)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword))

Write-Host ""
Write-Info "Step 3: Creating rclone configuration..."
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

# Create rclone config directory
$RcloneConfigDir = Split-Path $RcloneConfig -Parent
New-Item -ItemType Directory -Path $RcloneConfigDir -Force | Out-Null

Write-Host "Create VFS Cache Directory"
# Create the folder for the VFS cache to work
New-Item -ItemType Directory -Path $VFSCacheDir -Force | Out-Null

Write-Host "Create rclone base remote"
# Create the rclone configuration file (include shell_type and force unix shell semantics)
rclone config create "${MountName}_base" sftp host "$NASAddress" user "$NASUsername" pass "$PlainPassword" port "$NASPort" shell_type "$ShellType" --config "$RcloneConfig" 

Write-Host "Create rclone alias remote"
# Create the alias remote for mapping the nas path to the root of the future mounted disk
rclone config --config "$RcloneConfig" create "$MountName" alias remote "${MountName}_base:$NASAbsolutePath"

Write-Ok "Main installation completed successfully!"