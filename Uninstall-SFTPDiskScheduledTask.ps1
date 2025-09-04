<#
Uninstall-SFTPDiskScheduledTask.ps1

Simple uninstall script for rclone SFTP mount created by Install-SFTPDiskScheduledTask.ps1.

Parameters:
    -ConfigPath: Path to the JSON configuration file (default: config.json in script directory)

Examples:
    powershell -File .\Uninstall-SFTPDiskScheduledTask.ps1
    powershell -File .\Uninstall-SFTPDiskScheduledTask.ps1 -ConfigPath "C:\MyConfigs\nas.json"
#>

param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot 'config.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Info { param([string]$m) Write-Host $m -ForegroundColor Cyan }
function Write-Warn { param([string]$m) Write-Host $m -ForegroundColor Yellow }
function Write-Err { param([string]$m) Write-Host $m -ForegroundColor Red }
function Write-Ok { param([string]$m) Write-Host $m -ForegroundColor Green }

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
    Write-Err "Configuration Error: $($_.Exception.Message)"
    exit 1
}

# Extract variables from config
$MountName = $config.MountName
$DriveLetter = $config.DriveLetter
$taskName = $config.TaskName
$cacheDir = $config.VFSCacheDir
$logFile = $config.RcloneLogs
$mountScriptPath = $config.MountScriptPath
$rcloneConfig = $config.RcloneConfig
$driveLetterOnly = $DriveLetter.TrimEnd(':')

Write-Info "=== Uninstall rclone SFTP mount: $MountName ($DriveLetter) ==="

try {
    # 1. Stop scheduled task if present
    $task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
    if ($task) {
        Write-Info "Stopping scheduled task '$taskName'..."
        Stop-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue | Out-Null
        Write-Info "Unregistering scheduled task '$taskName'..."
        Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
        Write-Ok  "Scheduled task removed."
    }
    else {
        Write-Warn "Scheduled task '$taskName' not found."
    }

    # 2. Find and terminate rclone processes (using Get-Process as requested)
    Write-Info 'Scanning for rclone processes (Get-Process)...'
    $allRclone = @(Get-Process -Name rclone -ErrorAction SilentlyContinue)
    $candidateProcesses = @()
    foreach ($p in $allRclone) {
        $wmi = Get-WmiObject Win32_Process -Filter "ProcessId=$($p.Id)" -ErrorAction SilentlyContinue
        $cmdLine = $wmi.CommandLine
        $matches = $false
        if ($cmdLine) {
            if ($cmdLine -match [Regex]::Escape("mount ${MountName}:")) { $matches = $true }
            elseif ($cmdLine -match "\b$($driveLetterOnly):?\b") { $matches = $true }
        } else {
            # No command line accessible; include conservatively
            $matches = $true
        }
        if ($matches) {
            $candidateProcesses += [pscustomobject]@{
                ProcessId   = $p.Id
                Name        = $p.Name
                CommandLine = $cmdLine
            }
        }
    }
    Write-Info "Filtered to $($candidateProcesses.Count) matching rclone process(es)."

    if ($candidateProcesses.Count -gt 0) {
        foreach ($p in $candidateProcesses) {
            $cl = if ($p.CommandLine) { $p.CommandLine } else { '<no-cmdline-available>' }
            Write-Info "Stopping rclone PID $($p.ProcessId) -> $cl"
            Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
        }
        Start-Sleep -Seconds 2
    }
    else {
        Write-Warn 'No matching rclone mount process found.'
    }

    # 3. Attempt to remove PSDrive if still listed
    $psd = Get-PSDrive -Name $driveLetterOnly -ErrorAction SilentlyContinue
    if ($psd) {
        Write-Info "Removing PSDrive $DriveLetter..."
        Remove-PSDrive -Name $driveLetterOnly -Force -ErrorAction SilentlyContinue
    }

    # 4. Clean up cache directory
    if (Test-Path $cacheDir) {
        Write-Info "Removing cache directory $cacheDir"
        Remove-Item -Path $cacheDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    else { Write-Warn "Cache directory not found: $cacheDir" }

    # 5. Remove log file
    if (Test-Path $logFile) {
        Write-Info "Removing log file $logFile"
        Remove-Item -Path $logFile -Force -ErrorAction SilentlyContinue
    }

    # 6. Remove generated mount script
    if (Test-Path $mountScriptPath) {
        Write-Info "Removing mount script $mountScriptPath"
        Remove-Item -Path $mountScriptPath -Force -ErrorAction SilentlyContinue
    }

    # 7. Remove rclone remotes via rclone CLI (alias and base)
    foreach ($remote in @("${MountName}_base", $MountName)) {
        Write-Info "Deleting rclone remote '$remote'..."
        & rclone config delete $remote --config $rcloneConfig --auto-confirm 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Ok "Deleted remote '$remote'."
        }
        else {
            Write-Warn "Remote '$remote' not deleted (may not exist). Exit code $LASTEXITCODE."
        }
    }

    Write-Ok "Uninstall completed for $MountName."
    Write-Ok "Installation completed successfully!"
    Write-Host "Press any key to continue..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
catch {
    Write-Err $_.Exception.Message
    exit 1
}
