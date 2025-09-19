<#
.SYNOPSIS
    Bootstrap installer for SFTP Disk Scheduled Task setup.

.DESCRIPTION
    This script serves as a bootstrap installer that:
    1. Installs PStools (including PsExec) via winget
    2. Launches the main installation script with SYSTEM account privileges using PsExec
    
    This approach ensures the scheduled task is properly created with SYSTEM privileges
    and can run reliably during system startup and user logon scenarios.

.PARAMETER ConfigPath
    Path to the JSON configuration file to pass to the main installation script.
    Defaults to 'config.json' in the script directory.

.EXAMPLE
    .\Bootstrap-Install.ps1
    Runs the bootstrap installation with default config.json

.EXAMPLE
    .\Bootstrap-Install.ps1 -ConfigPath "C:\MyConfigs\production.json"
    Runs the bootstrap installation with a custom configuration file.

.NOTES
    - Requires Administrator privileges to install software and use PsExec
    - Installs PStools via winget if not already available
    - Uses PsExec to run the main installation script as SYSTEM account
    - The -s flag runs as SYSTEM, -i flag allows interaction with desktop

.LINK
    https://docs.microsoft.com/en-us/sysinternals/downloads/psexec
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

Write-Host "Installer Script Starting..." -ForegroundColor Cyan

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
Write-Host ""
Write-Host 'Configuration Summary'
Show-ConfigSummary -Config $config

Write-Host "=======================================================" -ForegroundColor Cyan
$proceed = Read-Host "Proceed with installation and mounting steps? (Y/N)"
if ($proceed -notin @('Y','y')) {
    Write-Host "Aborted by user."
    # pause for 3 seconds before exit
    Start-Sleep -Seconds 10
}
Write-Host ""

Write-Host ""
Write-Info "Bootstrap Installer for SFTP Disk Scheduled Task"
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Install dependencies
Write-Info "Step 1: Installing required dependencies..."

# Install PStools (includes PsExec)
Write-Info "Installing PStools (includes PsExec)..."
try {
    winget install Microsoft.Sysinternals.PSTools --silent --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -eq 0) {
        Write-Ok "PStools installed successfully"
    } elseif ($LASTEXITCODE -eq -1978335189) {
        Write-Info "PStools already installed (or newer version available)"
    } else {
        Write-Warn "PStools installation returned exit code: $LASTEXITCODE"
    }
}
catch {
    Write-Err "Failed to install PStools: $($_.Exception.Message)"
    throw
}

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

# Refresh PATH to ensure PsExec is available
$env:PATH = [System.Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH", "User")

# Step 2: Verify PsExec is available
Write-Info "Step 2: Verifying PsExec availability..."
try {
    $psexecPath = Get-Command "PsExec.exe" -ErrorAction SilentlyContinue
    if (-not $psexecPath) {
        # Try common installation paths
        $commonPaths = @(
            "${env:ProgramFiles}\Sysinternals\PsExec.exe",
            "${env:LOCALAPPDATA}\Microsoft\WindowsApps\PsExec.exe",
            "C:\PsTools\PsExec.exe"
        )
        
        foreach ($path in $commonPaths) {
            if (Test-Path $path) {
                $psexecPath = $path
                break
            }
        }
        
        if (-not $psexecPath) {
            throw "PsExec.exe not found in PATH or common installation directories"
        }
    } else {
        $psexecPath = $psexecPath.Source
    }
    
    Write-Ok "PsExec found at: $psexecPath"
}
catch {
    Write-Err "PsExec verification failed: $($_.Exception.Message)"
    Write-Err "Please ensure PStools is properly installed and PsExec.exe is accessible"
    throw
}

# Step 3: Prepare main installation script path and arguments
$mainScriptPath = Join-Path $PSScriptRoot 'Install-SFTPDiskScheduledTask.ps1'
if (-not (Test-Path $mainScriptPath)) {
    throw "Main installation script not found at: $mainScriptPath"
}

Write-Info "Step 3: Launching main installation script with SYSTEM privileges..."
Write-Host "Main script: $mainScriptPath" -ForegroundColor Gray
Write-Host "Config file: $ConfigPath" -ForegroundColor Gray
Write-Host ""

# Prepare arguments for the main script
$scriptArgs = "-File `"$mainScriptPath`""
if ($ConfigPath -ne (Join-Path $PSScriptRoot 'config.json')) {
    $scriptArgs += " -ConfigPath `"$ConfigPath`""
}

# Build PsExec command
# -s: Run as SYSTEM account
# -i: Allow interaction with desktop (required for the installation prompts)
# -d: Don't wait for process to terminate (we want to see the output)
$psexecArgs = @(
    '-s',
    '-i',
    'powershell.exe',
    $scriptArgs
)

try {
    Write-Info "Executing: PsExec $($psexecArgs -join ' ')"
    Write-Host ""
    
    # Start the process and wait for completion
    $process = Start-Process -FilePath $psexecPath -ArgumentList $psexecArgs -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host ""
        Write-Ok "Main installation completed successfully!"
    } else {
        Write-Host ""
        Write-Warn "Main installation exited with code: $($process.ExitCode)"
    }
    
    exit $process.ExitCode
}
catch {
    Write-Err "Failed to execute main installation script: $($_.Exception.Message)"
    throw
}